using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.Quest.Data
{
    [CreateAssetMenu()]
    public class QuestPool : ScriptableObject
    {
        public string test = "test";
        public List<QuestConfig> QuestConfigList = new List<QuestConfig>();
        public QuestConfig[] QuestConfigs => QuestConfigList.ToArray();
#if UNITY_EDITOR
        public QuestConfig AddQuestConfig()
        {
            var questConfig = CreateInstance<QuestConfig>();
            questConfig.id = System.Guid.NewGuid().ToString();
            questConfig.name = "QuestConfig";

            QuestConfigList.Add(questConfig);

            UnityEditor.AssetDatabase.AddObjectToAsset(questConfig, this); // ⭐ 关键
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            return questConfig;
        }
        public void RemoveQuestConfig(QuestConfig questConfig)
        {
            QuestConfigList.Remove(questConfig);

            UnityEditor.AssetDatabase.RemoveObjectFromAsset(questConfig); // ⭐ 关键
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        public QuestPool Clone()
        {
            var questPool = Instantiate(this);
            questPool.QuestConfigList = new List<QuestConfig>(QuestConfigList.Select(x => x.Clone()));
            return questPool;
        }
    }
}