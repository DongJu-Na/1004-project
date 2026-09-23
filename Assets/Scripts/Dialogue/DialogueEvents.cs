using System;

namespace Project1028.PlayFoundation
{
    /// <summary>후속 기능이 구독하는 대화 이벤트 허브. 페이로드에 항상 플레이어 개체를 포함한다(헌장 원칙 IV).</summary>
    public static class DialogueEvents
    {
        public static event Action<NpcIdentity, PlayerEntity> OnDialogueStarted;
        public static event Action<NpcIdentity, PlayerEntity, int> OnLineAdvanced;
        public static event Action<NpcIdentity, PlayerEntity> OnDialogueEnded;
        public static event Action<PlayerEntity, Destination> OnDestinationReceived;

        internal static void RaiseStarted(NpcIdentity npc, PlayerEntity player) => OnDialogueStarted?.Invoke(npc, player);
        internal static void RaiseLineAdvanced(NpcIdentity npc, PlayerEntity player, int index) => OnLineAdvanced?.Invoke(npc, player, index);
        internal static void RaiseEnded(NpcIdentity npc, PlayerEntity player) => OnDialogueEnded?.Invoke(npc, player);
        internal static void RaiseDestinationReceived(PlayerEntity player, Destination destination) => OnDestinationReceived?.Invoke(player, destination);
    }
}
