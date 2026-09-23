using NUnit.Framework;
using Project1028.Encounter;

namespace Project1028.Encounter.Tests
{
    public class RunLedgerTests
    {
        [Test] public void Record_History() { var l = new RunLedger(); Assert.IsTrue(l.Record("a")); Assert.IsFalse(l.Record("a")); Assert.IsTrue(l.History.Contains("a")); }
        [Test] public void AddFlags_Dedup() { var l = new RunLedger(); Assert.AreEqual(2, l.AddFlags(new[] { "x", "y", "x" })); Assert.AreEqual(0, l.AddFlags(new[] { "x" })); }
        [Test] public void Import_Prior() { var l = new RunLedger(); l.Import(new[] { "old" }); Assert.IsTrue(l.History.Contains("old")); }
        [Test] public void Export_Sorted() { var l = new RunLedger(); l.AddFlags(new[] { "b", "a" }); l.Record("z"); l.Record("y"); var (f, h) = l.Export(); Assert.AreEqual(new[] { "a", "b" }, f); Assert.AreEqual(new[] { "y", "z" }, h); }
        [Test] public void CallbacksFor_Filters()
        {
            var table = new[] { new CallbackEntry { flag = "x", targetKind = "document", targetId = "d" }, new CallbackEntry { flag = "y", targetKind = "npc_line", targetId = "n" } };
            var r = RunLedger.CallbacksFor("x", table);
            Assert.AreEqual(1, r.Count); Assert.AreEqual("d", r[0].targetId);
            Assert.AreEqual(0, RunLedger.CallbacksFor("zzz", table).Count);
        }
    }
}
