using NUnit.Framework;
using UnityEngine;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class BackConeCheckTests
    {
        private static readonly Vector3 T = Vector3.zero;
        private static readonly Vector3 F = Vector3.forward;
        private static Vector3 Behind(float deg, float d = 1.5f, float y = 0f) { var p = Quaternion.Euler(0f, deg, 0f) * Vector3.back * d; p.y = y; return p; }

        [Test] public void DirectlyBehind_True() => Assert.IsTrue(BackConeCheck.IsBehind(T, F, Behind(0f), 120f));
        [Test] public void Inside59_True() => Assert.IsTrue(BackConeCheck.IsBehind(T, F, Behind(59f), 120f));
        [Test] public void Outside61_False() => Assert.IsFalse(BackConeCheck.IsBehind(T, F, Behind(61f), 120f));
        [Test] public void Front_False() => Assert.IsFalse(BackConeCheck.IsBehind(T, F, Vector3.forward * 1.5f, 120f));
        [Test] public void HeightIgnored() => Assert.IsTrue(BackConeCheck.IsBehind(T, F, Behind(0f, 1.5f, 3f), 120f));
    }
}
