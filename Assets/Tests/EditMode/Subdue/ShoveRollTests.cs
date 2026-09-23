using NUnit.Framework;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class ShoveRollTests
    {
        [Test] public void Fail0_AlwaysSucceeds() => Assert.IsTrue(ShoveRoll.Succeeds(0f, 0f));
        [Test] public void Fail1_AlwaysFails() => Assert.IsFalse(ShoveRoll.Succeeds(1f, 0.99f));
        [Test] public void Threshold() { Assert.IsFalse(ShoveRoll.Succeeds(0.35f, 0.34f)); Assert.IsTrue(ShoveRoll.Succeeds(0.35f, 0.35f)); }
    }
}
