using System.Collections.Generic;

namespace Project1028.PlayFoundation
{
    public sealed class ValidationResult
    {
        public bool IsError;
        public readonly List<string> Messages = new List<string>();
        public bool HasWarnings => !IsError && Messages.Count > 0;

        public void Error(string message)
        {
            IsError = true;
            Messages.Add(message);
        }

        public void Warning(string message) => Messages.Add(message);
    }

    /// <summary>
    /// 순수 로직: 대화 데이터 검증. Error면 대화 시작 불가, Warning이면 진행하되 경고(FR-014).
    /// 3~5줄 규격은 SDD §6.1과 공유한다.
    /// </summary>
    public static class DialogueValidator
    {
        public const int MinLines = 3;
        public const int MaxLines = 5;

        public static ValidationResult Validate(DialogueData data)
        {
            var result = new ValidationResult();
            if (data == null)
            {
                result.Error("대화 데이터가 null입니다.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(data.npcId))
            {
                result.Error("npcId가 비어 있습니다.");
            }

            if (data.lines == null || data.lines.Length == 0)
            {
                result.Error("대사(lines)가 없습니다.");
            }
            else if (data.lines.Length < MinLines || data.lines.Length > MaxLines)
            {
                result.Warning($"§6.1 3~5줄 규격 위반: {data.lines.Length}줄");
            }

            if (data.destination == null)
            {
                result.Error("destination이 없습니다.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.destination.name))
                {
                    result.Error("destination.name이 비어 있습니다.");
                }
                if (!IsFinite(data.destination.x) || !IsFinite(data.destination.y) || !IsFinite(data.destination.z))
                {
                    result.Error("destination 좌표가 유한값이 아닙니다.");
                }
            }
            return result;
        }

        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
