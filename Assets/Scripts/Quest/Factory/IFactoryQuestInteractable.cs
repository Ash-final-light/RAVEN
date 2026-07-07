namespace Quest.Factory
{
    public interface IFactoryQuestInteractable
    {
        bool CanInteract { get; }
        string InteractionPrompt { get; }
        void Interact(FactoryQuestInteractionActor actor);
    }
}
