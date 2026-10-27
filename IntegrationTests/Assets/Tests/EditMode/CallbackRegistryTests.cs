using System;
using NUnit.Framework;

public class CallbackRegistryTests
{
    private CallbackRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        _registry = new CallbackRegistry();
    }

    [Test]
    public void TakesCallbacksByRequestIdInAnyOrder()
    {
        Action first = () => { };
        Action second = () => { };

        var firstId = _registry.Register(first);
        var secondId = _registry.Register(second);

        Assert.That(_registry.TryTake(secondId, out Action receivedSecond), Is.True);
        Assert.That(receivedSecond, Is.SameAs(second));
        Assert.That(_registry.TryTake(firstId, out Action receivedFirst), Is.True);
        Assert.That(receivedFirst, Is.SameAs(first));
    }

    [Test]
    public void TakingARequestIdTwiceFailsTheSecondTime()
    {
        var requestId = _registry.Register<Action>(() => { });

        Assert.That(_registry.TryTake(requestId, out Action _), Is.True);
        Assert.That(_registry.TryTake(requestId, out Action duplicate), Is.False);
        Assert.That(duplicate, Is.Null);
    }

    [Test]
    public void NullCallbackStillCreatesConsumableRequest()
    {
        Action callback = null;
        var requestId = _registry.Register(callback);

        Assert.That(requestId, Is.Not.Empty);
        Assert.That(_registry.TryTake(requestId, out Action received), Is.True);
        Assert.That(received, Is.Null);
        Assert.That(_registry.TryTake(requestId, out Action _), Is.False);
    }

    [Test]
    public void TypeMismatchDoesNotConsumeRequest()
    {
        Action callback = () => { };
        var requestId = _registry.Register(callback);

        Assert.That(_registry.TryTake(requestId, out Func<int> wrongType), Is.False);
        Assert.That(wrongType, Is.Null);
        Assert.That(_registry.TryTake(requestId, out Action received), Is.True);
        Assert.That(received, Is.SameAs(callback));
    }

    [Test]
    public void ClearRemovesEveryPendingRequest()
    {
        var firstId = _registry.Register<Action>(() => { });
        var secondId = _registry.Register<Action>(() => { });

        _registry.Clear();

        Assert.That(_registry.TryTake(firstId, out Action _), Is.False);
        Assert.That(_registry.TryTake(secondId, out Action _), Is.False);
    }
}
