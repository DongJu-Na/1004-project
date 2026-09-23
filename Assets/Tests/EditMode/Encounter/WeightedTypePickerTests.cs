using System.Collections.Generic;
using NUnit.Framework;
using Project1028.Encounter;

namespace Project1028.Encounter.Tests
{
    public class WeightedTypePickerTests
    {
        private static EncounterDef D(string id, string type) => new EncounterDef { id = id, type = type };
        private static List<EncounterDef> All() => new List<EncounterDef> { D("a", "A"), D("b", "B"), D("c", "C"), D("d1", "D"), D("d2", "D") };

        [Test] public void Empty_Null() => Assert.IsNull(WeightedTypePicker.Pick(new List<EncounterDef>(), new TypeWeights { A = 1 }, 0.5f, 0.5f));
        [Test] public void OnlyA_Weight_AlwaysA()
        {
            var w = new TypeWeights { A = 1, B = 0, C = 0, D = 0 };
            for (float r = 0f; r < 1f; r += 0.1f) Assert.AreEqual("a", WeightedTypePicker.Pick(All(), w, r, 0.5f).id);
        }
        [Test] public void MissingType_Renormalizes()
        {
            var w = new TypeWeights { A = 1, B = 0, C = 0, D = 0 };
            var noA = new List<EncounterDef> { D("d1", "D") };
            Assert.AreEqual("d1", WeightedTypePicker.Pick(noA, w, 0.5f, 0.5f).id);
        }
        [Test] public void IndexRoll_Deterministic()
        {
            var w = new TypeWeights { A = 0, B = 0, C = 0, D = 1 };
            Assert.AreEqual("d1", WeightedTypePicker.Pick(All(), w, 0.5f, 0.0f).id);
            Assert.AreEqual("d2", WeightedTypePicker.Pick(All(), w, 0.5f, 0.99f).id);
        }
        [Test] public void TypeRoll_Boundaries()
        {
            var w = new TypeWeights { A = 1, B = 1, C = 1, D = 1 };
            Assert.AreEqual(EncounterType.A, EncounterRules.TypeOf(WeightedTypePicker.Pick(All(), w, 0.1f, 0f)));
            Assert.AreEqual(EncounterType.D, EncounterRules.TypeOf(WeightedTypePicker.Pick(All(), w, 0.9f, 0f)));
        }
    }
}
