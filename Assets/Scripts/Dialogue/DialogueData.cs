using System;

namespace Project1028.PlayFoundation
{
    /// <summary>contracts/dialogue-schema.json과 필드명이 일치하는 JsonUtility 모델.</summary>
    [Serializable]
    public sealed class DialogueData
    {
        public string npcId;
        public string[] lines;
        public DestinationData destination;
    }

    [Serializable]
    public sealed class DestinationData
    {
        public string name;
        public float x;
        public float y;
        public float z;
    }
}
