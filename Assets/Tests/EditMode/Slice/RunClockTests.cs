using NUnit.Framework;
using Project1028.Slice;

namespace Project1028.Slice.Tests
{
    public class RunClockTests
    {
        private static RunClock C() => new RunClock(300f, 600f, 180f);

        [Test] public void Day_Until_Night() { var c = C(); Assert.IsNull(c.Tick(299f, false)); Assert.AreEqual(RunPhase.Day, c.Phase); Assert.AreEqual(RunPhase.Night, c.Tick(1f, false)); }
        [Test] public void Night_To_DepartureOpen() { var c = C(); c.Tick(300f, false); Assert.AreEqual(RunPhase.DepartureOpen, c.Tick(300f, false)); Assert.IsTrue(c.IsDepartureOpen); }
        [Test] public void Blocked_Lockdown_And_Back()
        {
            var c = C(); c.Tick(600f, false);
            Assert.AreEqual(RunPhase.Lockdown, c.Tick(1f, true));
            Assert.AreEqual(RunPhase.DepartureOpen, c.Tick(1f, false));
        }
        [Test] public void NextBoat_And_Remaining()
        {
            var c = C(); Assert.AreEqual(780f, c.NextBoatAt, 1e-4f);
            c.Tick(100f, false); Assert.AreEqual(500f, c.RemainingToDeparture, 1e-4f); Assert.AreEqual(680f, c.RemainingToNextBoat, 1e-4f);
        }
        [Test] public void End_StopsTicking() { var c = C(); c.End(); Assert.IsNull(c.Tick(1000f, false)); Assert.AreEqual(RunPhase.Ended, c.Phase); Assert.AreEqual(0f, c.Elapsed); }
        [Test] public void Skip_Advances() { var c = C(); c.Skip(60f); Assert.AreEqual(60f, c.Elapsed); }
    }
}
