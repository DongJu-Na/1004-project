using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>바닥 밖으로 떨어지면 PlayerEntity.SpawnPosition으로 복귀한다 (Edge case).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class FallRespawn : MonoBehaviour
    {
        [SerializeField] private float fallY = -5f;

        private PlayerEntity owner;
        private CharacterController controller;

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (owner.IsInVehicle) return; // 차량 리셋(006)이 담당
            if (transform.position.y >= fallY) return;

            bool hadController = controller != null && controller.enabled;
            if (hadController) controller.enabled = false;
            transform.position = owner.SpawnPosition;
            if (hadController) controller.enabled = true;

            RuntimeHud.Instance?.Warn($"{owner.Id} 낙하 복귀");
        }
    }
}
