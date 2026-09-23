using NUnit.Framework;
using Project1028.Encounter;

namespace Project1028.Encounter.Tests
{
    public class ChoiceResolverTests
    {
        [Test] public void Choose_Resolves() { var c = new ChoiceResolver("B", 12f); Assert.IsTrue(c.Choose("A")); Assert.IsTrue(c.Resolved); Assert.AreEqual("A", c.Option); Assert.IsFalse(c.ByTimeout); }
        [Test] public void Timeout_Default() { var c = new ChoiceResolver("B", 12f); Assert.IsFalse(c.Tick(11.9f)); Assert.IsTrue(c.Tick(0.1f)); Assert.AreEqual("B", c.Option); Assert.IsTrue(c.ByTimeout); }
        [Test] public void AfterResolve_ChooseIgnored() { var c = new ChoiceResolver("B", 12f); c.Choose("A"); Assert.IsFalse(c.Choose("B")); Assert.AreEqual("A", c.Option); }
        [Test] public void InvalidOption_False() => Assert.IsFalse(new ChoiceResolver("B", 12f).Choose("Z"));
        [Test] public void ForceDefault() { var c = new ChoiceResolver("A", 12f); c.ForceDefault(); Assert.AreEqual("A", c.Option); Assert.IsTrue(c.ByTimeout); }
    }
}
