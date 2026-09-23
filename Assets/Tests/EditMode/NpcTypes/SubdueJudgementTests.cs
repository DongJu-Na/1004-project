using NUnit.Framework;
using Project1028.NpcTypes;

namespace Project1028.NpcTypes.Tests
{
    public class SubdueJudgementTests
    {
        [Test] public void Antagonist_CanSubdue()
        {
            var v = SubdueJudgement.Evaluate(Faction.Antagonist);
            Assert.IsTrue(v.CanSubdue); Assert.IsTrue(v.PhysicalReaction); Assert.IsNotEmpty(v.Reason);
        }
        [TestCase(Faction.Victim)] [TestCase(Faction.Neutral)]
        public void VictimNeutral_Cannot_NoReaction(Faction f)
        {
            var v = SubdueJudgement.Evaluate(f);
            Assert.IsFalse(v.CanSubdue); Assert.IsFalse(v.PhysicalReaction); Assert.IsNotEmpty(v.Reason);
        }
        [Test] public void DefaultFactions_OnlyWatcherSubduable()
        {
            Assert.IsTrue(SubdueJudgement.Evaluate(NpcTypeDefinitions.DefaultFactionOf(NpcType.Watcher)).CanSubdue);
            foreach (var t in new[] { NpcType.Informer, NpcType.Sympathizer, NpcType.Silent, NpcType.Sentinel })
                Assert.IsFalse(SubdueJudgement.Evaluate(NpcTypeDefinitions.DefaultFactionOf(t)).CanSubdue, t.ToString());
        }
    }
}
