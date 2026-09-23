using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.PlayFoundation.Tests
{
    public class InteractableSelectorTests
    {
        private static readonly Vector3 Player = Vector3.zero;
        private static readonly Vector3 Fwd = Vector3.forward;

        private static InteractableSelector.Candidate C(float x, float z, float range, string tag) =>
            new InteractableSelector.Candidate(new Vector3(x, 0f, z), range, tag);

        [Test]
        public void NoCandidates_ReturnsNull()
        {
            Assert.IsNull(InteractableSelector.Pick(new List<InteractableSelector.Candidate>(), Player, Fwd));
            Assert.IsNull(InteractableSelector.Pick(null, Player, Fwd));
        }

        [Test]
        public void OutOfRange_IsExcluded()
        {
            var list = new List<InteractableSelector.Candidate> { C(0f, 5f, 2.5f, "far") };
            Assert.IsNull(InteractableSelector.Pick(list, Player, Fwd));
        }

        [Test]
        public void PrefersSmallerAngleToCameraForward()
        {
            var list = new List<InteractableSelector.Candidate>
            {
                C(2f, 0.5f, 5f, "side"),   // ~76°
                C(0.3f, 2f, 5f, "front"),  // ~8.5°
            };
            var picked = InteractableSelector.Pick(list, Player, Fwd);
            Assert.IsNotNull(picked);
            Assert.AreEqual("front", picked.Value.Payload);
        }

        [Test]
        public void AngleTie_PrefersCloser()
        {
            var list = new List<InteractableSelector.Candidate>
            {
                C(0f, 2.0f, 5f, "far-front"),
                C(0f, 1.0f, 5f, "near-front"),
            };
            var picked = InteractableSelector.Pick(list, Player, Fwd);
            Assert.AreEqual("near-front", picked.Value.Payload);
        }

        [Test]
        public void AllOutOfRange_ReturnsNull()
        {
            var list = new List<InteractableSelector.Candidate> { C(0f, 3f, 1f, "a"), C(3f, 0f, 1f, "b") };
            Assert.IsNull(InteractableSelector.Pick(list, Player, Fwd));
        }

        [Test]
        public void IgnoresVerticalComponent()
        {
            // 위쪽 높이 차만 있는 후보: 수평 거리 0 → 각도 0, 거리 0
            var list = new List<InteractableSelector.Candidate>
            {
                new InteractableSelector.Candidate(new Vector3(0f, 3f, 1f), 2f, "high-front"),
                C(1f, 1f, 2f, "diag"),
            };
            var picked = InteractableSelector.Pick(list, Player, Fwd);
            Assert.AreEqual("high-front", picked.Value.Payload);
        }

        [Test]
        public void UsesHorizontalCameraForward()
        {
            var tiltedForward = new Vector3(0f, -0.7f, 0.7f); // 아래를 보는 카메라
            var list = new List<InteractableSelector.Candidate> { C(0f, 2f, 5f, "front") };
            var picked = InteractableSelector.Pick(list, Player, tiltedForward);
            Assert.AreEqual("front", picked.Value.Payload);
        }
    }
}
