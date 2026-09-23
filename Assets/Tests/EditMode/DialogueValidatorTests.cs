using System.Linq;
using NUnit.Framework;
using Project1028.PlayFoundation;

namespace Project1028.PlayFoundation.Tests
{
    public class DialogueValidatorTests
    {
        private static DialogueData Make(int lineCount, string npcId = "npc_test", bool withDestination = true, string destName = "관리소", float x = 1f)
        {
            return new DialogueData
            {
                npcId = npcId,
                lines = lineCount < 0 ? null : Enumerable.Range(0, lineCount).Select(i => $"line {i}").ToArray(),
                destination = withDestination ? new DestinationData { name = destName, x = x, y = 0f, z = 2f } : null
            };
        }

        [Test]
        public void ZeroLines_IsError()
        {
            var r = DialogueValidator.Validate(Make(0));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void NullLines_IsError()
        {
            var r = DialogueValidator.Validate(Make(-1));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void TwoLines_IsWarningNotError()
        {
            var r = DialogueValidator.Validate(Make(2));
            Assert.IsFalse(r.IsError);
            Assert.IsTrue(r.HasWarnings);
            Assert.IsTrue(r.Messages.Any(m => m.Contains("3~5줄")));
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void ThreeToFiveLines_IsClean(int n)
        {
            var r = DialogueValidator.Validate(Make(n));
            Assert.IsFalse(r.IsError);
            Assert.IsEmpty(r.Messages);
        }

        [Test]
        public void SixLines_IsWarning()
        {
            var r = DialogueValidator.Validate(Make(6));
            Assert.IsFalse(r.IsError);
            Assert.IsTrue(r.Messages.Any(m => m.Contains("3~5줄")));
        }

        [Test]
        public void EmptyNpcId_IsError()
        {
            var r = DialogueValidator.Validate(Make(4, npcId: ""));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void MissingDestination_IsError()
        {
            var r = DialogueValidator.Validate(Make(4, withDestination: false));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void EmptyDestinationName_IsError()
        {
            var r = DialogueValidator.Validate(Make(4, destName: " "));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void NaNCoordinate_IsError()
        {
            var r = DialogueValidator.Validate(Make(4, x: float.NaN));
            Assert.IsTrue(r.IsError);
        }

        [Test]
        public void NullData_IsError()
        {
            var r = DialogueValidator.Validate(null);
            Assert.IsTrue(r.IsError);
        }
    }
}
