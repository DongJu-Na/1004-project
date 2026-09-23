using NUnit.Framework;
using Project1028.Vehicle;

namespace Project1028.Vehicle.Tests
{
    public class SeatAssignmentTests
    {
        [Test] public void Empty_Driver() => Assert.AreEqual(SeatKind.Driver, SeatAssignment.PickSeat(false, false));
        [Test] public void DriverTaken_Passenger() => Assert.AreEqual(SeatKind.Passenger, SeatAssignment.PickSeat(true, false));
        [Test] public void PassengerTaken_Driver() => Assert.AreEqual(SeatKind.Driver, SeatAssignment.PickSeat(false, true));
        [Test] public void Full_Null() => Assert.IsNull(SeatAssignment.PickSeat(true, true));
    }
}
