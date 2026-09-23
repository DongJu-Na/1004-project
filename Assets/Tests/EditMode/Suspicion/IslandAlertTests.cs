using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class IslandAlertTests
    {
        [TestCase(0, AlertZone.Calm)] [TestCase(25, AlertZone.Calm)] [TestCase(26, AlertZone.Watch)] [TestCase(50, AlertZone.Watch)]
        [TestCase(51, AlertZone.Tension)] [TestCase(75, AlertZone.Tension)] [TestCase(76, AlertZone.Lockdown)] [TestCase(100, AlertZone.Lockdown)]
        public void ZoneOf_Boundaries(int v, AlertZone expected) => Assert.AreEqual(expected, IslandAlert.ZoneOf(v));

        [Test] public void From25_PlusOne_CalmToWatch()
        {
            var a = new IslandAlert(25);
            var c = a.Apply(1);
            Assert.IsTrue(c.HasValue); Assert.AreEqual(AlertZone.Calm, c.Value.Old); Assert.AreEqual(AlertZone.Watch, c.Value.New);
        }

        [Test] public void From75_PlusOne_Lockdown_Blocked()
        {
            var a = new IslandAlert(75);
            var c = a.Apply(1);
            Assert.AreEqual(AlertZone.Lockdown, c.Value.New); Assert.IsTrue(a.DepartureBlocked);
        }

        [Test] public void At100_Plus30_Stays100_NoChange()
        {
            var a = new IslandAlert(100);
            Assert.IsFalse(a.Apply(30).HasValue); Assert.AreEqual(100, a.Value);
        }

        [Test] public void Decrease_CrossesDown_WatchToCalm()
        {
            var a = new IslandAlert(30);
            var c = a.Apply(-5);
            Assert.AreEqual(25, a.Value); Assert.AreEqual(AlertZone.Watch, c.Value.Old); Assert.AreEqual(AlertZone.Calm, c.Value.New);
        }

        [Test] public void Decay_AccumulatesFraction()
        {
            var a = new IslandAlert(60);
            Assert.IsNull(a.ApplyDecay(0.4f)); Assert.AreEqual(60, a.Value);
            Assert.IsNull(a.ApplyDecay(0.4f)); Assert.AreEqual(60, a.Value);
            a.ApplyDecay(0.4f); Assert.AreEqual(59, a.Value);
        }

        [Test] public void Decay_From76_ReleasesLockdown()
        {
            var a = new IslandAlert(76);
            var c = a.ApplyDecay(1f);
            Assert.AreEqual(75, a.Value); Assert.IsTrue(c.HasValue); Assert.AreEqual(AlertZone.Tension, c.Value.New); Assert.IsFalse(a.DepartureBlocked);
        }

        [Test] public void Decay_AtZero_DoesNothing()
        {
            var a = new IslandAlert(0);
            Assert.IsNull(a.ApplyDecay(5f)); Assert.AreEqual(0, a.Value);
        }
    }
}
