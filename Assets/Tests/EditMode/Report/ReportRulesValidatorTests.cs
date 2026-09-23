using NUnit.Framework;
using Project1028.Report;

namespace Project1028.Report.Tests
{
    public class ReportRulesValidatorTests
    {
        private static ReportRules Valid() => new ReportRules { version = "t", minWalkSeconds = 6f, reportDurationSeconds = 3f, cutDurationSeconds = 4f, cutRequiresTool = false, cutRange = 2f, arriveDistance = 1.2f, completedEventId = "report_completed", status = "확정대기" };

        [Test] public void Valid_Ok_Pending1() { var r = ReportRulesValidator.Validate(Valid(), _ => true); Assert.IsFalse(r.IsError, string.Join("\n", r.Messages)); Assert.AreEqual(1, r.PendingCount); }
        [Test] public void MinWalk0_Error() { var v = Valid(); v.minWalkSeconds = 0f; Assert.IsTrue(ReportRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void Cut0_Error() { var v = Valid(); v.cutDurationSeconds = 0f; Assert.IsTrue(ReportRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void NegativeReport_Error() { var v = Valid(); v.reportDurationSeconds = -1f; Assert.IsTrue(ReportRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void Range0_Error() { var v = Valid(); v.cutRange = 0f; Assert.IsTrue(ReportRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void UnknownEvent_Error() => Assert.IsTrue(ReportRulesValidator.Validate(Valid(), _ => false).IsError);
        [Test] public void MissingStatus_Error() { var v = Valid(); v.status = null; Assert.IsTrue(ReportRulesValidator.Validate(v, _ => true).IsError); }
    }
}
