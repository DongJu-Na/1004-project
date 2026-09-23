using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Project1028.Encounter;
using Project1028.Suspicion;

namespace Project1028.Encounter.Tests
{
    public class CandidateFilterTests
    {
        private static EncounterDef D(string id, string type = "D", string tod = "any", string minZone = "Calm", string req = null, string forbid = null, string variantOf = null) =>
            new EncounterDef { id = id, type = type, lines = new[] { "a", "b", "c" }, npcs = new[] { new NpcDef() }, outcome = new OutcomeDef(), conditions = new ConditionDef { timeOfDay = tod, minZone = minZone, requiresFlag = req, forbidsFlag = forbid }, variantOf = variantOf };

        private static readonly PoolDef[] Pools =
        {
            new PoolDef { poolId = "isl", islandId = "island_test", encounterIds = new[] { "isl_only" } },
            new PoolDef { poolId = "other", islandId = "island_other", encounterIds = new[] { "other_only" } },
            new PoolDef { poolId = "common", islandId = "common", encounterIds = new[] { "c_any", "c_night", "c_tension", "c_req", "c_forbid", "c_var" } },
        };
        private static EncounterDef[] Defs() => new[]
        {
            D("isl_only"), D("other_only"), D("c_any"), D("c_night", tod: "night"), D("c_tension", minZone: "Tension"), D("c_req", req: "k"), D("c_forbid", forbid: "k"),
            D("c_var", type: "A", variantOf: "c_any"),
        };
        private static readonly CallbackEntry[] Cbs = { new CallbackEntry { flag = "unlock", targetKind = "encounter_variant", targetId = "c_var" } };

        private static List<string> Ids(DayPhase phase = DayPhase.Day, AlertZone zone = AlertZone.Calm, string[] flags = null, string[] hist = null) =>
            CandidateFilter.Filter(Defs(), Pools, "island_test", phase, zone, new HashSet<string>(flags ?? new string[0]), new HashSet<string>(hist ?? new string[0]), Cbs).Select(d => d.id).ToList();

        [Test] public void OtherIsland_Excluded() { var ids = Ids(); Assert.Contains("isl_only", ids); Assert.IsFalse(ids.Contains("other_only")); Assert.Contains("c_any", ids); }
        [Test] public void Night_Only_ExcludedInDay() { Assert.IsFalse(Ids().Contains("c_night")); Assert.Contains("c_night", Ids(DayPhase.Night)); }
        [Test] public void MinZone() { Assert.IsFalse(Ids().Contains("c_tension")); Assert.Contains("c_tension", Ids(zone: AlertZone.Tension)); }
        [Test] public void RequiresFlag() { Assert.IsFalse(Ids().Contains("c_req")); Assert.Contains("c_req", Ids(flags: new[] { "k" })); }
        [Test] public void ForbidsFlag() { Assert.Contains("c_forbid", Ids()); Assert.IsFalse(Ids(flags: new[] { "k" }).Contains("c_forbid")); }
        [Test] public void History_ExcludesOriginal() => Assert.IsFalse(Ids(hist: new[] { "c_any" }).Contains("c_any"));
        [Test] public void Variant_NeedsOriginalSeenAndFlag()
        {
            Assert.IsFalse(Ids().Contains("c_var"));
            Assert.IsFalse(Ids(hist: new[] { "c_any" }).Contains("c_var"));
            Assert.IsFalse(Ids(flags: new[] { "unlock" }).Contains("c_var"));
            Assert.Contains("c_var", Ids(flags: new[] { "unlock" }, hist: new[] { "c_any" }));
        }
    }
}
