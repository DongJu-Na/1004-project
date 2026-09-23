using NUnit.Framework;
using Project1028.Report;

namespace Project1028.Report.Tests
{
    public class CutProgressTests
    {
        [Test] public void Progresses()
        {
            var c = new CutProgress(4f);
            Assert.AreEqual(CutTickResult.Progressed, c.Tick(1f, true)); Assert.AreEqual(0.25f, c.Progress01, 1e-4f);
        }
        [Test] public void Invalid_Cancels_Resets()
        {
            var c = new CutProgress(4f); c.Tick(1f, true);
            Assert.AreEqual(CutTickResult.Cancelled, c.Tick(1f, false)); Assert.AreEqual(0f, c.Progress01);
        }
        [Test] public void Invalid_WithoutProgress_Idle() => Assert.AreEqual(CutTickResult.Idle, new CutProgress(4f).Tick(1f, false));
        [Test] public void Completes()
        {
            var c = new CutProgress(4f); c.Tick(2f, true);
            Assert.AreEqual(CutTickResult.Completed, c.Tick(2f, true)); Assert.IsTrue(c.IsDone);
            Assert.AreEqual(CutTickResult.Idle, c.Tick(1f, true));
        }
        [Test] public void Reset_Zero() { var c = new CutProgress(4f); c.Tick(3f, true); c.Reset(); Assert.AreEqual(0f, c.Progress01); }
    }
}
