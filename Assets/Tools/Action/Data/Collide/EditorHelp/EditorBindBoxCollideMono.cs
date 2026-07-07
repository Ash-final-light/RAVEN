using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Action.Data.Collide.EditorHelp
{
    public class EditorBindBoxCollideMono:BindBoxCollideMono
    {
        /// <summary>
        /// 父类的actionClip，后续数值修改到这里
        /// </summary>
        [LabelText("父类SO"),SerializeField]private ActionClip actionClip;   
        public ActionCollideDetectionClipData detectionClipData;
        public Transform parent;

        [Button("应用到SO")]
        public void ApplyToActionClip()
        {
            actionClip.collideDetection.ApplyCollideDetection(detectionClipData.channelId, detectionClipData);
        }
        [Button("修改检测数据")]
        public void ModifyDetectionData()
        {
            switch (detectionClipData.type)
            {
                case ActionCollideDetectionType.Bind:
                    SetParam(parent, detectionClipData.data);
                    break;
                default:
                    SetParam(detectionClipData.data);
                    break;
            }
        }
        [Button("从SO移除")]
        public void RemoveFromActionClip()
        {
            actionClip.collideDetection.RemoveCollideDetection(detectionClipData.channelId);
        }
        [Button("检测")]
        public void DetectTest(System.Action<Collider> callback)
        {
            Detect(callback,detectionClipData.colliderLayerMask);
        }
    }
}