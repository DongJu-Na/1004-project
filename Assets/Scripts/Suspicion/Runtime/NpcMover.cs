using UnityEngine;

namespace Project1028.Suspicion
{
    /// <summary>
    /// NPC 최소 이동: 지점으로 걷기·대상 따라가기·바라보기. 장애물 회피 없음(평지 테스트용). 005에서 NavMesh로 교체 가능.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speed = 1.6f;
        [SerializeField, Min(0f)] private float turnSpeed = 360f;
        [SerializeField, Min(0.01f)] private float arriveDistance = 0.3f;

        private enum Mode { Idle, MoveTo, Follow }
        private Mode mode = Mode.Idle;
        private Vector3 target;
        private Transform followTarget;
        private float keepDistance;
        private Vector3? faceTarget;

        public float Speed { get => speed; set => speed = Mathf.Max(0f, value); }
        public bool IsMoving { get; private set; }
        public bool HasArrived { get; private set; }

        public void MoveTo(Vector3 worldPos)
        {
            mode = Mode.MoveTo;
            target = worldPos;
            HasArrived = false;
            faceTarget = null;
        }

        public void Follow(Transform t, float distance)
        {
            if (t == null) { Stop(); return; }
            mode = Mode.Follow;
            followTarget = t;
            keepDistance = Mathf.Max(arriveDistance, distance);
            HasArrived = false;
            faceTarget = null;
        }

        public void FaceTowards(Vector3 worldPos)
        {
            faceTarget = worldPos;
        }

        public void Stop()
        {
            mode = Mode.Idle;
            followTarget = null;
            IsMoving = false;
        }

        private void Update()
        {
            Vector3 lookAt = transform.position;
            bool move = false;
            Vector3 dest = transform.position;

            switch (mode)
            {
                case Mode.MoveTo:
                    dest = target;
                    move = true;
                    break;
                case Mode.Follow:
                    if (followTarget == null) { Stop(); break; }
                    dest = followTarget.position;
                    lookAt = dest;
                    move = true;
                    break;
            }

            IsMoving = false;
            if (move)
            {
                Vector3 flat = dest - transform.position;
                flat.y = 0f;
                float stopAt = mode == Mode.Follow ? keepDistance : arriveDistance;
                if (flat.magnitude > stopAt)
                {
                    Vector3 step = flat.normalized * speed * Time.deltaTime;
                    if (step.magnitude > flat.magnitude - stopAt) step = flat.normalized * (flat.magnitude - stopAt);
                    transform.position += step;
                    IsMoving = step.sqrMagnitude > 0f;
                    lookAt = dest;
                }
                else if (mode == Mode.MoveTo)
                {
                    HasArrived = true;
                    mode = Mode.Idle;
                }
            }

            if (faceTarget.HasValue && !IsMoving) lookAt = faceTarget.Value;

            Vector3 dir = lookAt - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 1e-4f)
            {
                Quaternion want = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
            }
        }
    }
}
