using NUnit.Framework;
using Project1028.Encounter;
using static Project1028.Encounter.SpawnGate;

namespace Project1028.Encounter.Tests
{
    public class SpawnGateTests
    {
        private static SpawnContext Ok() => new SpawnContext { RunCount = 1, PerRunMax = 4, SinceLastEnd = 60f, MinInterval = 20f, Moving = true, Busy = false, Scouting = false, ActiveInstance = false };

        [Test] public void AllGood_Ok() => Assert.IsTrue(CanSpawn(Ok()).ok);
        [Test] public void Cap_Blocks() { var c = Ok(); c.RunCount = 4; Assert.IsFalse(CanSpawn(c).ok); Assert.IsTrue(CanSpawn(c).reason.Contains("상한")); }
        [Test] public void Interval_Blocks() { var c = Ok(); c.SinceLastEnd = 5f; Assert.IsFalse(CanSpawn(c).ok); }
        [Test] public void NotMoving_Blocks() { var c = Ok(); c.Moving = false; Assert.IsTrue(CanSpawn(c).reason.Contains("이동")); }
        [Test] public void Busy_Blocks() { var c = Ok(); c.Busy = true; Assert.IsFalse(CanSpawn(c).ok); }
        [Test] public void Scouting_Blocks() { var c = Ok(); c.Scouting = true; Assert.IsFalse(CanSpawn(c).ok); }
        [Test] public void Active_Blocks() { var c = Ok(); c.ActiveInstance = true; Assert.IsFalse(CanSpawn(c).ok); }
        [Test] public void Roll_VehicleHigher() { Assert.IsTrue(Roll(true, 0.35f, 0.7f, 0.5f)); Assert.IsFalse(Roll(false, 0.35f, 0.7f, 0.5f)); }
    }
}
