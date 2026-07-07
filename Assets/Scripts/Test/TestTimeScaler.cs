using Sirenix.OdinInspector;
using TimeScale;
using TimeScale.Command;
using Tools.TimeScale.Command;
using Tools.TimeScale.Receiver;
using UnityEngine;

namespace Test
{
    public class TestTimeScaler:MonoBehaviour
    {
        [Button("测试全局减速0.2")]
        private void ApplyScale(TimeScaleType type = TimeScaleType.Effect, float scale = 0.2f)
        {
            var cmd = new TimeScaleGlobalCommand("Test_Slow", 5.0f, scale, type);
            TimeScaleManager.Instance.AddCommand(cmd);
            Debug.Log($"已发送：全局{type}减速 {scale}");
        }
    }
}