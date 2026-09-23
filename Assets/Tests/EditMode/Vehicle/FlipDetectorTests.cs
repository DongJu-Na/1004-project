using NUnit.Framework;
using Project1028.Vehicle;

namespace Project1028.Vehicle.Tests
{
    public class FlipDetectorTests
    {
        [Test] public void Upright_NotFlipped() { var f = new FlipDetector(0.2f, 3f); Assert.IsFalse(f.Tick(1f, 1f)); Assert.IsFalse(f.IsFlipped); }
        [Test] public void Below_NotYet() { var f = new FlipDetector(0.2f, 3f); f.Tick(-0.5f, 1f); f.Tick(-0.5f, 1f); Assert.IsFalse(f.Tick(-0.5f, 0.9f)); }
        [Test] public void Below_ReachesSeconds_OnceTrue()
        {
            var f = new FlipDetector(0.2f, 3f); f.Tick(-0.5f, 2.9f);
            Assert.IsTrue(f.Tick(-0.5f, 0.1f)); Assert.IsTrue(f.IsFlipped); Assert.IsFalse(f.Tick(-0.5f, 1f));
        }
        [Test] public void Upright_Resets() { var f = new FlipDetector(0.2f, 3f); f.Tick(-0.5f, 3f); f.Tick(1f, 0.1f); Assert.IsFalse(f.IsFlipped); }
        [Test] public void Sideways_BelowThreshold_Flips() { var f = new FlipDetector(0.2f, 3f); Assert.IsTrue(f.Tick(0.1f, 3f)); }
    }
}
