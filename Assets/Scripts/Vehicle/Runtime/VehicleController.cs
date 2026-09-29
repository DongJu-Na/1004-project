using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    /// <summary>
    /// Rigidbody 아케이드 주행: 4점 레이캐스트 서스펜션, 접지 시 전진력·조향 토크·제동·측면 감쇠. 뒤집힘 자동 복구, 경계 리셋.
    /// NPC와 접촉하면 속도를 크게 줄이고 NPC는 건드리지 않는다 (FR-015·FR-016, 헌장 원칙 V). 게임 규칙(소음→의심 등)은 없다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        public static VehicleController Instance { get; private set; }

        // 차체(Cube 프리미티브)의 단위 공간 좌표. 스케일이 자동 적용되므로 ±0.5 안에 둔다. y=0(중심)에서 아래로 쏜다.
        [SerializeField] private Vector3[] suspensionPoints =
        {
            new Vector3(-0.4f, 0f, 0.35f), new Vector3(0.4f, 0f, 0.35f),
            new Vector3(-0.4f, 0f, -0.35f), new Vector3(0.4f, 0f, -0.35f),
        };
        [SerializeField] private Transform[] wheelVisuals = new Transform[0];

        private Rigidbody rb;
        private VehicleParams p = VehicleParams.Default;
        private FlipDetector flip;
        private Vector3 spawnPos;
        private Quaternion spawnRot;
        private float throttle, steer, brake;
        private int groundedCount;
        private float wheelSpin;

        public VehicleParams Params => p;
        public bool EngineOn { get; set; }
        public bool IsGrounded => groundedCount >= 2;
        public bool IsFlipped => flip != null && flip.IsFlipped;
        /// <summary>전진 방향 부호 속도(m/s).</summary>
        public float Speed => Vector3.Dot(rb.linearVelocity, transform.forward);
        public bool IsMoving => rb.linearVelocity.magnitude > p.movingSpeedThreshold;
        public VehicleSeats Seats { get; private set; }

        public VehicleState State => new VehicleState
        {
            Speed = Speed, IsMoving = IsMoving, EngineOn = EngineOn,
            DriverId = Seats != null && Seats.Driver != null ? Seats.Driver.Id : null,
            PassengerId = Seats != null && Seats.Passenger != null ? Seats.Passenger.Id : null,
            IsFlipped = IsFlipped, IsGrounded = IsGrounded,
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Debug.LogError("[Vehicle] 차량은 1대만 (FR-001)"); Destroy(gameObject); return; }
            Instance = this;
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Seats = GetComponent<VehicleSeats>();
            spawnPos = transform.position;
            spawnRot = transform.rotation;

            if (!VehicleParamsLoader.TryLoad(out p, out var result))
                foreach (var m in result.Messages) Debug.LogWarning("[Vehicle] " + m);
            flip = new FlipDetector(p.flipUpThreshold, p.flipRecoverSeconds);
        }

        private void Start()
        {
            RuntimeHud.Instance?.Warn($"차량 파라미터: StreamingAssets/Vehicle/vehicle_params.json (max {p.maxSpeed} m/s)");
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void SetInput(float newThrottle, float newSteer, float newBrake)
        {
            throttle = Mathf.Clamp(newThrottle, -1f, 1f);
            steer = Mathf.Clamp(newSteer, -1f, 1f);
            brake = Mathf.Clamp01(newBrake);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            ApplySuspension(dt);

            if (transform.position.y < p.resetY) { ResetToSpawn(); return; }
            if (flip.Tick(transform.up.y, dt)) Recover();

            if (!IsGrounded) return;

            float fwdSpeed = Speed;
            Vector3 vel = rb.linearVelocity;

            // 전진/후진
            if (EngineOn && Mathf.Abs(throttle) > 0.01f)
            {
                bool underLimit = throttle > 0f ? fwdSpeed < p.maxSpeed : fwdSpeed > -p.reverseMaxSpeed;
                if (underLimit) rb.AddForce(transform.forward * throttle * p.acceleration * rb.mass, ForceMode.Force);
            }
            // 제동 (데드밴드: 한 스텝에 멈출 수 있으면 전진 속도 성분을 0으로)
            if (brake > 0.01f && Mathf.Abs(fwdSpeed) > 0.05f)
            {
                float stepDelta = brake * p.brakeForce * dt;
                if (Mathf.Abs(fwdSpeed) <= stepDelta) rb.linearVelocity = vel - transform.forward * fwdSpeed;
                else rb.AddForce(-transform.forward * Mathf.Sign(fwdSpeed) * brake * p.brakeForce * rb.mass, ForceMode.Force);
            }
            // 조향 (속도에 비례, 고속에서 감쇠, 후진 시 반전)
            if (Mathf.Abs(steer) > 0.01f && Mathf.Abs(fwdSpeed) > 0.2f)
            {
                float speedFactor = Mathf.Clamp01(Mathf.Abs(fwdSpeed) / 3f);
                float falloff = 1f - p.steerSpeedFalloff * Mathf.Clamp01(Mathf.Abs(fwdSpeed) / p.maxSpeed);
                rb.AddTorque(Vector3.up * steer * p.steerTorque * speedFactor * falloff * Mathf.Sign(fwdSpeed) * rb.mass, ForceMode.Force);
            }
            // 측면 그립: 스텝마다 측면 속도의 일정 비율을 제거 (배처럼 미끄러지지 않게)
            Vector3 lateral = Vector3.Project(rb.linearVelocity, transform.right);
            rb.AddForce(-lateral * Mathf.Clamp01(p.lateralGrip), ForceMode.VelocityChange);

            wheelSpin += fwdSpeed * dt * Mathf.Rad2Deg / 0.35f; // 바퀴 반지름 0.35m
            foreach (var w in wheelVisuals) if (w != null) w.localRotation = Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(0f, wheelSpin, 0f); // 실린더 축(Y) 기준 굴림
        }

        private void ApplySuspension(float dt)
        {
            groundedCount = 0;
            // 차체 중심에서 아래로: 차체 절반 높이 + 서스펜션 길이. 바닥까지 거리에서 절반 높이를 뺀 값이 서스펜션 압축량이다.
            float halfHeight = transform.lossyScale.y * 0.5f;
            float travel = p.suspensionLength;
            float rayLen = halfHeight + travel;
            foreach (var local in suspensionPoints)
            {
                Vector3 origin = transform.TransformPoint(local);
                if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLen, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.IsChildOf(transform)) continue;
                    groundedCount++;
                    float compression = Mathf.Clamp01(1f - (hit.distance - halfHeight) / travel);
                    float pointVel = Vector3.Dot(rb.GetPointVelocity(origin), transform.up);
                    float force = compression * p.suspensionSpring - pointVel * p.suspensionDamper;
                    rb.AddForceAtPosition(transform.up * force * rb.mass * 0.25f, origin, ForceMode.Force);
                }
            }
        }

        /// <summary>뒤집힘 복구: yaw만 유지해 세우고 속도 0.</summary>
        public void Recover()
        {
            transform.position += Vector3.up * 0.8f;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            flip.Reset();
            VehicleEvents.RaiseRecovered();
            RuntimeHud.Instance?.Warn("차량 복구 (뒤집힘)");
        }

        /// <summary>경계 이탈: 스폰으로 복귀. 탑승자는 좌석 자식이라 함께 돌아온다.</summary>
        public void ResetToSpawn()
        {
            transform.SetPositionAndRotation(spawnPos, spawnRot);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            flip.Reset();
            VehicleEvents.RaiseReset();
            RuntimeHud.Instance?.Warn("차량 경계 이탈 → 시작 위치 복귀");
        }

        // FR-015·FR-016: NPC와 닿으면 차량만 감속. NPC의 위치·자세·상태는 건드리지 않는다. 피해·기절 개념 없음 (원칙 V).
        private void OnCollisionEnter(Collision c) => DampIfNpc(c);
        private void OnCollisionStay(Collision c) => DampIfNpc(c);
        private void DampIfNpc(Collision c)
        {
            if (c.collider == null || c.collider.GetComponentInParent<NpcIdentity>() == null) return;
            rb.linearVelocity *= 0.3f;
        }
    }
}
