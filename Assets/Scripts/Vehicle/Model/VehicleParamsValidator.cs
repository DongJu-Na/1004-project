using System.Collections.Generic;

namespace Project1028.Vehicle
{
    public sealed class VehicleValidationResult
    {
        public bool IsError;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
    }

    public static class VehicleParamsValidator
    {
        public static VehicleValidationResult Validate(VehicleParams p)
        {
            var r = new VehicleValidationResult();
            if (p == null) { r.Error("파라미터가 null입니다."); return r; }
            if (p.maxSpeed <= 0f) r.Error("maxSpeed > 0");
            if (p.reverseMaxSpeed <= 0f) r.Error("reverseMaxSpeed > 0");
            if (p.acceleration <= 0f) r.Error("acceleration > 0");
            if (p.brakeForce <= 0f) r.Error("brakeForce > 0");
            if (p.steerTorque <= 0f) r.Error("steerTorque > 0");
            if (p.steerSpeedFalloff < 0f || p.steerSpeedFalloff > 1f) r.Error("steerSpeedFalloff 0~1");
            if (p.lateralGrip < 0f || p.lateralGrip > 1f) r.Error("lateralGrip 0~1");
            if (p.suspensionLength <= 0f) r.Error("suspensionLength > 0");
            if (p.suspensionSpring <= 0f) r.Error("suspensionSpring > 0");
            if (p.suspensionDamper < 0f) r.Error("suspensionDamper ≥ 0");
            if (p.enterRange <= 0f) r.Error("enterRange > 0");
            if (p.exitMaxSpeed < 0f) r.Error("exitMaxSpeed ≥ 0");
            if (p.flipUpThreshold < -1f || p.flipUpThreshold > 1f) r.Error("flipUpThreshold -1~1");
            if (p.flipRecoverSeconds < 0f) r.Error("flipRecoverSeconds ≥ 0");
            if (p.movingSpeedThreshold < 0f) r.Error("movingSpeedThreshold ≥ 0");
            return r;
        }
    }
}
