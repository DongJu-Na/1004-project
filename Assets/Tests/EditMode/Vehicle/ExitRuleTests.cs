using NUnit.Framework;
using Project1028.Vehicle;

namespace Project1028.Vehicle.Tests
{
    public class ExitRuleTests
    {
        [TestCase(0f, true)] [TestCase(1.5f, true)] [TestCase(1.51f, false)] [TestCase(-1f, true)] [TestCase(-2f, false)]
        public void CanExit(float speed, bool expected) => Assert.AreEqual(expected, ExitRule.CanExit(speed, 1.5f));
    }
}
