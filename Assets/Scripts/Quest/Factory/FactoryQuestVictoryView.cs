using Tools.RecycleViewFramework.View;
using UnityEngine;

namespace Quest.Factory
{
    public sealed class FactoryQuestVictoryView : View
    {
        public void CloseVictoryView() => Close();

        private void Update()
        {
            if (Visibility && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)))
                Close();
        }
    }
}
