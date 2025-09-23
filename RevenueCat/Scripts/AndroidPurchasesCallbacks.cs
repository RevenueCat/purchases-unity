#if UNITY_ANDROID
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat
{
    internal class AndroidPurchasesCallback<TReturnType> : AndroidJavaProxy
    {
        private TaskCompletionSource<TReturnType> _tcs;

        public AndroidPurchasesCallback(CancellationToken cancellationToken) : base("com.revenuecat.purchases.unity.PurchasesWrapper$Callback")
        {
            _tcs = new TaskCompletionSource<TReturnType>();

            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(() => { _tcs.TrySetCanceled(); });
            }
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onReceived(string json)
        {
            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<TReturnType>(json);
                    _tcs.SetResult(data);
                }
                catch (Exception e)
                {
                    _tcs.SetException(e);
                }
            }, null);
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onError(string json)
        {
            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<Error>(json);
                    _tcs.SetException(new Exception(error.ToString()));
                }
                catch (Exception e)
                {
                    _tcs.SetException(e);
                }
            }, null);
        }

        public Task<TReturnType> Task => _tcs.Task;
    }

    internal class CustomerInfoHandler : AndroidJavaProxy
    {
        private Action<CustomerInfo> _action;

        public CustomerInfoHandler(Action<CustomerInfo> action) : base("com.revenuecat.purchases.unity.PurchasesWrapper$CustomerInfoHandler")
        {
            _action = action;
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onCustomerReceived(string json)
        {
            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
            {
                try
                {
                    var customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(json, PurchasesSdk.JsonSerializerSettings);
                    _action?.Invoke(customerInfo);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, null);
        }

    }

    internal class LogHandler : AndroidJavaProxy
    {
        private Action<RevenueCatLogMessage> _action;

        public LogHandler(Action<RevenueCatLogMessage> action) : base("com.revenuecat.purchases.unity.PurchasesWrapper$LogHandler")
        {
            _action = action;
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onLogReceived(string json)
        {
            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
            {
                try
                {
                    var logMessage = JsonConvert.DeserializeObject<RevenueCatLogMessage>(json, PurchasesSdk.JsonSerializerSettings);
                    _action?.Invoke(logMessage);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, null);
        }
    }
}
#endif // UNITY_ANDROID