#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Collections.Generic;
using System.IO;
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

        [SetUp]
        public void SetUp()
        {
            _native.Clear();
            _previousSendToNative = CheckpointBridge.SendToNative;
            CheckpointBridge.SendToNative = (method, first, second) => _native.Add($"{method}({first}, {second ?? "null"})");
            _previousContext = SynchronizationContext.Current;
            _context = new QueuedContext();
            SynchronizationContext.SetSynchronizationContext(_context);
        }

        [TearDown]
        public void TearDown()
        {
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
        public void EditorDoesNotPretendToPassCheckpoints()
        {
            Assert.Throws<PlatformNotSupportedException>(() => Checkpoints.Checkpoint("upgrade", _ => Assert.Fail()));
        }
    }
}
