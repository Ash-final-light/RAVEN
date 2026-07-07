using Tools.Quest.Progression;
using UnityEngine;
using VContainer;

namespace Tools.Quest
{
    public class GiveExpQuestRewards:BaseQuestRewards
    {
        [Inject] private IPlayerProgressionService playerProgressionService;
        [SerializeField] private int experience = 2;

        public override void GiveReward()
        {
            Debug.Log($"GiveExpQuestRewards: {experience}");
            playerProgressionService.AddExperience(experience);
        }
    }
}
