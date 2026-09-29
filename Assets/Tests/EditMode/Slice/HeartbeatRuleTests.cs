using NUnit.Framework;
using Project1028.Slice;

namespace Project1028.Slice.Tests
{
    public class HeartbeatRuleTests
    {
        private static readonly float[] I = { 2f, 1.2f, 0.7f, 0.4f };
        private static readonly float[] M = { 1f, 0.9f, 0.75f, 0.55f };
        [Test] public void Calm_Stage0() => Assert.AreEqual(2f, HeartbeatRule.IntervalFor(0, 0, I, M), 1e-4f);
        [Test] public void Stage3_Calm() => Assert.AreEqual(0.4f, HeartbeatRule.IntervalFor(3, 0, I, M), 1e-4f);
        [Test] public void Stage0_Lockdown() => Assert.AreEqual(1.1f, HeartbeatRule.IntervalFor(0, 3, I, M), 1e-4f);
        [Test] public void Stage3_Lockdown() => Assert.AreEqual(0.22f, HeartbeatRule.IntervalFor(3, 3, I, M), 1e-4f);
        [Test] public void OutOfRange_Clamped() { Assert.AreEqual(0.22f, HeartbeatRule.IntervalFor(9, 9, I, M), 1e-4f); Assert.AreEqual(2f, HeartbeatRule.IntervalFor(-1, -1, I, M), 1e-4f); }
        [Test] public void Null_Default() => Assert.AreEqual(2f, HeartbeatRule.IntervalFor(0, 0, null, null), 1e-4f);
    }
}
