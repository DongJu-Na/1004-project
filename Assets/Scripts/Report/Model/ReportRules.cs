using System;

namespace Project1028.Report
{
    /// <summary>report_rules.json 모델. 스키마: contracts/report-rules-schema.json</summary>
    [Serializable]
    public sealed class ReportRules
    {
        public string version;
        public float minWalkSeconds;
        public float reportDurationSeconds;
        public float cutDurationSeconds;
        public bool cutRequiresTool;
        public float cutRange;
        public float arriveDistance;
        public string completedEventId;
        public string status;
    }
}
