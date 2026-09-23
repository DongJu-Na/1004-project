using System.Linq;
using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class PropagationSchedulerTests
    {
        [Test] public void Reserve_OncePerMeeting()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f);
            Assert.IsTrue(s.TryReserve("A", "B", "P1", 4f));
            Assert.IsFalse(s.TryReserve("A", "B", "P1", 4f));
        }

        [Test] public void Drain_RespectsDelay()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f); s.TryReserve("A", "B", "P1", 4f);
            Assert.AreEqual(0, s.Drain(3f).Count());
            Assert.AreEqual(1, s.Drain(4f).Count());
            Assert.AreEqual(0, s.Pending.Count);
        }

        [Test] public void Remeeting_AllowsReserveAgain()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f); Assert.IsTrue(s.TryReserve("A", "B", "P1", 4f));
            s.SetMeeting("A", "B", false, 5f);
            s.SetMeeting("A", "B", true, 6f); Assert.IsTrue(s.TryReserve("A", "B", "P1", 10f));
        }

        [Test] public void Directions_AreIndependent()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f);
            Assert.IsTrue(s.TryReserve("A", "B", "P1", 4f));
            Assert.IsTrue(s.TryReserve("B", "A", "P1", 4f));
        }

        [Test] public void Players_AreIndependent()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f);
            Assert.IsTrue(s.TryReserve("A", "B", "P1", 4f));
            Assert.IsTrue(s.TryReserve("A", "B", "P2", 4f));
        }

        [Test] public void ReservationSurvivesMeetingEnd()
        {
            var s = new PropagationScheduler();
            s.SetMeeting("A", "B", true, 0f); s.TryReserve("A", "B", "P1", 4f);
            s.SetMeeting("A", "B", false, 1f);
            Assert.AreEqual(1, s.Drain(4f).Count());
        }

        [Test] public void NoMeeting_CannotReserve()
        {
            var s = new PropagationScheduler();
            Assert.IsFalse(s.TryReserve("A", "B", "P1", 4f));
        }
    }
}
