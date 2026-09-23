using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>대화 종료 시 플레이어에게 전달되는 목적지 레코드.</summary>
    public sealed class Destination
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public string SourceNpcId { get; }
        public PlayerEntity Receiver { get; }
        public DestinationMarker Marker { get; internal set; }

        public Destination(string name, Vector3 position, string sourceNpcId, PlayerEntity receiver)
        {
            Name = name;
            Position = position;
            SourceNpcId = sourceNpcId;
            Receiver = receiver;
        }
    }
}
