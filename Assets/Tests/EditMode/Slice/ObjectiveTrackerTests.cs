using NUnit.Framework;
using Project1028.Slice;

namespace Project1028.Slice.Tests
{
    public class ObjectiveTrackerTests
    {
        private static ObjectiveTracker Make() => new ObjectiveTracker(new[]
        {
            new ObjectiveStep { id = "a", text = "t", markerFacilityId = "w", condition = ObjectiveConditions.EvidencePicked },
            new ObjectiveStep { id = "b", text = "t", markerFacilityId = "d", condition = ObjectiveConditions.AtDockWithEvidence },
        });

        [Test] public void Starts_AtFirst() { var t = Make(); Assert.AreEqual("a", t.Current.id); Assert.IsFalse(t.IsComplete); }
        [Test] public void WrongOrder_NoAdvance() { var t = Make(); Assert.IsFalse(t.Satisfy(ObjectiveConditions.AtDockWithEvidence)); Assert.AreEqual(0, t.Index); }
        [Test] public void Advance_Then_Complete()
        {
            var t = Make();
            Assert.IsTrue(t.Satisfy(ObjectiveConditions.EvidencePicked)); Assert.AreEqual(1, t.Index);
            Assert.IsFalse(t.Satisfy(ObjectiveConditions.EvidencePicked));
            Assert.IsTrue(t.Satisfy(ObjectiveConditions.AtDockWithEvidence)); Assert.IsTrue(t.IsComplete); Assert.IsNull(t.Current);
        }
        [Test] public void Empty_IsComplete() => Assert.IsTrue(new ObjectiveTracker(null).IsComplete);
    }
}
