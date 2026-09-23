using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project1028.PlayFoundation
{
    public enum MovementState { Idle, Walking, Sprinting }

    /// <summary>
    /// 플레이어 개체 하나. 이동 상태 집계, 잠금(대화 등), 수령 목적지, 씬 내 등록을 담당한다.
    /// 게임 규칙(의심·진영·위협) 필드는 두지 않는다 (헌장 원칙 I).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerEntity : MonoBehaviour
    {
        private static readonly List<PlayerEntity> all = new List<PlayerEntity>();
        public static IReadOnlyList<PlayerEntity> All => all;
        public static event Action<PlayerEntity> OnRegistered;
        public static event Action<PlayerEntity> OnUnregistered;

        [SerializeField] private string id = "P1";
        [Tooltip("시야 판정 등에 쓰는 몸 중심의 pivot 기준 높이. 프리미티브 Capsule은 pivot이 중심이므로 0.")]
        [SerializeField] private float centerHeight = 0f;

        private readonly HashSet<object> lockOwners = new HashSet<object>();
        private readonly List<Destination> receivedDestinations = new List<Destination>();
        private ThirdPersonMotor motor;

        public string Id => id;
        public bool IsLocked => lockOwners.Count > 0;
        public Vector3 Position => transform.position;
        public Vector3 CenterPosition => transform.position + Vector3.up * centerHeight;
        public Vector3 Forward => transform.forward;
        public OrbitCamera Camera { get; set; }
        /// <summary>두 손이 점유되어 일반 상호작용(대화 등)이 불가한 상태. 004 PlayerHands가 설정.</summary>
        public bool HandsBusy { get; set; }
        /// <summary>차량 탑승 중. 006 VehicleSeats가 설정. 도보 조작·상호작용 안내가 꺼진다.</summary>
        public bool IsInVehicle { get; set; }
        public Vector3 SpawnPosition { get; private set; }
        public IReadOnlyCollection<Destination> ReceivedDestinations => receivedDestinations;

        public MovementState MovementState
        {
            get
            {
                if (motor == null) return MovementState.Idle;
                return motor.CurrentState;
            }
        }

        public void Configure(string newId, float newCenterHeight)
        {
            id = newId;
            centerHeight = newCenterHeight;
        }

        private void Awake()
        {
            motor = GetComponent<ThirdPersonMotor>();
            SpawnPosition = transform.position;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError($"[PlayerEntity] Id가 비어 있습니다: {name}", this);
            }
        }

        private void OnEnable()
        {
            foreach (var other in all)
            {
                if (other != this && other.id == id)
                {
                    Debug.LogError($"[PlayerEntity] Id 중복: {id} ({name}, {other.name})", this);
                }
            }
            all.Add(this);
            OnRegistered?.Invoke(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
            OnUnregistered?.Invoke(this);
        }

        /// <summary>이동·카메라 잠금. 소유자가 하나라도 남아 있으면 잠긴 상태다.</summary>
        public void Lock(object owner)
        {
            if (owner == null) return;
            lockOwners.Add(owner);
            ApplyLock();
        }

        public void Unlock(object owner)
        {
            if (owner == null) return;
            lockOwners.Remove(owner);
            ApplyLock();
        }

        private void ApplyLock()
        {
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
            if (motor != null) motor.SetMovementEnabled(!IsLocked);
        }

        public bool HasDestinationFrom(string npcId)
        {
            for (int i = 0; i < receivedDestinations.Count; i++)
            {
                if (receivedDestinations[i].SourceNpcId == npcId) return true;
            }
            return false;
        }

        internal void AddDestination(Destination destination)
        {
            if (destination == null) return;
            receivedDestinations.Add(destination);
        }
    }
}
