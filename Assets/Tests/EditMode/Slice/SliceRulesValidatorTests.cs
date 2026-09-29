using NUnit.Framework;
using Project1028.Slice;

namespace Project1028.Slice.Tests
{
    public class SliceRulesValidatorTests
    {
        private static SliceRules Valid() => new SliceRules
        {
            version = "t",
            clock = new ClockRule { nightAtSeconds = 300f, departureAtSeconds = 600f, nextBoatDelaySeconds = 180f, status = "확정대기" },
            evidence = new EvidenceRule { investigateSeconds = 2.5f, twoHanded = true, seenCooldownSeconds = 3f, oneHandEventId = "evidence_seen_one_hand", twoHandEventId = "evidence_seen_two_hands", status = "확정대기" },
            restricted = new RestrictedRule { cooldownSeconds = 5f, eventId = "enter_restricted_area", status = "확정대기" },
            dock = new DockRule { boardRadius = 6f, requireEvidenceForSuccess = true, status = "확정" },
            vehicle = new VehicleRule { allowEvidenceAboard = true },
            objectives = new[] { new ObjectiveStep { id = "a", text = "t", markerFacilityId = "w", condition = "evidence_picked" } },
            hints = new[] { "h" }, hintSeconds = 30f,
            feedback = new FeedbackRule
            {
                heartbeat = new HeartbeatSetting { intervalsByStage = new[] { 2f, 1.2f, 0.7f, 0.4f }, zoneMultiplier = new[] { 1f, 0.9f, 0.75f, 0.55f }, volume = 0.5f },
                pulse = new PulseSetting { seconds = 0.6f, alpha = 0.45f }, tint = new TintSetting { alphaByZone = new[] { 0f, 0.08f, 0.18f, 0.3f }, nightAlpha = 0.35f },
                hitstop = new HitstopSetting { seconds = 0.1f }, shake = new ShakeSetting { amplitude = 0.25f, seconds = 0.25f }, bigText = new BigTextSetting { seconds = 1.2f },
            },
        };

        [Test] public void Valid_Ok_Pending3() { var r = SliceRulesValidator.Validate(Valid(), _ => true); Assert.IsFalse(r.IsError, string.Join("\n", r.Messages)); Assert.AreEqual(3, r.PendingCount); }
        [Test] public void DepartureBeforeNight_Error() { var v = Valid(); v.clock.departureAtSeconds = 200f; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void NoObjectives_Error() { var v = Valid(); v.objectives = new ObjectiveStep[0]; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void BadCondition_Error() { var v = Valid(); v.objectives[0].condition = "nope"; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void HeartbeatLength3_Error() { var v = Valid(); v.feedback.heartbeat.intervalsByStage = new[] { 1f, 1f, 1f }; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void Volume15_Error() { var v = Valid(); v.feedback.heartbeat.volume = 1.5f; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void UnknownEvent_Error() => Assert.IsTrue(SliceRulesValidator.Validate(Valid(), _ => false).IsError);
        [Test] public void NoHints_Error() { var v = Valid(); v.hints = new string[0]; Assert.IsTrue(SliceRulesValidator.Validate(v, _ => true).IsError); }
    }
}
