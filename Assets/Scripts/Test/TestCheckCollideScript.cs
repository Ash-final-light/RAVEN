using Sirenix.OdinInspector;
using Tools.Action.Data;
using Tools.Action.Running;
using Tools.InputHandle;
using UnityEngine;

namespace Test
{
    [RequireComponent(typeof(TestStateController))]
    public class TestCheckCollideScript : MonoBehaviour
    {
        [LabelText("动作播放器")] public ActionPlayer actionPlayer;
        [LabelText("自身碰撞检测参数")]
        public ActionCollideDetectionClipData collideDetectionSelf;
        
        [Button("绑定自身碰撞检测事件")]
        public void BindSelfCollideDetection()
        {
            actionPlayer = new ActionPlayer(gameObject.GetComponent<TestStateController>(),collideDetectionSelf);
            actionPlayer.RegisterCollideEvent(collideDetectionSelf.sliceId, (collider) =>
            {
                Debug.Log($"自身碰撞检测: {collider.name}");
            });
        }
        [Button("检测自身碰撞")]
        public void CheckSelfCollide()
        {
            actionPlayer.CheckCollide();
        }
        
        [LabelText("方形碰撞盒参数")]
        public ActionCollideDetectionClipData collideDetectionBox;
        [Button("绑定方形碰撞盒")]
        public void BindBoxCollide()
        {
            actionPlayer = new ActionPlayer(gameObject.GetComponent<TestStateController>(),collideDetectionBox);
            actionPlayer.RegisterCollideEvent(collideDetectionBox.sliceId, (collider) =>
            {
                Debug.Log($"方形碰撞盒: {collider.name}");
            });
        }
        [Button("检测方形碰撞盒")]
        public void CheckBoxCollide()
        {
            actionPlayer.CheckCollide();
        }
        
        [LabelText("扇形碰撞盒参数")]
        public ActionCollideDetectionClipData collideDetectionSector;
        [Button("绑定扇形碰撞盒")]
        public void BindSectorCollide()
        {
            actionPlayer = new ActionPlayer(gameObject.GetComponent<TestStateController>(),collideDetectionSector);
            actionPlayer.RegisterCollideEvent(collideDetectionSector.sliceId, (collider) =>
            {
                Debug.Log($"扇形碰撞盒: {collider.name}");
            });
        }
        [Button("检测扇形碰撞盒")]
        public void CheckSectorCollide()
        {
            actionPlayer.CheckCollide();
        }
    }
}