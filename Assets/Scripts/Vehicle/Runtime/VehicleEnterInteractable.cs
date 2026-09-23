using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    /// <summary>"탑승 [E]" 안내. 빈 좌석이 있을 때만.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VehicleSeats))]
    public sealed class VehicleEnterInteractable : MonoBehaviour, IInteractable
    {
        private VehicleSeats seats;
        private VehicleController controller;
        private void Awake() { seats = GetComponent<VehicleSeats>(); controller = GetComponent<VehicleController>(); }

        public string PromptText => seats.Driver == null ? "탑승 (운전석)" : "탑승 (동승석)";
        public float InteractionRange => controller != null ? controller.Params.enterRange : 3f;
        public Vector3 WorldPosition => transform.position;
        public bool CanInteract(PlayerEntity player) => player != null && !player.IsLocked && !player.IsInVehicle && !player.HandsBusy && seats.HasFreeSeat;
        public void Interact(PlayerEntity player) => seats.TryEnter(player);
    }
}
