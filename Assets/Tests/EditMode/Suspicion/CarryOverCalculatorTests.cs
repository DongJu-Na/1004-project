using System.Collections.Generic;
using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class CarryOverCalculatorTests
    {
        private static readonly CarryOverRule Rule = new CarryOverRule { personalFactor = 0.5f, islandFactor = 0.5f, islandMin = 5, status = "확정대기" };
        private static readonly List<(string, string, int)> None = new List<(string, string, int)>();

        [Test] public void Island80_Is40() => Assert.AreEqual(40, CarryOverCalculator.Compute(None, 80, Rule).Island);
        [Test] public void Island4_ClampsToMin5() => Assert.AreEqual(5, CarryOverCalculator.Compute(None, 4, Rule).Island);
        [Test] public void Island0_IsMin_NotZero() => Assert.AreEqual(5, CarryOverCalculator.Compute(None, 0, Rule).Island);

        [Test] public void Personal_FloorsByFactor()
        {
            var list = new List<(string, string, int)> { ("n", "p3", 3), ("n", "p1", 1), ("n", "p2", 2) };
            var snap = CarryOverCalculator.Compute(list, 0, Rule);
            Assert.AreEqual(1, snap.Personal[("n", "p3")]);
            Assert.AreEqual(0, snap.Personal[("n", "p1")]);
            Assert.AreEqual(1, snap.Personal[("n", "p2")]);
        }
    }
}
