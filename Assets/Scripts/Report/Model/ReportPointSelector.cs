using System.Collections.Generic;
using UnityEngine;

namespace Project1028.Report
{
    /// <summary>사용 가능한 신고 지점 중 가장 가까운 것. 순수 로직.</summary>
    public static class ReportPointSelector
    {
        public struct PointInfo
        {
            public string Id;
            public Vector3 Position;
            public bool Usable;
            public PointInfo(string id, Vector3 pos, bool usable) { Id = id; Position = pos; Usable = usable; }
        }

        public static string PickNearestUsable(IReadOnlyList<PointInfo> points, Vector3 from)
        {
            if (points == null) return null;
            string best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (!points[i].Usable) continue;
                float d = Vector3.Distance(points[i].Position, from);
                if (d < bestDist) { bestDist = d; best = points[i].Id; }
            }
            return best;
        }
    }
}
