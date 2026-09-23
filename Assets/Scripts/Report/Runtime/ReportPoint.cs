using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project1028.Report
{
    /// <summary>§5.1 신고 지점(레벨 배치). 전화기는 끊을 수 있고 관리소는 없다. 끊긴 전화기는 런 동안 사용 불가(FR-004).</summary>
    [DisallowMultipleComponent]
    public sealed class ReportPoint : MonoBehaviour
    {
        private static readonly List<ReportPoint> all = new List<ReportPoint>();
        public static IReadOnlyList<ReportPoint> All => all;
        public static event Action<ReportPoint> OnUsabilityChanged;

        [SerializeField] private string id = "phone_a";
        [SerializeField] private ReportPointKind kind = ReportPointKind.Phone;
        [SerializeField] private string displayName = "전화기 A";
        [SerializeField] private bool isUsable = true;

        public string Id => id;
        public ReportPointKind Kind => kind;
        public string DisplayName => displayName;
        public bool IsUsable => isUsable && isActiveAndEnabled;
        public bool CanBeCut => kind == ReportPointKind.Phone;
        public Vector3 Position => transform.position;

        public void Configure(string newId, ReportPointKind newKind, string newDisplayName)
        {
            id = newId; kind = newKind; displayName = newDisplayName;
        }

        public void SetUsable(bool usable)
        {
            if (isUsable == usable) return;
            isUsable = usable;
            OnUsabilityChanged?.Invoke(this);
        }

        private void OnEnable() { all.Add(this); OnUsabilityChanged?.Invoke(this); }
        private void OnDisable() { all.Remove(this); OnUsabilityChanged?.Invoke(this); }
    }
}
