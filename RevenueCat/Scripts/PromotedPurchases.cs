using System;
using System.Collections.Generic;
using System.Threading;
using RevenueCat.SimpleJSON;
using UnityEngine;
using UnityEngine.Scripting;

/// <summary>Optional native bridge for promoted App Store purchases.</summary>
public interface IPromotedPurchasesWrapper
{
    void MakeDeferredPurchase(string callbackID, string gameObject);
}

public partial class Purchases
{
    /// <summary>Receives a function that starts a promoted purchase when the app is ready.</summary>
    public delegate void ShouldPurchasePromoProductListener(Action<MakePurchaseFunc> deferredPurchase);

    private readonly List<ShouldPurchasePromoProductListener> _promotedPurchaseListeners =
        new List<ShouldPurchasePromoProductListener>();
    private readonly Dictionary<string, Action<MakePurchaseFunc>> _pendingPromotedPurchases =
        new Dictionary<string, Action<MakePurchaseFunc>>();
    private readonly Dictionary<string, MakePurchaseFunc> _deferredPurchaseCallbacks =
        new Dictionary<string, MakePurchaseFunc>();

    /// <summary>
    /// iOS only. Register/remove listeners and invoke deferredPurchase on Unity's main thread. Replays pending purchases received before registration.
    /// Save deferredPurchase until onboarding, login or parental approval is complete. The purchase starts only
    /// when it is called. Each purchase can be started once across all listeners, even after cancellation or failure.
    /// Pending purchases live in memory and replay when a replacement component configures the SDK.
    /// Keep this component alive until a started purchase completes. Android and Editor produce no events.
    /// </summary>
    public void AddShouldPurchasePromoProductListener(ShouldPurchasePromoProductListener promoListener)
    {
        if (promoListener == null) throw new ArgumentNullException(nameof(promoListener));
        if (_promotedPurchaseListeners.Contains(promoListener)) return;
        _promotedPurchaseListeners.Add(promoListener);
        foreach (var callbackID in new List<string>(_pendingPromotedPurchases.Keys))
        {
            if (!_promotedPurchaseListeners.Contains(promoListener)) break;
            if (_pendingPromotedPurchases.TryGetValue(callbackID, out var purchase))
                NotifyPromotedPurchaseListener(promoListener, purchase);
        }
    }

    /// <summary>Removes a listener. Previously delivered deferredPurchase functions remain valid.</summary>
    public bool RemoveShouldPurchasePromoProductListener(ShouldPurchasePromoProductListener promoListener)
    {
        return _promotedPurchaseListeners.Remove(promoListener);
    }

    [Preserve]
    private void _receivePromotedPurchase(string json)
    {
        var callbackID = JSON.Parse(json)["callbackID"].Value;
        if (_pendingPromotedPurchases.ContainsKey(callbackID) || _deferredPurchaseCallbacks.ContainsKey(callbackID)) return;
        var started = 0;
        Action<MakePurchaseFunc> deferredPurchase = callback =>
        {
            if (this == null) throw new InvalidOperationException("The Purchases component has been destroyed.");
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (Interlocked.Exchange(ref started, 1) != 0)
                throw new InvalidOperationException("This promoted purchase has already been started.");
            _pendingPromotedPurchases.Remove(callbackID);
            _deferredPurchaseCallbacks.Add(callbackID, callback);
            try
            {
                if (!(_wrapper is IPromotedPurchasesWrapper wrapper))
                    throw new NotSupportedException("Promoted App Store purchases are only available on iOS.");
                wrapper.MakeDeferredPurchase(callbackID, gameObject.name);
            }
            catch
            {
                _deferredPurchaseCallbacks.Remove(callbackID);
                throw;
            }
        };
        _pendingPromotedPurchases.Add(callbackID, deferredPurchase);
        foreach (var promoListener in _promotedPurchaseListeners.ToArray())
        {
            NotifyPromotedPurchaseListener(promoListener, deferredPurchase);
        }
    }

    private static void NotifyPromotedPurchaseListener(ShouldPurchasePromoProductListener promoListener,
        Action<MakePurchaseFunc> deferredPurchase)
    {
        try { promoListener(deferredPurchase); }
        catch (Exception exception) { Debug.LogException(exception); }
    }

    [Preserve]
    private void _makeDeferredPurchase(string json)
    {
        var response = JSON.Parse(json);
        var callbackID = response["callbackID"].Value;
        if (!_deferredPurchaseCallbacks.TryGetValue(callbackID, out var callback)) return;
        _deferredPurchaseCallbacks.Remove(callbackID);
        callback(new PurchaseResult(response));
    }
}
