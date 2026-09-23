using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>플레이어가 근접해 실행할 수 있는 대상. 후속 기능(조사·탑승·끊기)이 구현을 추가한다.</summary>
    public interface IInteractable
    {
        string PromptText { get; }
        float InteractionRange { get; }
        Vector3 WorldPosition { get; }
        bool CanInteract(PlayerEntity player);
        void Interact(PlayerEntity player);
    }
}
