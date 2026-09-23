using NUnit.Framework;
using Project1028.Report;

namespace Project1028.Report.Tests
{
    public class ReportSpeedRuleTests
    {
        [Test] public void FarEnough_KeepsNormal() => Assert.AreEqual(1.6f, ReportSpeedRule.SpeedFor(16f, 1.6f, 6f), 1e-4f);
        [Test] public void Close_SlowsDown() => Assert.AreEqual(1.0f, ReportSpeedRule.SpeedFor(6f, 1.6f, 6f), 1e-4f);
        [Test] public void ZeroDistance_ClampsMin() => Assert.AreEqual(ReportSpeedRule.MinSpeed, ReportSpeedRule.SpeedFor(0f, 1.6f, 6f), 1e-4f);
        [Test] public void NoMinWalk_Normal() => Assert.AreEqual(1.6f, ReportSpeedRule.SpeedFor(3f, 1.6f, 0f), 1e-4f);
    }
}
