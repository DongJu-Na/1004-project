using NUnit.Framework;
using Project1028.NpcTypes;

namespace Project1028.NpcTypes.Tests
{
    public class InformerDeliveryStateTests
    {
        [Test] public void Trigger_IdleToWaiting()
        {
            var s = new InformerDeliveryState();
            Assert.IsTrue(s.Trigger(0f)); Assert.AreEqual(DeliveryPhase.Waiting, s.Phase);
        }
        [Test] public void Tick_BeforeDelay_NoTransition()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f);
            Assert.IsNull(s.Tick(7f, 8f, true)); Assert.AreEqual(DeliveryPhase.Waiting, s.Phase);
        }
        [Test] public void Tick_AfterDelay_Moving()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f);
            Assert.AreEqual(DeliveryPhase.Moving, s.Tick(8f, 8f, true));
        }
        [Test] public void Tick_AfterDelay_NoManager_Held()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f);
            Assert.AreEqual(DeliveryPhase.Held, s.Tick(8f, 8f, false));
        }
        [Test] public void Held_ManagerAppears_Moving()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f); s.Tick(8f, 8f, false);
            Assert.AreEqual(DeliveryPhase.Moving, s.Tick(9f, 8f, true));
        }
        [Test] public void Moving_TriggerAgain_Ignored()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f); s.Tick(8f, 8f, true);
            Assert.IsFalse(s.Trigger(9f)); Assert.AreEqual(DeliveryPhase.Moving, s.Phase);
        }
        [Test] public void Arrive_Delivered_ThenTriggerIgnored()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f); s.Tick(8f, 8f, true); s.Arrive();
            Assert.AreEqual(DeliveryPhase.Delivered, s.Phase); Assert.IsFalse(s.Trigger(20f));
        }
        [Test] public void Reset_ToIdle()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f); s.Reset();
            Assert.AreEqual(DeliveryPhase.Idle, s.Phase);
        }
        [Test] public void Moving_ManagerLost_Held()
        {
            var s = new InformerDeliveryState(); s.Trigger(0f); s.Tick(8f, 8f, true);
            Assert.AreEqual(DeliveryPhase.Held, s.Tick(9f, 8f, false));
        }
    }
}
