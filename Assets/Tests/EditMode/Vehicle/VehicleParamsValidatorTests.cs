using NUnit.Framework;
using Project1028.Vehicle;

namespace Project1028.Vehicle.Tests
{
    public class VehicleParamsValidatorTests
    {
        [Test] public void Default_Ok() => Assert.IsFalse(VehicleParamsValidator.Validate(VehicleParams.Default).IsError);
        [Test] public void MaxSpeed0_Error() { var p = VehicleParams.Default; p.maxSpeed = 0f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void Falloff15_Error() { var p = VehicleParams.Default; p.steerSpeedFalloff = 1.5f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void GripNegative_Error() { var p = VehicleParams.Default; p.lateralGrip = -0.1f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void Suspension0_Error() { var p = VehicleParams.Default; p.suspensionLength = 0f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void ExitNegative_Error() { var p = VehicleParams.Default; p.exitMaxSpeed = -1f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void FlipThreshold2_Error() { var p = VehicleParams.Default; p.flipUpThreshold = 2f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
        [Test] public void FlipSecondsNegative_Error() { var p = VehicleParams.Default; p.flipRecoverSeconds = -1f; Assert.IsTrue(VehicleParamsValidator.Validate(p).IsError); }
    }
}
