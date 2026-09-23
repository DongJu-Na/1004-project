using NUnit.Framework;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class HandRulesTests
    {
        [TestCase(SubdueAction.Backstab, HandState.Empty, HeldKind.None, true)]
        [TestCase(SubdueAction.Backstab, HandState.OneHand, HeldKind.Object, false)]
        [TestCase(SubdueAction.Backstab, HandState.TwoHands, HeldKind.Object, false)]
        [TestCase(SubdueAction.Shove, HandState.Empty, HeldKind.None, true)]
        [TestCase(SubdueAction.Shove, HandState.OneHand, HeldKind.Object, false)]
        [TestCase(SubdueAction.Shove, HandState.TwoHands, HeldKind.Object, false)]
        [TestCase(SubdueAction.ObjectStrike, HandState.Empty, HeldKind.None, false)]
        [TestCase(SubdueAction.ObjectStrike, HandState.OneHand, HeldKind.Object, true)]
        [TestCase(SubdueAction.ObjectStrike, HandState.TwoHands, HeldKind.Object, true)]
        public void NineCombos(SubdueAction a, HandState h, HeldKind k, bool expected)
        {
            var (ok, reason) = HandRules.CanPerform(a, h, k);
            Assert.AreEqual(expected, ok);
            if (!ok) Assert.IsNotEmpty(reason);
        }

        [TestCase(SubdueAction.Backstab)] [TestCase(SubdueAction.Shove)] [TestCase(SubdueAction.ObjectStrike)]
        public void HoldingBody_RejectsAll(SubdueAction a) => Assert.IsFalse(HandRules.CanPerform(a, HandState.TwoHands, HeldKind.UnconsciousNpc).ok);

        [Test] public void PickUpRules()
        {
            Assert.IsTrue(HandRules.CanPickUpObject(HandState.Empty)); Assert.IsFalse(HandRules.CanPickUpObject(HandState.OneHand));
            Assert.IsTrue(HandRules.CanPickUpBody(HandState.Empty)); Assert.IsFalse(HandRules.CanPickUpBody(HandState.OneHand));
        }
    }
}
