using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class PersonalSuspicionTests
    {
        [Test] public void Initial_IsZero_Indifferent()
        {
            var s = new PersonalSuspicion(0f);
            Assert.AreEqual(0, s.Value); Assert.AreEqual(SuspicionStage.Indifferent, s.Stage);
        }

        [Test] public void PlusOne_BecomesAware_WithChange()
        {
            var s = new PersonalSuspicion(0f);
            var c = s.Apply(1, 10f);
            Assert.IsTrue(c.HasValue); Assert.AreEqual(SuspicionStage.Indifferent, c.Value.Old); Assert.AreEqual(SuspicionStage.Aware, c.Value.New);
        }

        [Test] public void PlusFive_ClampsToThree_Certain()
        {
            var s = new PersonalSuspicion(0f);
            s.Apply(5, 1f);
            Assert.AreEqual(3, s.Value); Assert.AreEqual(SuspicionStage.Certain, s.Stage);
        }

        [Test] public void AtThree_PlusOne_NoChange()
        {
            var s = new PersonalSuspicion(0f); s.Apply(3, 1f);
            Assert.IsFalse(s.Apply(1, 2f).HasValue); Assert.AreEqual(3, s.Value);
        }

        [Test] public void MinusOne_FromThree_IsAlert()
        {
            var s = new PersonalSuspicion(0f); s.Apply(3, 1f);
            var c = s.Apply(-1, 2f);
            Assert.AreEqual(SuspicionStage.Alert, c.Value.New); Assert.AreEqual(2, s.Value);
        }

        [Test] public void MinusTen_ClampsToZero()
        {
            var s = new PersonalSuspicion(0f); s.Apply(2, 1f); s.Apply(-10, 2f);
            Assert.AreEqual(0, s.Value);
        }

        [Test] public void LastRaisedAt_UpdatesOnlyOnPositive()
        {
            var s = new PersonalSuspicion(0f);
            s.Apply(1, 5f); Assert.AreEqual(5f, s.LastRaisedAt);
            s.Apply(-1, 9f); Assert.AreEqual(5f, s.LastRaisedAt);
        }

        [Test] public void SameFrame_TwoPlusOne_IsThree()
        {
            var s = new PersonalSuspicion(0f); s.Apply(2, 1f); s.Apply(1, 1f);
            Assert.AreEqual(3, s.Value);
        }
    }
}
