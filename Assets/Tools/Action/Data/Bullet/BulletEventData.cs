using System;
using Sirenix.OdinInspector;
using Tools.Action.Data.Collide;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data.Bullet
{
    [Serializable]
    public class BulletEventData
    {
        [LabelText("子弹模型")]
        public BulletModel model = new();

        [LabelText("发射挂点路径")]
        public string fireSocketPath;

        [LabelText("本地偏移")]
        public Vector3 localOffset = Vector3.zero;

        [LabelText("发射角度偏移")]
        public float fireDegreeOffset;

        [LabelText("持续时间")]
        public float duration = 1f;

        [LabelText("发射后可命中延迟")]
        public float canHitAfterCreated;

        [LabelText("始终使用发射角")]
        public bool useFireDegreeForever = true;

        public BulletLauncher CreateLauncher(ActionStateController controller)
        {
            Transform socket = ResolveSocket(controller);
            return new BulletLauncher
            {
                model = model?.Clone(),
                caster = controller != null ? controller.gameObject : null,
                firePosition = socket != null ? socket.TransformPoint(localOffset) : localOffset,
                fireDegree = (socket != null ? socket.eulerAngles.y : 0f) + fireDegreeOffset,
                duration = duration,
                useFireDegreeForever = useFireDegreeForever,
                canHitAfterCreated = canHitAfterCreated,
            };
        }

        private Transform ResolveSocket(ActionStateController controller)
        {
            if (controller == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(fireSocketPath))
            {
                return controller.transform;
            }

            Transform socket = controller.transform.Find(fireSocketPath);
            if (socket == null)
            {
                Debug.LogWarning($"[BulletEventData] Fire socket not found. path={fireSocketPath}, owner={controller.name}");
                return controller.transform;
            }

            return socket;
        }
    }
}
