using Sirenix.OdinInspector;
using Tools.Action.Running;
using Tools.InputHandle;
using UnityEngine;
using Tools.Action.Data;
namespace Test
{
    [RequireComponent(typeof(TestStateController))]
    public class TestCheckAnimScript : MonoBehaviour
    {
        [LabelText("动作播放器")] public ActionPlayer actionPlayer;
        
        [LabelText("attack01数据")] public ActionAnimation actionAnimation01 = new(); 
        [Button("attack01动画")]
        public void PlayAnimation()
        {
            actionPlayer = new ActionPlayer(gameObject.GetComponent<TestStateController>(),actionAnimation01);
            actionPlayer.CheckAnimation();
        }
        [LabelText("attack02数据")] public ActionAnimation actionAnimation02 = new(); 
        [Button("attack02动画")]
        public void PlayAnimation2()
        {
            actionPlayer = new ActionPlayer(gameObject.GetComponent<TestStateController>(),actionAnimation02);
            actionPlayer.CheckAnimation();
        }
        [Button("调整运行时动画速度")]
        public void ChangeSpeed(float speed)
        {
            gameObject.GetComponent<TestStateController>().AnimationAbility.SetAnimancerSpeed(speed);
        }
    }
}