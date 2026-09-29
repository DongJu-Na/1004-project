using NUnit.Framework;
using Project1028.Slice;

namespace Project1028.Slice.Tests
{
    public class DepartureRuleTests
    {
        [Test] public void BeforeDeparture_Denied() { var v = DepartureRule.Evaluate(100f, 600f, 780f, false, true, true, true); Assert.IsFalse(v.CanBoard); Assert.IsTrue(v.Reason.Contains("아직")); }
        [Test] public void Blocked_Denied() { var v = DepartureRule.Evaluate(650f, 600f, 780f, true, true, true, true); Assert.IsFalse(v.CanBoard); Assert.IsTrue(v.Reason.Contains("봉쇄")); }
        [Test] public void NotAllAtDock_Denied() { var v = DepartureRule.Evaluate(650f, 600f, 780f, false, true, false, true); Assert.IsFalse(v.CanBoard); Assert.IsTrue(v.Reason.Contains("전원")); }
        [Test] public void WithEvidence_Success() { var v = DepartureRule.Evaluate(650f, 600f, 780f, false, true, true, true); Assert.IsTrue(v.CanBoard); Assert.AreEqual(RunOutcome.Success, v.Outcome); }
        [Test] public void NoEvidence_Required_LeftWithout() { var v = DepartureRule.Evaluate(650f, 600f, 780f, false, false, true, true); Assert.IsTrue(v.CanBoard); Assert.AreEqual(RunOutcome.LeftWithoutEvidence, v.Outcome); }
        [Test] public void NoEvidence_NotRequired_Success() => Assert.AreEqual(RunOutcome.Success, DepartureRule.Evaluate(650f, 600f, 780f, false, false, true, false).Outcome);
        [Test] public void Forced_WhenBlockedAtNextBoat() { Assert.AreEqual(RunOutcome.ForcedDeparture, DepartureRule.Forced(780f, 780f, true)); Assert.IsNull(DepartureRule.Forced(780f, 780f, false)); Assert.IsNull(DepartureRule.Forced(700f, 780f, true)); }
    }
}
