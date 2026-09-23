using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    /// <summary>FR-017 차량 상태 패널 + V(P2 동승 토글, 디버그).</summary>
    public sealed class VehicleDebugPanel : MonoBehaviour
    {
        private Text text;
        private float nextRefresh;
        private readonly Queue<string> log = new Queue<string>();
        private readonly StringBuilder sb = new StringBuilder();

        private void OnEnable()
        {
            VehicleEvents.OnEntered += Entered; VehicleEvents.OnExited += Exited; VehicleEvents.OnRecovered += Recovered; VehicleEvents.OnReset += Reset; VehicleEvents.OnExitDenied += Denied;
        }
        private void OnDisable()
        {
            VehicleEvents.OnEntered -= Entered; VehicleEvents.OnExited -= Exited; VehicleEvents.OnRecovered -= Recovered; VehicleEvents.OnReset -= Reset; VehicleEvents.OnExitDenied -= Denied;
        }
        private void Entered(PlayerEntity p, SeatKind s) => Push($"{p.Id} 탑승 {s}");
        private void Exited(PlayerEntity p, SeatKind s) => Push($"{p.Id} 하차 {s}");
        private void Recovered() => Push("복구");
        private void Reset() => Push("경계 리셋");
        private void Denied(PlayerEntity p, string r) => Push($"{p.Id} 하차 거부: {r}");
        private void Push(string s) { log.Enqueue($"[{Time.time:0.0}] {s}"); while (log.Count > 6) log.Dequeue(); }

        private void Update()
        {
            var kb = Keyboard.current;
            var vehicle = VehicleController.Instance;
            if (kb != null && vehicle != null && kb.vKey.wasPressedThisFrame && PlayerEntity.All.Count > 1)
            {
                var p2 = PlayerEntity.All[1];
                if (p2.IsInVehicle) vehicle.Seats.TryExit(p2); else vehicle.Seats.TryEnter(p2);
            }

            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("VehiclePanel", new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(560f, 360f), 20, TextAnchor.UpperRight);
                text.color = new Color(0.85f, 0.95f, 1f);
            }
            sb.Clear();
            if (vehicle == null) { sb.AppendLine("차량 없음"); text.text = sb.ToString(); return; }
            var s = vehicle.State;
            sb.Append("속도 ").Append(s.Speed.ToString("0.0")).Append(" m/s  이동:").Append(s.IsMoving ? "예" : "아니오").Append("  엔진:").Append(s.EngineOn ? "ON" : "OFF")
              .Append("  접지:").Append(s.IsGrounded ? "예" : "아니오").Append("  뒤집힘:").Append(s.IsFlipped ? "예" : "아니오").AppendLine();
            sb.Append("운전석 ").Append(s.DriverId ?? "-").Append("  동승석 ").Append(s.PassengerId ?? "-").AppendLine();
            sb.AppendLine("— 로그 —");
            foreach (var l in log) sb.AppendLine(l);
            sb.AppendLine("키: E 탑승/하차 · W/S 가속/후진 · A/D 조향 · Shift 제동 · Space 복구 · V P2 동승 토글");
            text.text = sb.ToString();
        }
    }
}
