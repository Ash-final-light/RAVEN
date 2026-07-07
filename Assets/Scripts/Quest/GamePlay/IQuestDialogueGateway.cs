using System;

namespace Quest.GamePlay
{
    public interface IQuestDialogueGateway
    {
        void Play(string dialogueId, Action onCompleted);
    }
}
