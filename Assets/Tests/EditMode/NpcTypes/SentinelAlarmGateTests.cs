using NUnit.Framework;
using Project1028.NpcTypes;

namespace Project1028.NpcTypes.Tests
{
    public class SentinelAlarmGateTests
    {
        [Test] public void FirstInRadius_Alarms() => Assert.IsTrue(new SentinelAlarmGate(5f).TryAlarm(true, 0f));
        [Test] public void WithinCooldown_NoAlarm()
        {
            var g = new SentinelAlarmGate(5f); g.TryAlarm(true, 0f);
            Assert.IsFalse(g.TryAlarm(true, 4f));
        }
        [Test] public void AfterCooldown_Alarms()
        {
            var g = new SentinelAlarmGate(5f); g.TryAlarm(true, 0f);
            Assert.IsTrue(g.TryAlarm(true, 5f));
        }
        [Test] public void OutOfRadius_NoAlarm_NoCooldownUpdate()
        {
            var g = new SentinelAlarmGate(5f);
            Assert.IsFalse(g.TryAlarm(false, 0f));
            Assert.IsTrue(g.TryAlarm(true, 1f));
        }
    }
}
