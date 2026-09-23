using System;
using System.IO;
using UnityEngine;
using Project1028.Suspicion;

namespace Project1028.Report
{
    public static class ReportRulesLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Report", "report_rules.json");

        public static bool TryLoad(SuspicionRules suspicionRules, out ReportRules rules, out ReportValidationResult result)
        {
            rules = null;
            result = new ReportValidationResult();
            string path = Path;
            if (!File.Exists(path)) { result.Error($"신고 규칙 파일 없음: {path}"); return false; }
            try { rules = JsonUtility.FromJson<ReportRules>(File.ReadAllText(path)); }
            catch (Exception ex) { result.Error($"신고 규칙 파싱 실패: {path} ({ex.GetType().Name}: {ex.Message})"); return false; }
            Func<string, bool> exists = id => suspicionRules == null || suspicionRules.FindEvent(id) != null;
            result = ReportRulesValidator.Validate(rules, exists);
            return !result.IsError;
        }
    }
}
