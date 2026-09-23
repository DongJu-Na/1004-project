using System;
using System.IO;
using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>Assets/StreamingAssets/Dialogue/&lt;npcId&gt;.json 로드 + 검증. 매 호출마다 다시 읽는다(SC-004).</summary>
    public static class DialogueLoader
    {
        public static string GetPath(string npcId) =>
            Path.Combine(Application.streamingAssetsPath, "Dialogue", npcId + ".json");

        public static bool TryLoad(string npcId, out DialogueData data, out ValidationResult result)
        {
            data = null;
            result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(npcId))
            {
                result.Error("npcId가 비어 있어 대화 파일을 찾을 수 없습니다.");
                return false;
            }

            string path = GetPath(npcId);
            if (!File.Exists(path))
            {
                result.Error($"대화 파일 없음: {path}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<DialogueData>(json);
            }
            catch (Exception ex)
            {
                result.Error($"대화 파일 파싱 실패: {path} ({ex.GetType().Name}: {ex.Message})");
                return false;
            }

            result = DialogueValidator.Validate(data);
            return !result.IsError;
        }
    }
}
