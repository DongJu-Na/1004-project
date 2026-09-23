using NUnit.Framework;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class UnconsciousTimerTests
    {
        [Test] public void Roll0_IsMin() => Assert.AreEqual(90f, new UnconsciousTimer(90f, 180f, 0f).Duration, 1e-4f);
        [Test] public void Roll1_IsMax() => Assert.AreEqual(180f, new UnconsciousTimer(90f, 180f, 1f).Duration, 1e-4f);
        [Test] public void RollHalf_IsMid() => Assert.AreEqual(135f, new UnconsciousTimer(90f, 180f, 0.5f).Duration, 1e-4f);
        [Test] public void Tick_Decreases_ThenAwake()
        {
            var t = new UnconsciousTimer(90f, 180f, 0f);
            t.Tick(30f); Assert.AreEqual(60f, t.Remaining, 1e-4f); Assert.IsFalse(t.IsAwake);
            t.Tick(70f); Assert.AreEqual(0f, t.Remaining); Assert.IsTrue(t.IsAwake);
        }
        [Test] public void NegativeTick_Ignored() { var t = new UnconsciousTimer(90f, 180f, 0f); t.Tick(-5f); Assert.AreEqual(90f, t.Remaining); }
        [Test] public void MaxBelowMin_Clamped() => Assert.AreEqual(100f, new UnconsciousTimer(100f, 50f, 1f).Duration, 1e-4f);
    }
}
