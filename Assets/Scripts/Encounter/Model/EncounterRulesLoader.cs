using System;
using System.IO;
using UnityEngine;
using Project1028.Suspicion;

namespace Project1028.Encounter
{
    public static class EncounterRulesLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Encounter", "encounters.json");

        public static bool TryLoad(SuspicionRules suspicionRules, out EncounterRules rules, out EncounterValidationResult result)
        {
            rules = null;
            result = new EncounterValidationResult();
            string path = Path;
            if (!File.Exists(path)) { result.Error($"인카운터 파일 없음: {path}"); return false; }
            try { rules = JsonUtility.FromJson<EncounterRules>(File.ReadAllText(path)); }
            catch (Exception ex) { result.Error($"인카운터 파싱 실패: {path} ({ex.GetType().Name}: {ex.Message})"); return false; }
            Func<string, bool> exists = id => suspicionRules == null || suspicionRules.FindEvent(id) != null;
            result = EncounterRulesValidator.Validate(rules, exists);
            return !result.IsError;
        }
    }
}
