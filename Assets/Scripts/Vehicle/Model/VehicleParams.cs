using System;

namespace Project1028.Vehicle
{
    /// <summary>vehicle_params.json 모델(기반 기능 튠 값). 스키마: contracts/vehicle-params-schema.json</summary>
    [Serializable]
    public sealed class VehicleParams
    {
        public string version = "0.1";
        public float maxSpeed = 14f;
        public float reverseMaxSpeed = 5f;
        public float acceleration = 18f;
        public float brakeForce = 30f;
        public float steerTorque = 6f;
        public float steerSpeedFalloff = 0.6f;
        public float lateralGrip = 0.85f;
        public float suspensionLength = 0.6f;
        public float suspensionSpring = 60f;
        public float suspensionDamper = 6f;
        public float enterRange = 3f;
        public float exitMaxSpeed = 1.5f;
        public float flipUpThreshold = 0.2f;
        public float flipRecoverSeconds = 3f;
        public float resetY = -5f;
        public float movingSpeedThreshold = 0.3f;

        public static VehicleParams Default => new VehicleParams();
    }
}
