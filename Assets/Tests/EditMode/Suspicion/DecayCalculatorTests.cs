using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class DecayCalculatorTests
    {
        [Test] public void BeforeInterval_Zero() => Assert.AreEqual(0, DecayCalculator.PersonalSteps(0f, 0f, 59f, 60f));
        [Test] public void AtInterval_One() => Assert.AreEqual(1, DecayCalculator.PersonalSteps(0f, 0f, 60f, 60f));
        [Test] public void TwoIntervals_Two() => Assert.AreEqual(2, DecayCalculator.PersonalSteps(0f, 0f, 125f, 60f));
        [Test] public void UsesLaterOfRaisedOrDecay() => Assert.AreEqual(1, DecayCalculator.PersonalSteps(0f, 60f, 125f, 60f));
        [Test] public void RecentRaise_Resets() => Assert.AreEqual(0, DecayCalculator.PersonalSteps(100f, 0f, 125f, 60f));
        [Test] public void IslandAmount_HalfPerMinute_60s() => Assert.AreEqual(0.5f, DecayCalculator.IslandAmount(60f, 0.5f), 1e-5f);
        [Test] public void IslandAmount_Zero() => Assert.AreEqual(0f, DecayCalculator.IslandAmount(0f, 0.5f));
        [Test] public void ZeroInterval_Zero() => Assert.AreEqual(0, DecayCalculator.PersonalSteps(0f, 0f, 500f, 0f));
    }
}
