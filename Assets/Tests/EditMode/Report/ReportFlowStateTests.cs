using NUnit.Framework;
using Project1028.Report;

namespace Project1028.Report.Tests
{
    public class ReportFlowStateTests
    {
        [Test] public void Start_Moving() { var s = new ReportFlowState(); s.Start("A"); Assert.AreEqual(ReportPhase.Moving, s.Phase); Assert.AreEqual("A", s.TargetId); }
        [Test] public void Arrive_Reporting() { var s = new ReportFlowState(); s.Start("A"); s.Arrive(10f); Assert.AreEqual(ReportPhase.Reporting, s.Phase); }
        [Test] public void Tick_CompletesAfterDuration()
        {
            var s = new ReportFlowState(); s.Start("A"); s.Arrive(10f);
            Assert.IsFalse(s.Tick(12f, 3f)); Assert.IsTrue(s.Tick(13f, 3f)); Assert.AreEqual(ReportPhase.Completed, s.Phase);
        }
        [Test] public void Retarget_Moving() { var s = new ReportFlowState(); s.Start("A"); s.Retarget("B"); Assert.AreEqual(ReportPhase.Moving, s.Phase); Assert.AreEqual("B", s.TargetId); }
        [Test] public void RetargetNull_Abandoned() { var s = new ReportFlowState(); s.Start("A"); s.Retarget(null); Assert.AreEqual(ReportPhase.Abandoned, s.Phase); Assert.IsTrue(s.IsTerminal); }
        [Test] public void Interrupt() { var s = new ReportFlowState(); s.Start("A"); s.Interrupt(); Assert.AreEqual(ReportPhase.Interrupted, s.Phase); }
        [Test] public void Terminal_IgnoresTransitions()
        {
            var s = new ReportFlowState(); s.Start("A"); s.Interrupt();
            s.Arrive(1f); s.Retarget("B"); Assert.AreEqual(ReportPhase.Interrupted, s.Phase); Assert.IsFalse(s.Tick(100f, 0f));
        }
        [Test] public void Retarget_WhileReporting_BackToMoving() { var s = new ReportFlowState(); s.Start("A"); s.Arrive(1f); s.Retarget("B"); Assert.AreEqual(ReportPhase.Moving, s.Phase); }
    }
}
