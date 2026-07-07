using System.Collections.Generic;
using Tools.Action.Data.Bullet;
using Tools.Action.Running;
using Tools.Battle;
using Tools.Buff;
using Tools.InputHandle;
using UnityEngine;

namespace Tools.Action.Data.Event
{
    public readonly struct ActionEventContext
    {
        public readonly ActionStateController Controller;
        public readonly ActionPlayer Player;
        public readonly ActionClip Clip;
        public readonly int Tick;

        public ActionEventContext(
            ActionStateController controller,
            ActionPlayer player,
            ActionClip clip,
            int tick)
        {
            Controller = controller;
            Player = player;
            Clip = clip;
            Tick = tick;
        }
    }

    public interface IActionEventHandler
    {
        ActionEventEnum EventType { get; }
        void Handle(ActionEventContext context, ActionEventClipData data);
    }

    public class ActionEventDispatcher
    {
        private readonly Dictionary<ActionEventEnum, IActionEventHandler> handlers = new();

        public ActionEventDispatcher(BuffManager buffManager)
        {
            Register(new DebugEventHandler());
            Register(new AddBuffEventHandler(buffManager));
            Register(new BlackboardEventHandler());
            Register(new FireBulletEventHandler());
            Register(new SlideJudgeEventHandler());
            Register(new CharacterPropertyFlagEventHandler());
        }

        public void Register(IActionEventHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            handlers[handler.EventType] = handler;
        }

        public void Dispatch(ActionEventContext context, ActionEventClipData data)
        {
            if (data == null)
            {
                Debug.LogError("[ActionEventDispatcher] Event data is null.");
                return;
            }

            if (!handlers.TryGetValue(data.type, out var handler))
            {
                Debug.LogWarning($"[ActionEventDispatcher] Missing handler. type={data.type}, event={data.name}");
                return;
            }

            handler.Handle(context, data);
        }
    }

    public class DebugEventHandler : IActionEventHandler
    {
        public ActionEventEnum EventType => ActionEventEnum.Debug;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            Debug.Log($"<color=green>[ActionDebug] {context.Clip?.name} {data.name} {data.message}</color>");
        }
    }

    public class AddBuffEventHandler : IActionEventHandler
    {
        private readonly BuffManager buffManager;

        public AddBuffEventHandler(BuffManager buffManager)
        {
            this.buffManager = buffManager;
        }

        public ActionEventEnum EventType => ActionEventEnum.AddBuff;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            if (buffManager == null)
            {
                Debug.LogError($"[AddBuffEventHandler] BuffManager is null. event={data.name}");
                return;
            }

            if (context.Controller == null)
            {
                Debug.LogError($"[AddBuffEventHandler] Controller is null. event={data.name}");
                return;
            }

            buffManager.CreateBuff(
                context.Controller.characterId,
                context.Controller.characterId,
                data.buffId,
                data.buffStack);
        }
    }

    public class BlackboardEventHandler : IActionEventHandler
    {
        public ActionEventEnum EventType => ActionEventEnum.Blackboard;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            if (context.Controller == null || context.Controller.RunningBlackboard == null)
            {
                Debug.LogError($"[BlackboardEventHandler] Blackboard is null. event={data.name}");
                return;
            }

            BlackboardEvent blackboardEvent = data.blackboardEvent;
            if (blackboardEvent == null)
            {
                Debug.LogError($"[BlackboardEventHandler] Blackboard event is null. event={data.name}");
                return;
            }

            var blackboard = context.Controller.RunningBlackboard;
            switch (blackboardEvent.type)
            {
                case BlackboardValueType.Bool:
                    blackboard.SetBoolParameter(blackboardEvent.key, blackboardEvent.boolValue);
                    break;
                case BlackboardValueType.Int:
                {
                    int value = data.blackboardOp == BlackboardOp.Add
                        ? blackboard.GetIntParameter(blackboardEvent.key) + blackboardEvent.intValue
                        : blackboardEvent.intValue;
                    blackboard.SetIntParameter(blackboardEvent.key, value);
                    break;
                }
                case BlackboardValueType.Float:
                {
                    float value = data.blackboardOp == BlackboardOp.Add
                        ? blackboard.GetFloatParameter(blackboardEvent.key) + blackboardEvent.floatValue
                        : blackboardEvent.floatValue;
                    blackboard.SetFloatParameter(blackboardEvent.key, value);
                    break;
                }
                case BlackboardValueType.String:
                    blackboard.SetStringParameter(blackboardEvent.key, blackboardEvent.stringValue);
                    break;
                default:
                    Debug.LogWarning($"[BlackboardEventHandler] Unsupported blackboard value type. type={blackboardEvent.type}, event={data.name}");
                    break;
            }
        }
    }

    public class FireBulletEventHandler : IActionEventHandler
    {
        public ActionEventEnum EventType => ActionEventEnum.FireBullet;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            if (context.Controller == null)
            {
                Debug.LogError($"[FireBulletEventHandler] Controller is null. event={data.name}");
                return;
            }

            if (data.bulletEvent == null)
            {
                Debug.LogError($"[FireBulletEventHandler] Bullet event is null. event={data.name}");
                return;
            }

            BulletLauncher launcher = data.bulletEvent.CreateLauncher(context.Controller);
            if (launcher?.model == null)
            {
                Debug.LogError($"[FireBulletEventHandler] Bullet launcher or model is null. event={data.name}");
                return;
            }

            if (string.IsNullOrEmpty(launcher.model.abName) || string.IsNullOrEmpty(launcher.model.assetName))
            {
                Debug.LogError($"[FireBulletEventHandler] Bullet prefab address is invalid. event={data.name}");
                return;
            }

            BindDefaultBulletDamage(context.Controller, launcher);

            if (!string.IsNullOrEmpty(data.name) && data.name.Contains("BulletLogTest"))
            {
                Debug.Log($"[BulletFire] event={data.name}, bullet={launcher.model.assetName}");
                launcher.model.onHit += collider =>
                {
                    Debug.Log($"[BulletHit] event={data.name}, target={(collider ? collider.name : "null")}");
                };
            }

            context.Controller.BulletAbility.SetBulletModel(
                launcher.model.abName,
                launcher.model.assetName);

            context.Controller.BulletAbility.Fire(launcher);
        }

        private void BindDefaultBulletDamage(ActionStateController caster, BulletLauncher launcher)
        {
            if (caster == null || launcher?.model == null)
            {
                return;
            }

            launcher.model.onHit += collider =>
            {
                if (collider == null)
                {
                    return;
                }

                if (launcher.model.damageHitConfig == null)
                {
                    Debug.LogWarning($"[BulletDamage] Missing damage config. bullet={launcher.model.assetName}");
                    return;
                }

                ActionStateController target = collider.GetComponentInParent<ActionStateController>();
                if (target == null)
                {
                    Debug.Log($"[BulletDamage] Ignored non-character target. collider={collider.name}");
                    return;
                }

                if (target == caster)
                {
                    return;
                }

                if (!CanHitByCamp(caster, target, launcher.model))
                {
                    Debug.Log($"[BulletDamage] Ignored by camp. caster={caster.characterId}, target={target.characterId}");
                    return;
                }

                BattleManager.Instance.AddDamage(
                    caster.characterId,
                    target.characterId,
                    launcher.model.damageHitConfig);

                Debug.Log(
                    $"[BulletDamage] AddDamage. caster={caster.characterId}, " +
                    $"target={target.characterId}, bullet={launcher.model.assetName}");
            };
        }

        private bool CanHitByCamp(ActionStateController caster, ActionStateController target, BulletModel model)
        {
            if (!BattleManager.Instance.Data.TryGetValue(caster.characterId, out var casterProperty))
            {
                Debug.LogWarning($"[BulletDamage] Caster is not in battle data. caster={caster.characterId}");
                return false;
            }

            if (!BattleManager.Instance.Data.TryGetValue(target.characterId, out var targetProperty))
            {
                Debug.LogWarning($"[BulletDamage] Target is not in battle data. target={target.characterId}");
                return false;
            }

            bool sameCamp = casterProperty.isPlayer == targetProperty.isPlayer;
            return sameCamp ? model.hitAlly : model.hitFoe;
        }
    }

    public class SlideJudgeEventHandler : IActionEventHandler
    {
        public ActionEventEnum EventType => ActionEventEnum.SlideJudge;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            if (context.Controller == null)
            {
                Debug.LogError($"[SlideJudgeEventHandler] Controller is null. event={data.name}");
                return;
            }

            if (data.slideJudgeEvent == null)
            {
                Debug.LogError($"[SlideJudgeEventHandler] Slide judge event is null. event={data.name}");
                return;
            }

            switch (data.slideJudgeEvent.op)
            {
                case SlideJudgeOp.Open:
                    context.Controller.OpenSlideJudge();
                    break;
                case SlideJudgeOp.Close:
                    context.Controller.CloseSlideJudge();
                    break;
                default:
                    Debug.LogWarning($"[SlideJudgeEventHandler] Unsupported op. op={data.slideJudgeEvent.op}, event={data.name}");
                    break;
            }
        }
    }

    public class CharacterPropertyFlagEventHandler : IActionEventHandler
    {
        public ActionEventEnum EventType => ActionEventEnum.CharacterPropertyFlag;

        public void Handle(ActionEventContext context, ActionEventClipData data)
        {
            if (context.Controller == null)
            {
                Debug.LogError($"[CharacterPropertyFlagEventHandler] Controller is null. event={data.name}");
                return;
            }

            if (data.characterPropertyFlagEvent == null)
            {
                Debug.LogError($"[CharacterPropertyFlagEventHandler] Flag event is null. event={data.name}");
                return;
            }

            context.Controller.SetTemporaryCharacterPropertyFlag(
                data.characterPropertyFlagEvent.flagType,
                data.characterPropertyFlagEvent.value);
        }
    }
}
