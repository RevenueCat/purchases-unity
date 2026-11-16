#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using RevenueCatUI;
using RevenueCatUI.Internal;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class CheckpointTests
    {
        private sealed class QueuedContext : SynchronizationContext
        {
            private readonly Queue<Action> _queue = new Queue<Action>();
            public override void Post(SendOrPostCallback callback, object state) => _queue.Enqueue(() => callback(state));
            internal void Drain() { while (_queue.Count > 0) _queue.Dequeue()(); }
        }

        private readonly List<string> _native = new List<string>();
        private Action<string, string, string> _previousSendToNative;
        private SynchronizationContext _previousContext;
        private QueuedContext _context;

        private readonly List<string> _starts = new List<string>();
        private Action<string, CheckpointParams, string, bool> _previousStartNative;
        private string _lastCallId;

        private const string PaywallParams = "{\"checkpointIdentifier\":\"upgrade\"," +
            "\"customVariables\":{\"name\":\"Taylor\",\"level\":12,\"premium\":true}," +
            "\"presentationMode\":\"MODAL_SHEET\"," +
            "\"offering\":{\"identifier\":\"pro\",\"serverDescription\":\"Pro\",\"metadata\":{},\"availablePackages\":[]}}";

        private Action Start(CheckpointPaywallPresenter presenter = null, bool native = false)
        {
            return CheckpointBridge.Start("upgrade", _ => { },
                new CheckpointParams(paywallPresenter: presenter, useNativePaywallPresenter: native));
        }

        [SetUp]
        public void SetUp()
        {
            _native.Clear();
            _starts.Clear();
            _previousSendToNative = CheckpointBridge.SendToNative;
            CheckpointBridge.SendToNative = (method, first, second) => _native.Add($"{method}({first}, {second ?? "null"})");
            _previousStartNative = CheckpointBridge.StartNative;
            CheckpointBridge.StartNative = (identifier, parameters, callId, custom) =>
            {
                _lastCallId = callId;
                _starts.Add($"{identifier}, custom presenter: {custom}");
            };
            _previousContext = SynchronizationContext.Current;
            _context = new QueuedContext();
            SynchronizationContext.SetSynchronizationContext(_context);
        }

        [TearDown]
        public void TearDown()
        {
            Checkpoints.SetPaywallPresenter(null);
            CheckpointBridge.StartNative = _previousStartNative;
            CheckpointBridge.SendToNative = _previousSendToNative;
            SynchronizationContext.SetSynchronizationContext(_previousContext);
        }

        private static JSONNode Result(bool withFlow = false)
        {
            var root = Path.Combine(Application.dataPath, "Tests", "EditMode", "JsonModelFixtures");
            var customer = JSON.Parse(File.ReadAllText(Path.Combine(root, "customer-info-full.json")));
            var result = new JSONObject();
            result["customer"] = customer;
            result["flow"] = JSONNull.CreateOrGet();
            if (withFlow)
            {
                var flow = new JSONObject();
                var entitlements = new JSONArray();
                var entitlement = new JSONObject();
                entitlement["entitlementInfo"] = customer["entitlements"]["all"]["premium"];
                entitlements.Add(entitlement);
                flow["obtainedEntitlements"] = entitlements;
                var products = new JSONArray();
                var product = new JSONObject();
                product["product"] = JSON.Parse(File.ReadAllText(Path.Combine(root, "store-product-known-category.json")));
                products.Add(product);
                flow["obtainedProducts"] = products;
                result["flow"] = flow;
            }
            return result;
        }

        [Test]
        public void PassWithoutFlowIncludesCustomer()
        {
            var result = new CheckpointResult(Result());
            Assert.That(result.Flow, Is.Null);
            Assert.That(result.Customer.OriginalAppUserId, Is.Not.Empty);
        }

        [Test]
        public void FlowParsesObtainedEntitlementsAndProducts()
        {
            var result = new CheckpointResult(Result(true));
            Assert.That(result.Flow.ObtainedEntitlements.Count, Is.EqualTo(1));
            Assert.That(result.Flow.ObtainedEntitlements[0].EntitlementInfo.Identifier, Is.EqualTo("premium"));
            Assert.That(result.Flow.ObtainedProducts.Count, Is.EqualTo(1));
            Assert.That(result.Flow.ObtainedProducts[0].Product.Identifier, Is.Not.Empty);
        }

        [Test]
        public void ParametersPreserveNativeVariableTypesAndPresentationMode()
        {
            var parameters = new CheckpointParams(new Dictionary<string, CustomVariableValue>
            {
                { "source", CustomVariableValue.String("unity") },
                { "level", CustomVariableValue.Number(42) },
                { "premium", CustomVariableValue.Boolean(true) }
            }, FlowPresentationMode.ModalFullScreen);
            var json = JSON.Parse(parameters.CustomVariablesToJsonString());
            Assert.That(json["source"].IsString, Is.True);
            Assert.That(json["level"].AsDouble, Is.EqualTo(42));
            Assert.That(json["premium"].IsBoolean, Is.True);
            Assert.That(parameters.NativePresentationMode, Is.EqualTo("MODAL_FULL_SCREEN"));
            Assert.That(new CheckpointParams(presentationMode: FlowPresentationMode.ModalSheet).NativePresentationMode, Is.EqualTo("MODAL_SHEET"));
            Assert.That(new CheckpointParams().NativePresentationMode, Is.EqualTo("DEFAULT"));
        }

        [Test]
        public void CallbacksAreQueuedRoutedAndInvokedOnlyOnce()
        {
            var previous = SynchronizationContext.Current;
            var context = new QueuedContext();
            SynchronizationContext.SetSynchronizationContext(context);
            var calls = 0;
            var first = Guid.NewGuid().ToString();
            var second = Guid.NewGuid().ToString();
            var removeFirst = CheckpointBridge.Register(first, _ => calls++);
            var removeSecond = CheckpointBridge.Register(second, _ => calls += 100);
            try
            {
                var json = Result().ToString();
                CheckpointBridge.OnPassed(first, json);
                CheckpointBridge.OnPassed(first, json);
                CheckpointBridge.OnPassed("unknown", json);
                Assert.That(calls, Is.Zero);
                context.Drain();
                Assert.That(calls, Is.EqualTo(1));
            }
            finally
            {
                removeFirst();
                removeSecond();
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Test]
        public void DetachSuppressesResultsAlreadyQueuedForTheMainThread()
        {
            var previous = SynchronizationContext.Current;
            var context = new QueuedContext();
            SynchronizationContext.SetSynchronizationContext(context);
            var calls = 0;
            var callId = Guid.NewGuid().ToString();
            var remove = CheckpointBridge.Register(callId, _ => calls++);
            try
            {
                CheckpointBridge.OnPassed(callId, Result().ToString());
                remove();
                remove();
                context.Drain();
                CheckpointBridge.OnPassed(callId, Result().ToString());
                context.Drain();
                Assert.That(calls, Is.Zero);
            }
            finally
            {
                remove();
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Test]
        public void NativeErrorLogsAndDetachesWithoutPassing()
        {
            var previous = SynchronizationContext.Current;
            var context = new QueuedContext();
            SynchronizationContext.SetSynchronizationContext(context);
            var calls = 0;
            var callId = Guid.NewGuid().ToString();
            var remove = CheckpointBridge.Register(callId, _ => calls++);
            try
            {
                CheckpointBridge.OnError(callId, "NOT_CONFIGURED", "Configure Purchases before calling a checkpoint.");
                UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
                    "[RevenueCatUI] Checkpoint could not be started (NOT_CONFIGURED): Configure Purchases before calling a checkpoint.");
                context.Drain();
                CheckpointBridge.OnPassed(callId, Result().ToString());
                context.Drain();
                Assert.That(calls, Is.Zero);
            }
            finally
            {
                remove();
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Test]
        public void DetachNotifiesNativeOnce()
        {
            var callId = Guid.NewGuid().ToString();
            var detach = CheckpointBridge.Register(callId, _ => Assert.Fail());
            detach();
            detach();
            Assert.That(_native, Is.EqualTo(new[] { $"detachCheckpoint({callId}, null)" }));
        }

        [Test]
        public void DetachAfterPassingOrFailingDoesNotNotifyNative()
        {
            var passedId = Guid.NewGuid().ToString();
            var failedId = Guid.NewGuid().ToString();
            var detachPassed = CheckpointBridge.Register(passedId, _ => { });
            var detachFailed = CheckpointBridge.Register(failedId, _ => Assert.Fail());
            CheckpointBridge.OnPassed(passedId, Result().ToString());
            CheckpointBridge.OnError(failedId, "NOT_CONFIGURED", "Not configured.");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "[RevenueCatUI] Checkpoint could not be started (NOT_CONFIGURED): Not configured.");
            _context.Drain();
            detachPassed();
            detachFailed();
            Assert.That(_native, Is.Empty);
        }

        [Test]
        public void PaywallRequestsWithoutPresenterNavigateBack()
        {
            var callId = Guid.NewGuid().ToString();
            var detach = CheckpointBridge.Register(callId, _ => Assert.Fail());
            CheckpointBridge.OnPaywallPresentationRequested(callId, "request-1", "{}");
            CheckpointBridge.OnPaywallPresentationRequested("unknown", "request-2", "{}");
            _context.Drain();
            Assert.That(_native, Is.EqualTo(new[]
            {
                "completePaywallPresentation(request-1, NAVIGATED_BACK)",
                "completePaywallPresentation(request-2, NAVIGATED_BACK)"
            }));
            detach();
        }

        [Test]
        public void ErrorDialogRequestsWithoutFactoryUseDefault()
        {
            CheckpointBridge.OnErrorDialogRequested("unknown", "request-1", "{\"kind\":\"flowActionFailed\",\"code\":1,\"message\":\"m\"}");
            _context.Drain();
            Assert.That(_native, Is.EqualTo(new[] { "resolveErrorDialog(request-1, {\"type\":\"useDefault\"})" }));
        }

        [Test]
        public void RoutesTypedPaywallParamsAndCompletesAtMostOnce()
        {
            PaywallPresentationParams received = null;
            var detach = Start((parameters, complete) =>
            {
                received = parameters;
                complete(PaywallPresentationResult.Continued);
                complete(PaywallPresentationResult.NavigatedBack);
            });
            Assert.That(_starts, Is.EqualTo(new[] { "upgrade, custom presenter: True" }));
            CheckpointBridge.OnPaywallPresentationRequested(_lastCallId, "request", PaywallParams);
            Assert.That(received, Is.Null, "The presenter runs on the Unity main thread, not the native one.");
            _context.Drain();
            Assert.That(received.CheckpointIdentifier, Is.EqualTo("upgrade"));
            Assert.That(received.Offering.Identifier, Is.EqualTo("pro"));
            Assert.That(received.PresentationMode, Is.EqualTo(FlowPresentationMode.ModalSheet));
            Assert.That(received.CustomVariables, Is.EquivalentTo(new Dictionary<string, CustomVariableValue>
            {
                { "name", CustomVariableValue.String("Taylor") },
                { "level", CustomVariableValue.Number(12) },
                { "premium", CustomVariableValue.Boolean(true) }
            }));
            Assert.That(_native, Is.EqualTo(new[] { "completePaywallPresentation(request, CONTINUED)" }));
            detach();
        }

        [Test]
        public void CapturesGlobalPresenterAndSupportsPerCallAndNativeOverrides()
        {
            var called = new List<string>();
            Checkpoints.SetPaywallPresenter((_, complete) => { called.Add("first"); complete(PaywallPresentationResult.Closed); });
            var detachFirst = Start();
            var first = _lastCallId;
            Checkpoints.SetPaywallPresenter((_, complete) => { called.Add("second"); complete(PaywallPresentationResult.Closed); });
            var detachSecond = Start((_, complete) => { called.Add("override"); complete(PaywallPresentationResult.Closed); });
            var second = _lastCallId;
            var detachNative = Start(native: true);
            var native = _lastCallId;
            Assert.That(_starts, Is.EqualTo(new[]
            {
                "upgrade, custom presenter: True", "upgrade, custom presenter: True", "upgrade, custom presenter: False"
            }));
            CheckpointBridge.OnPaywallPresentationRequested(first, "one", PaywallParams);
            CheckpointBridge.OnPaywallPresentationRequested(second, "two", PaywallParams);
            CheckpointBridge.OnPaywallPresentationRequested(native, "three", PaywallParams);
            _context.Drain();
            Assert.That(called, Is.EqualTo(new[] { "first", "override" }));
            Assert.That(_native, Is.EqualTo(new[]
            {
                "completePaywallPresentation(three, NAVIGATED_BACK)",
                "completePaywallPresentation(one, CLOSED)",
                "completePaywallPresentation(two, CLOSED)"
            }));
            detachFirst();
            detachSecond();
            detachNative();
        }

        [Test]
        public void PresenterThatThrowsNavigatesBack()
        {
            var detach = Start((_, __) => throw new InvalidOperationException("boom"));
            CheckpointBridge.OnPaywallPresentationRequested(_lastCallId, "request", PaywallParams);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("presenter failed.*boom"));
            _context.Drain();
            Assert.That(_native, Is.EqualTo(new[] { "completePaywallPresentation(request, NAVIGATED_BACK)" }));
            detach();
            Assert.That(_native.Count, Is.EqualTo(1), "Backing out ends the call, so there is nothing to detach.");
        }

        [Test]
        public void ReturningFromThePresenterDoesNotCompleteThePaywall()
        {
            var detach = Start((_, __) => { });
            CheckpointBridge.OnPaywallPresentationRequested(_lastCallId, "request", PaywallParams);
            _context.Drain();
            Assert.That(_native, Is.Empty);
            detach();
        }

        [Test]
        public void MalformedPaywallParamsNavigateBackWithoutInvokingPresenter()
        {
            var called = false;
            var detach = Start((_, __) => called = true);
            CheckpointBridge.OnPaywallPresentationRequested(_lastCallId, "request", "{\"bad\":true}");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Malformed checkpoint paywall parameters"));
            _context.Drain();
            Assert.That(called, Is.False);
            Assert.That(_native, Is.EqualTo(new[] { "completePaywallPresentation(request, NAVIGATED_BACK)" }));
            detach();
        }

        [Test]
        public void DetachCancelsQueuedAndLatePresentations()
        {
            var called = false;
            var detach = Start((_, __) => called = true);
            var callId = _lastCallId;
            CheckpointBridge.OnPaywallPresentationRequested(callId, "queued", PaywallParams);
            detach();
            detach();
            CheckpointBridge.OnPaywallPresentationRequested(callId, "late", PaywallParams);
            _context.Drain();
            Assert.That(called, Is.False);
            // Native already backed out the queued request when it detached without an active request.
            Assert.That(_native, Is.EqualTo(new[]
            {
                $"detachCheckpoint({callId}, null)",
                "completePaywallPresentation(late, NAVIGATED_BACK)"
            }));
        }

        [Test]
        public void PresentedPaywallCanCompleteAfterDetach()
        {
            Action<PaywallPresentationResult> complete = null;
            var detach = Start((_, completion) => complete = completion);
            var callId = _lastCallId;
            CheckpointBridge.OnPaywallPresentationRequested(callId, "request", PaywallParams);
            _context.Drain();
            detach();
            Assert.That(_native, Is.EqualTo(new[] { $"detachCheckpoint({callId}, request)" }));
            complete(PaywallPresentationResult.Continued);
            _context.Drain();
            Assert.That(_native.Last(), Is.EqualTo("completePaywallPresentation(request, CONTINUED)"));
        }

        [Test]
        public void PassingAfterCompletionCallsTheCallbackOnce()
        {
            var passes = 0;
            var detach = CheckpointBridge.Start("upgrade", _ => passes++,
                new CheckpointParams(paywallPresenter: (_, complete) => complete(PaywallPresentationResult.Continued)));
            CheckpointBridge.OnPaywallPresentationRequested(_lastCallId, "request", PaywallParams);
            _context.Drain();
            CheckpointBridge.OnPassed(_lastCallId, Result().ToString());
            CheckpointBridge.OnPassed(_lastCallId, Result().ToString());
            _context.Drain();
            detach();
            Assert.That(passes, Is.EqualTo(1));
            Assert.That(_native, Is.EqualTo(new[] { "completePaywallPresentation(request, CONTINUED)" }));
        }

        [Test]
        public void EditorDoesNotPretendToPassCheckpoints()
        {
            Assert.Throws<PlatformNotSupportedException>(() => Checkpoints.Checkpoint("upgrade", _ => Assert.Fail()));
        }
    }
}
