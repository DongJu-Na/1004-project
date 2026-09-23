using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Project1028.Report;
using static Project1028.Report.ReportPointSelector;

namespace Project1028.Report.Tests
{
    public class ReportPointSelectorTests
    {
        [Test] public void Empty_Null() { Assert.IsNull(PickNearestUsable(new List<PointInfo>(), Vector3.zero)); Assert.IsNull(PickNearestUsable(null, Vector3.zero)); }
        [Test] public void OnlyUnusable_Null() => Assert.IsNull(PickNearestUsable(new List<PointInfo> { new PointInfo("a", Vector3.one, false) }, Vector3.zero));
        [Test] public void SkipsNearUnusable() => Assert.AreEqual("far", PickNearestUsable(new List<PointInfo> { new PointInfo("near", new Vector3(1, 0, 0), false), new PointInfo("far", new Vector3(10, 0, 0), true) }, Vector3.zero));
        [Test] public void PicksNearest() => Assert.AreEqual("b", PickNearestUsable(new List<PointInfo> { new PointInfo("a", new Vector3(5, 0, 0), true), new PointInfo("b", new Vector3(2, 0, 0), true) }, Vector3.zero));
        [Test] public void Tie_First() => Assert.AreEqual("a", PickNearestUsable(new List<PointInfo> { new PointInfo("a", new Vector3(3, 0, 0), true), new PointInfo("b", new Vector3(-3, 0, 0), true) }, Vector3.zero));
    }
}
