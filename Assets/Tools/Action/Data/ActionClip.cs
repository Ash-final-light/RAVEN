using System;
using System.Collections.Generic;
using Animancer;
using Animancer.TransitionLibraries;
using Sirenix.OdinInspector;
using Tools.Action.Data.Bullet;
using Tools.Action.Data.Event;
using Tools.Audio;
using Tools.Damage.SO;
using Tools.InputHandle.Data;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Tools.Action.Data
{
    /// <summary>
    /// 项目设计为仅支持一段动画片段，所有animation部分可能做出修改
    /// </summary>
    [CreateAssetMenu(menuName = "Action/Action Clip")]
    public class ActionClip : SerializedScriptableObject
    {
        [  LabelText("名称")] public new string name; // 片段名称
        [  LabelText("帧率")] public int frameRate = 30; // 片段帧率
        [  LabelText("时长")] public float duration = 1f; // 片段时长，影响片段总帧数
        [  LabelText("总帧数")] public int totalTicks = 60; // 片段总帧数
        [  LabelText("动作状态")] public CharacterStateType StateType; // 片段状态
        [  LabelText("流程")] public ActionProcess process; // 片段流程
        [  LabelText("动画")] public ActionAnimation animation; // 片段动画
        [  LabelText("音频")] public ActionAudio audio; // 片段音频
        [  LabelText("特效")] public ActionEffect effect; // 片段特效
        [  LabelText("碰撞检测")] public ActionCollideDetection collideDetection; // 片段碰撞检测
        [  LabelText("事件")] public ActionEvent events; // 片段事件
#if UNITY_EDITOR
        [  LabelText("通道列表")] public List<ActionChannelData> channels = new(); // 片段通道列表，仅在编辑器下使用，用于可视化动作片段的通道
#endif
    }

    [Serializable]
    public class ActionProcess
    {
        [  LabelText("前摇时间点")] public float anticipationTime; // 前摇时间点
        [  LabelText("判定时间点")] public float judgmentTime; // 判定时间点
        [  LabelText("后摇时间点")] public float recoveryTime; // 后摇时间点
        [  LabelText("前摇帧")] public int anticipationTick; // 前摇帧
        [  LabelText("判定帧")] public int judgmentTick; // 判定帧
        [  LabelText("后摇帧")] public int recoveryTick; // 后摇帧
    }

    [Serializable]
    public class ActionAnimation
    {
        [ LabelText("动画库")] public TransitionLibraryAsset transitionLibrary; // 动画库
        [ LabelText("动画片段")] public ActionAnimationClipData animationClip = new(); // 动画片段
    }

    [Serializable]
    public class ActionAnimationClipData
    {
        [ LabelText("片段")] public TransitionAsset transition; // 动画片段
        [ LabelText("片段开始时间点")] public float startTime; // 动画片段开始时间点，影响开始帧
        [ LabelText("片段时长")] public float duration; // 动画片段时长，影响持续帧数
        [ LabelText("片段开始帧")] public int startTick; // 动画片段开始帧
        [ LabelText("片段持续帧数")] public int durationTicks; // 动画片段持续帧数
        [ LabelText("片段速度")] public float speed = 1f; // 动画片段速度
#if UNITY_EDITOR
        public string channelId;
#endif
    }

    [Serializable]
    public class ActionAudio
    {
        [FormerlySerializedAs("newAudioClips")] [  LabelText("音频片段列表")]
        public List<ActionAudioClipData> audioClips = new(); // 音频片段列表
    }

    public enum ActionAudioType
    {
        Specified,
        Random,
    }

    [Serializable]
    public class ActionAudioClipData
    {
        [  LabelText("片段类型")] public ActionAudioType type; // 音频片段类型

        [  LabelText("片段音频")] [ShowIf("type", ActionAudioType.Specified)]
        public AudioClip specifiedAudioClip; // 音频片段特定音频

        [  LabelText("片段音频")] [ShowIf("type", ActionAudioType.Random)]
        public AudioClipRandomizer randomAudioClip; // 音频片段随机音频

        [  LabelText("片段开始时间点")] public float startTime; // 音频片段开始时间点，影响开始帧
        [  LabelText("片段时长")] public float duration; // 音频片段时长，影响持续帧数
        [  LabelText("片段开始帧")] public int startTick; // 音频片段开始帧
        [  LabelText("片段持续帧数")] public int durationTicks; // 音频片段持续帧数
        [  LabelText("片段音量")] public float volume = 1f; // 音频片段音量

        public string Name => type switch
        {
            ActionAudioType.Specified => specifiedAudioClip != null ? specifiedAudioClip.name : "",
            ActionAudioType.Random => randomAudioClip != null ? randomAudioClip.name : "",
            _ => "",
        };

        public AudioClip AudioClip => type switch
        {
            ActionAudioType.Specified => specifiedAudioClip,
            ActionAudioType.Random => randomAudioClip?.Random(),
            _ => null,
        };

#if UNITY_EDITOR
        public string channelId;
#endif
    }

    [Serializable]
    public class ActionEffect
    {
        [  LabelText("特效片段列表")] public List<ActionEffectClipData> effectClips = new(); // 特效片段列表
    }

    public enum ActionEffectType
    {
        Dynamic,
        Fixed,
    }

    [Serializable]
    public class ActionEffectClipData
    {
        [ LabelText("片段预设体ab包名称")] public string abName;
        [ LabelText("片段预设体名称")] public string assetName;
        
        [ LabelText("片段开始时间点")] public float startTime; // 特效片段开始时间点，影响开始帧
        [ LabelText("片段时长")] public float duration; // 特效片段时长，影响持续帧数
        [ LabelText("片段开始帧")] public int startTick; // 特效片段开始帧
        [ LabelText("片段持续帧数")] public int durationTicks; // 特效片段持续帧数
        [ LabelText("片段类型")] public ActionEffectType type = ActionEffectType.Dynamic; // 特效片段类型
        [ LabelText("片段起始时间")] public float startLifetime = 0f; // 特效片段起始时间
        [ LabelText("片段模拟速度")] public float simulationSpeed = 1f; // 特效片段模拟速度
        [ LabelText("片段本地位置")] public Vector3 localPosition = Vector3.zero; // 特效片段本地位置

        [FormerlySerializedAs("newLocalRotation")] [ LabelText("片段本地旋转量")]
        public Quaternion localRotation = Quaternion.identity; // 特效片段本地旋转量

        [ LabelText("片段本地缩放")] public Vector3 localScale = Vector3.one; // 特效片段本地缩放
#if UNITY_EDITOR
        public string channelId;
#endif
    }

    [Serializable]
    public class ActionCollideDetection
    {
        [  LabelText("碰撞检测片段列表")]
        public List<ActionCollideDetectionClipData> collideDetectionClips = new(); // 碰撞检测片段列表
        //#if UNITY_EDITOR
        public void ApplyCollideDetection(string channelId,ActionCollideDetectionClipData detectionClipData)
        {
            foreach (var clip in collideDetectionClips)
            {
                if (clip.channelId == channelId)
                {
                    clip.ApplyToActionClip(detectionClipData);
                    return;
                }
            }
            collideDetectionClips.Add(detectionClipData);
        }
        //#endif
        public void RemoveCollideDetection(string channelId)
        {
            collideDetectionClips.RemoveAll(x => x.channelId == channelId);
        }
    }

    /// <summary>
    /// 这里把ReadOnly属性去掉了，后续看情况进行字段保护
    /// </summary>
    [Serializable]
    public class ActionCollideDetectionClipData
    {
        [ LabelText("片段类型")]
        public ActionCollideDetectionType type = ActionCollideDetectionType.None; // 碰撞检测片段类型
        [ LabelText("碰撞检测事件")]
        public ActionCollideEventType collideEventType = ActionCollideEventType.CauseDamage;
        [LabelText("碰撞数值配置")]
        public CollideHitConfigSO collideHitConfig;
        [LabelText("信号球伤害配置")]
        public List<SignalBallDamageConfig> signalBallDamageConfigs = new();
        
        [  LabelText("片段开始时间点")] public float startTime; // 碰撞检测片段开始时间点，影响开始帧
        [  LabelText("片段时长")] public float duration; // 碰撞检测片段时长，影响持续帧数
        [  LabelText("片段开始帧")] public int startTick; // 碰撞检测片段开始帧
        [  LabelText("片段持续帧数")] public int durationTicks; // 碰撞检测片段持续帧数
        [ReadOnly,  LabelText("片段检测层")] public int colliderLayerMask => colliderLayerName == "" ? 0 : (1 << LayerMask.NameToLayer(colliderLayerName));// 碰撞检测层，用于外部判定是否处于同一碰撞，过滤多种碰撞
        //TIPS: 填写Layer名称
        [  LabelText("片段层名称")]
        public string colliderLayerName; // 碰撞检测层名称，用于外部判定是否处于同一碰撞，过滤多种碰撞
        //TIPS： 唯一id标识符
        [  LabelText("片段事件名")] public string sliceId = Guid.NewGuid().ToString(); // 碰撞检测片段唯一id，用来找对应碰撞事件
        [Button("重置片段事件ID")]
        private void ResetSliceId()
        {
            sliceId = Guid.NewGuid().ToString();
        }
        [  LabelText("片段类型数据"), SerializeReference]
        public ActionCollideDetectionBaseData data = new ActionCollideDetectionEmptyData(); // 碰撞检测片段类型数据
//#if UNITY_EDITOR
        public string channelId;
        [Button("重置轨道ID")]
        private void ResetChannelId()
        {
            channelId = Guid.NewGuid().ToString();
        }
        public void ApplyToActionClip(ActionCollideDetectionClipData actionClip)
        {
            actionClip.type = type;
            actionClip.startTime = startTime;
            actionClip.duration = duration;
            actionClip.startTick = startTick;
            actionClip.durationTicks = durationTicks;
            actionClip.colliderLayerName = colliderLayerName;
            actionClip.sliceId = sliceId;
            actionClip.data = data.Clone(actionClip.type);
        }

        public DamageHitConfigSO GetSignalBallDamageConfig(int consumedSignalCount)
        {
            foreach (var config in signalBallDamageConfigs)
            {
                if (config.consumedSignalCount == consumedSignalCount)
                {
                    return config.damageConfig;
                }
            }

            return null;
        }
//#endif
    }

    [Serializable]
    public class SignalBallDamageConfig
    {
        [MinValue(1)] public int consumedSignalCount = 1;
        public DamageHitConfigSO damageConfig;
    }

    public enum ActionCollideEventType
    {
        CauseDamage,
        Custom,
    }

    public enum ActionCollideDetectionType
    {
        None,
        Bind,
        Box,
        Sphere,
        Sector,
    }

    [Serializable]
    public abstract class ActionCollideDetectionBaseData
    {

    }

    [Serializable]
    public class ActionCollideDetectionEmptyData : ActionCollideDetectionBaseData
    {
    }

    [Serializable]
    public class ActionCollideDetectionSelfData : ActionCollideDetectionBaseData
    {
    }

    [Serializable]
    public abstract class ActionCollideDetectionShapeData : ActionCollideDetectionBaseData
    {
#if UNITY_EDITOR
        [LabelText("是否在编辑器中展示")] public bool showInEditor = false;
#endif

        [LabelText("本地位置")] public Vector3 localPosition = Vector3.zero;
    }

    [Serializable]
    public class ActionCollideDetectionBoxData : ActionCollideDetectionShapeData
    {
        [LabelText("本地旋转量")] public Quaternion localRotation = Quaternion.identity;
        [LabelText("尺寸")] public Vector3 size = Vector3.one;
    }

    [Serializable]
    public class ActionCollideDetectionSphereData : ActionCollideDetectionShapeData
    {
        [LabelText("半径"), MinValue(0f)] public float radius = 1f;
    }

    [Serializable]
    public class ActionCollideDetectionSectorData : ActionCollideDetectionShapeData
    {
        [LabelText("本地旋转量")] public Quaternion localRotation = Quaternion.identity;
        [LabelText("内环半径"), MinValue(0f)] public float insideRadius = 0f;

        [FormerlySerializedAs("radius")] [LabelText("外圈半径"), MinValue(0f)]
        public float outsideRadius = 1f;

        [LabelText("高度")] public float height = 1f;
        [LabelText("扇形轴心转角")] public float anglePivot = 0f;
        [LabelText("扇形张角")] public float angle = 60f;
    }

    public enum ActionEventParameter
    {
        None,
        Bool,
        Int,
        Float,
        String,
        UnityObject,
    }

    [Serializable]
    public class ActionEvent
    {
        [  LabelText("事件片段列表")] public List<ActionEventClipData> eventClips = new(); // 事件片段列表
    }

    [Serializable]
    public class ActionEventClipData
    {
        [  LabelText("片段名称")] public string name; // 事件片段名称
        [  LabelText("片段触发时间点")] public float time; // 事件片段触发时间点
        [  LabelText("片段触发帧")] public int tick; // 事件片段触发帧
        
        public ActionEventEnum type; // 事件片段类型
        
        #region buff部分
        [LabelText("Buff数字")] [ShowIf("type", ActionEventEnum.AddBuff)]
        public int buffId; // 事件片段Buff数字
        [LabelText("Buff层数")] [ShowIf("type", ActionEventEnum.AddBuff)]
        public int buffStack; // 事件片段Buff层数
        #endregion
        
        #region 黑板部分
        [LabelText("黑板操作")] [ShowIf("type", ActionEventEnum.Blackboard)]
        public BlackboardOp blackboardOp; // 事件片段黑板操作
        
        [LabelText("黑板值")] [ShowIf("type", ActionEventEnum.Blackboard)]
        public BlackboardEvent blackboardEvent; // 事件片段黑板值
        #endregion

        #region Debug 部分
        [LabelText("Debug")] [ShowIf("type", ActionEventEnum.Debug)]
        public string message;
        #endregion

        #region Bullet 部分
        [LabelText("子弹事件")] [ShowIf("type", ActionEventEnum.FireBullet)]
        public BulletEventData bulletEvent;
        #endregion

        #region Slide Judge
        [LabelText("闪避判定事件")] [ShowIf("type", ActionEventEnum.SlideJudge)]
        public SlideJudgeEventData slideJudgeEvent;
        #endregion

        #region Character Property Flag
        [ShowIf("type", ActionEventEnum.CharacterPropertyFlag)]
        public CharacterPropertyFlagEventData characterPropertyFlagEvent;
        #endregion

        /*[FormerlySerializedAs("type")] [  LabelText("片段参数类型")]
        public ActionEventParameter parameter; // 事件片段触发类型

        [  LabelText("片段参数值")] [ShowIf("parameter", ActionEventParameter.Bool)]
        public bool boolPayload; // 事件片段参数值

        [  LabelText("片段参数值")] [ShowIf("parameter", ActionEventParameter.Int)]
        public int intPayload; // 事件片段参数值

        [  LabelText("片段参数值")] [ShowIf("parameter", ActionEventParameter.Float)]
        public float floatPayload; // 事件片段参数值

        [  LabelText("片段参数值")] [ShowIf("parameter", ActionEventParameter.String)]
        public string stringPayload; // 事件片段参数值

        [  LabelText("片段参数值")] [ShowIf("parameter", ActionEventParameter.UnityObject)]
        public Object objectPayload; // 事件片段参数值*/
#if UNITY_EDITOR
        public string channelId;
#endif
    }

    [Serializable]
    public class SlideJudgeEventData
    {
        public SlideJudgeOp op;
    }

    [Serializable]
    public class CharacterPropertyFlagEventData
    {
        public CharacterPropertyFlagType flagType;
        public bool value;
    }

    public class BlackboardEvent
    {
        [LabelText("黑板值类型")] public BlackboardValueType type; // 事件片段黑板值类型
        
        [LabelText("黑板值键")] [ShowIf("type",BlackboardValueType.Bool)]
        public string key; // 事件片段黑板值键
        
        [LabelText("黑板值")] [ShowIf("type",BlackboardValueType.Bool)]
        public bool boolValue;
        
        [LabelText("黑板值")] [ShowIf("type",BlackboardValueType.Float)]
        public float floatValue;
        
        [LabelText("黑板值")] [ShowIf("type",BlackboardValueType.Int)]
        public int intValue;
        
        [LabelText("黑板值")] [ShowIf("type",BlackboardValueType.String)]
        public string stringValue;
    }

#if UNITY_EDITOR
    [Serializable]
    public class ActionChannelData
    {
        [  LabelText("通道id")] public string id;
        [  LabelText("通道名称")] public string name;
    }
#endif

}
