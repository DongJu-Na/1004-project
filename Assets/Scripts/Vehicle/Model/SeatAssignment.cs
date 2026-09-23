namespace Project1028.Vehicle
{
    public enum SeatKind { Driver, Passenger }

    /// <summary>빈 좌석 배정: 운전석 우선. 둘 다 차면 null. 순수 로직.</summary>
    public static class SeatAssignment
    {
        public static SeatKind? PickSeat(bool driverOccupied, bool passengerOccupied)
        {
            if (!driverOccupied) return SeatKind.Driver;
            if (!passengerOccupied) return SeatKind.Passenger;
            return null;
        }
    }
}
