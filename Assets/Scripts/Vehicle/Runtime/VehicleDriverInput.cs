using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    /// <summary>운전자 입력 → 차량. Move.y 가속/후진, Move.x 조향, Sprint 제동, Jump 복구, Interact 하차. 동승자는 Interact(하차)만.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VehicleController))]
    [RequireComponent(typeof(VehicleSeats))]
    public sealed class VehicleDriverInput : MonoBehaviour
    {
        private VehicleController controller;
        private VehicleSeats seats;

        private void Awake()
        {
            controller = GetComponent<VehicleController>();
            seats = GetComponent<VehicleSeats>();
        }

        private void Update()
        {
            var driver = seats.Driver;
            if (driver != null && driver.IsLocked)
            {
                controller.SetInput(0f, 0f, 0f); // 대화(인카운터) 중: 운전 입력·하차·복구 무시
            }
            else if (driver != null)
            {
                var input = driver.GetComponent<PlayerInput>();
                var actions = input != null ? input.actions : null;
                if (actions != null)
                {
                    var move = actions.FindAction("Move", false);
                    var sprint = actions.FindAction("Sprint", false);
                    var jump = actions.FindAction("Jump", false);
                    var interact = actions.FindAction("Interact", false);
                    Vector2 m = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
                    controller.SetInput(m.y, m.x, sprint != null && sprint.IsPressed() ? 1f : 0f);
                    if (jump != null && jump.WasPressedThisFrame()) controller.Recover();
                    if (interact != null && interact.WasPressedThisFrame()) seats.TryExit(driver);
                }
                else controller.SetInput(0f, 0f, 0f);
            }
            else controller.SetInput(0f, 0f, 0f);

            var passenger = seats.Passenger;
            if (passenger != null && !passenger.IsLocked)
            {
                var input = passenger.GetComponent<PlayerInput>();
                var interact = input != null && input.actions != null ? input.actions.FindAction("Interact", false) : null;
                if (interact != null && interact.WasPressedThisFrame()) seats.TryExit(passenger); // Move/Sprint는 무시 (FR-007)
            }
        }
    }
}
