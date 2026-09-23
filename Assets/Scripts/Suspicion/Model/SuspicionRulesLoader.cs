using System;
using System.IO;
using UnityEngine;

namespace Project1028.Suspicion
{
    public static class SuspicionRulesLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Suspicion", "suspicion_rules.json");

        public static bool TryLoad(out SuspicionRules rules, out RulesValidationResult result)
        {
            rules = null;
            result = new RulesValidationResult();
            string path = Path;
            if (!File.Exists(path)) { result.Error($"규칙 파일 없음: {path}"); return false; }
            try
            {
                rules = JsonUtility.FromJson<SuspicionRules>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                result.Error($"규칙 파일 파싱 실패: {path} ({ex.GetType().Name}: {ex.Message})");
                return false;
            }
            result = SuspicionRulesValidator.Validate(rules);
            return !result.IsError;
        }
    }
}
