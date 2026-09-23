using NUnit.Framework;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.PlayFoundation.Tests
{
    public class VisionEvaluatorTests
    {
        private static readonly Vector3 Eye = new Vector3(0f, 1.6f, 0f);
        private static readonly Vector3 Fwd = Vector3.forward;
        private const float Fov = 90f;
        private const float Range = 10f;

        private static Vector3 AtAngle(float deg, float dist, float y = 1.6f)
        {
            var dir = Quaternion.Euler(0f, deg, 0f) * Vector3.forward;
            var p = Eye + dir * dist;
            p.y = y;
            return p;
        }

        [Test]
        public void Front_InRange_IsInCone()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(0f, 5f), Fov, Range);
            Assert.IsTrue(r.InFov);
            Assert.IsTrue(r.InRange);
            Assert.IsTrue(r.InCone);
            Assert.AreEqual(5f, r.Distance, 0.01f);
            Assert.AreEqual(0f, r.HorizontalAngleDeg, 0.01f);
        }

        [Test]
        public void JustInsideHalfFov_IsInFov()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(44f, 5f), Fov, Range);
            Assert.IsTrue(r.InFov);
            Assert.AreEqual(44f, r.HorizontalAngleDeg, 0.01f);
        }

        [Test]
        public void JustOutsideHalfFov_IsNotInFov()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(46f, 5f), Fov, Range);
            Assert.IsFalse(r.InFov);
            Assert.IsTrue(r.InRange);
            Assert.IsFalse(r.InCone);
        }

        [Test]
        public void Behind_IsNotInFov()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(180f, 3f), Fov, Range);
            Assert.IsFalse(r.InFov);
            Assert.AreEqual(180f, r.HorizontalAngleDeg, 0.01f);
        }

        [Test]
        public void Front_OutOfRange_IsNotInRange()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(0f, 11f), Fov, Range);
            Assert.IsTrue(r.InFov);
            Assert.IsFalse(r.InRange);
            Assert.IsFalse(r.InCone);
        }

        [Test]
        public void Elevated_Front_UsesHorizontalAngle()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(0f, 5f, y: 4.6f), Fov, Range);
            Assert.IsTrue(r.InFov, "높이 차는 수평 판정에 영향이 없어야 한다");
            Assert.AreEqual(0f, r.HorizontalAngleDeg, 0.01f);
            Assert.AreEqual(Mathf.Sqrt(25f + 9f), r.Distance, 0.01f);
        }

        [Test]
        public void ExactlyAtRange_IsInRange()
        {
            var r = VisionEvaluator.Evaluate(Eye, Fwd, AtAngle(0f, 10f), Fov, Range);
            Assert.IsTrue(r.InRange);
        }
    }
}
