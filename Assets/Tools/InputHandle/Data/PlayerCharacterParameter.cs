using System;
using Sirenix.OdinInspector;
using Tools.Camera;
using Tools.Skill;
using UnityEngine;

namespace Tools.Battle.Data
{
    [Serializable]
    public class PlayerCharacterParameters
    {
        
        #region 渲染帧更新数据

        [Title("玩家移动输入")] public Vector2 playerInputRawValueInFrame; // 该帧中玩家移动输入的原始向量，二维向量
        public Vector3 playerInputMovementInFrame; // 该帧中玩家移动输入在相机坐标系的向量，三维向量
        public Vector3 playerInputCharacterMovementInFrame; // 该帧中玩家移动输入在角色坐标系的向量

        [Title("玩家按键输入")] 
        public bool allowInputReaction = true;
        
        public bool isSlideInFrame;// 闪避
        public bool isAttackInFrame;// 普通攻击
        public bool isInteractInFrame;// 交互

        public bool isUltimateInFrame;
        public bool isSkillBallInFrame;
        public int skillBallInputIndex = -1;
        public SkillBallColor consumedSignalColor;
        public string consumedSignalInputName;
        public int consumedSignalCount;
        public string consumedSkillActionName;
        public int[] consumedSignalRuntimeIds;

        [Title("玩家相机锁定数据")] public CameraLockData cameraLockData;

        [Title("玩家闪避攻击计时")] public float evadeAttackCountdown;

        #endregion

        //TODO：逻辑帧更新数据，待补充，暂时不需要
        
    }
}
