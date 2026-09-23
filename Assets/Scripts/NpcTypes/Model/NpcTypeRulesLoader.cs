using System;
using System.IO;
using UnityEngine;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    public static class NpcTypeRulesLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "NpcTypes", "npc_types.json");

        public static bool TryLoad(SuspicionRules suspicionRules, out NpcTypeRules rules, out TypeRulesValidationResult result)
        {
            rules = null;
            result = new TypeRulesValidationResult();
            string path = Path;
            if (!File.Exists(path)) { result.Error($"유형 규칙 파일 없음: {path}"); return false; }
            try { rules = JsonUtility.FromJson<NpcTypeRules>(File.ReadAllText(path)); }
            catch (Exception ex) { result.Error($"유형 규칙 파싱 실패: {path} ({ex.GetType().Name}: {ex.Message})"); return false; }

            Func<string, bool> exists = id => suspicionRules == null || suspicionRules.FindEvent(id) != null;
            result = NpcTypeRulesValidator.Validate(rules, exists);
            return !result.IsError;
        }
    }
}
