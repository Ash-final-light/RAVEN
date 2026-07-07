namespace Tools.Action.Data.Interact
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        string InteractionPrompt { get; }
        void Interact();
    }
}
