namespace Project1028.Report
{
    /// <summary>§5.1 신고 지점 종류. 전화기만 끊을 수 있다.</summary>
    public enum ReportPointKind { Phone, Office }

    public enum ReportPhase { Moving, Reporting, Completed, Abandoned, Interrupted }

    public static class ReportNames
    {
        public static string Korean(ReportPointKind k) => k == ReportPointKind.Phone ? "전화기" : "관리소";
        public static string Korean(ReportPhase p)
        {
            switch (p)
            {
                case ReportPhase.Moving: return "이동 중";
                case ReportPhase.Reporting: return "신고 중";
                case ReportPhase.Completed: return "완료";
                case ReportPhase.Abandoned: return "포기";
                default: return "중단";
            }
        }
    }
}
