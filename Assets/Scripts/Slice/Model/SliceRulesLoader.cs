using System;
using System.IO;
using UnityEngine;
using Project1028.Suspicion;

namespace Project1028.Slice
{
    public static class SliceRulesLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Slice", "slice_rules.json");

        public static bool TryLoad(SuspicionRules suspicionRules, out SliceRules rules, out SliceValidationResult result)
        {
            rules = null;
            result = new SliceValidationResult();
            string path = Path;
            if (!File.Exists(path)) { result.Error($"슬라이스 규칙 파일 없음: {path}"); return false; }
            try { rules = JsonUtility.FromJson<SliceRules>(File.ReadAllText(path)); }
            catch (Exception ex) { result.Error($"슬라이스 규칙 파싱 실패: {ex.Message}"); return false; }
            Func<string, bool> exists = id => suspicionRules == null || suspicionRules.FindEvent(id) != null;
            result = SliceRulesValidator.Validate(rules, exists);
            return !result.IsError;
        }
    }
}
