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

        public AndroidPurchasesCallback(CancellationToken cancellationToken = default) : base("com.revenuecat.purchases.unity.PurchasesWrapper$Callback")
        {
            _tcs = new TaskCompletionSource<TReturnType>();

            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(() =>
                {
                    _tcs.TrySetCanceled();
                });
            }
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onCompleted(string json)
        {
            var data = JsonConvert.DeserializeObject<TReturnType>(json);
            _tcs.SetResult(data);
        }

        // ReSharper disable once InconsistentNaming
        // matches name of Java method
        [UsedImplicitly]
        public void onError(string json)
        {
            // deserialize json
            var error = JsonConvert.DeserializeObject<Error>(json);
            _tcs.SetException(new Exception(error.ToString()));
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
        public void onReceived(string json)
        {
            var customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(json, PurchasesSdk.JsonSerializerSettings);
            _action?.Invoke(customerInfo);
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
        public void onLog(string json)
        {
            var logMessage = JsonConvert.DeserializeObject<RevenueCatLogMessage>(json, PurchasesSdk.JsonSerializerSettings);
            _action?.Invoke(logMessage);
        }
    }
}
#endif // UNITY_ANDROID