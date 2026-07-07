using System;
using System.Collections.Generic;
using DG.Tweening;
using Tools.Skill;
using UnityEngine;

namespace UI.UIAction.SkillBallUI
{
    public class ActionUISkillBallBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rightMostAnchor;
        [SerializeField] private RectTransform ballEntityRoot;
        [Tooltip("Use SignalBallSlot_0 as the entity prefab/template.")]
        [SerializeField] private ActionUISkillBallEntity entityPrefab;
        [SerializeField, Min(1)] private int maxVisibleCount = 8;

        [Header("Motion")]
        [SerializeField] private float slotSpacing = 52f;
        [SerializeField] private float pushFromLeftOffset = 72f;
        [SerializeField] private float pushDuration = 0.22f;
        [SerializeField] private float moveDuration = 0.18f;
        [SerializeField] private float consumeStagger = 0.035f;

        private BaseSkillDispatcher dispatcher;
        private bool isInMatrixMode;
        private readonly Dictionary<int, ActionUISkillBallEntity> entitiesByRuntimeId = new();
        private readonly HashSet<int> consumingRuntimeIds = new();

        public event Action<int> EntityClicked;
        public event Action<int> SkillBallSlotClicked;

        public void Bind(BaseSkillDispatcher newDispatcher)
        {
            Unbind();

            dispatcher = newDispatcher;
            if (dispatcher == null)
            {
                ClearAllEntities();
                return;
            }

            dispatcher.SkillBallsAdded += OnSkillBallsChanged;
            dispatcher.SkillBallsConsumed += OnSkillBallsConsumed;
            dispatcher.MatrixModeChanged += OnMatrixModeChanged;
        }

        public void Unbind()
        {
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.SkillBallsAdded -= OnSkillBallsChanged;
            dispatcher.SkillBallsConsumed -= OnSkillBallsConsumed;
            dispatcher.MatrixModeChanged -= OnMatrixModeChanged;
            dispatcher = null;
        }

        private void Awake()
        {
            if (ballEntityRoot == null)
            {
                ballEntityRoot = transform as RectTransform;
            }

            if (rightMostAnchor == null && entityPrefab != null)
            {
                rightMostAnchor = entityPrefab.RectTransform;
            }
        }

        private void OnDestroy()
        {
            Unbind();
            ClearAllEntities();
        }

        private void OnSkillBallsChanged(IReadOnlyList<SkillBallViewData> balls)
        {
            RemoveOrphanGeneratedChildren();

            HashSet<int> nextVisibleRuntimeIds = new();
            int visibleCount = Mathf.Min(balls.Count, maxVisibleCount);

            for (int i = 0; i < visibleCount; i++)
            {
                SkillBallViewData data = balls[i];
                nextVisibleRuntimeIds.Add(data.RuntimeId);

                ActionUISkillBallEntity entity = GetOrCreateEntity(data, i, out bool isNewEntity);
                if (entity == null)
                {
                    continue;
                }

                entity.SetData(data, i + 1);
                entity.SetAuraActive(isInMatrixMode);

                Vector2 slotPosition = GetSlotPosition(i);
                if (!Application.isPlaying)
                {
                    entity.SetImmediatePosition(slotPosition);
                }
                else if (isNewEntity)
                {
                    entity.PlayPushIn(slotPosition, pushDuration);
                }
                else
                {
                    entity.MoveTo(slotPosition, moveDuration);
                }
            }

            RemoveEntitiesNotIn(nextVisibleRuntimeIds);
        }

        private void OnSkillBallsConsumed(SkillBallConsumeViewData consumeData)
        {
            if (consumeData.RuntimeIds == null || consumeData.RuntimeIds.Length == 0)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                foreach (int runtimeId in consumeData.RuntimeIds)
                {
                    DestroyEntity(runtimeId);
                }

                return;
            }

            Sequence sequence = DOTween.Sequence().SetUpdate(true);

            for (int i = 0; i < consumeData.RuntimeIds.Length; i++)
            {
                int runtimeId = consumeData.RuntimeIds[i];
                if (!entitiesByRuntimeId.TryGetValue(runtimeId, out ActionUISkillBallEntity entity))
                {
                    continue;
                }

                consumingRuntimeIds.Add(runtimeId);
                int capturedRuntimeId = runtimeId;
                sequence.Insert(i * consumeStagger, entity.PlayConsumeTween());
                sequence.InsertCallback(i * consumeStagger + entity.ConsumeDuration, () => DestroyEntity(capturedRuntimeId));
            }
        }

        private void OnMatrixModeChanged(bool active)
        {
            isInMatrixMode = active;

            foreach (ActionUISkillBallEntity entity in entitiesByRuntimeId.Values)
            {
                if (entity != null)
                {
                    entity.SetAuraActive(active);
                }
            }
        }

        private ActionUISkillBallEntity GetOrCreateEntity(SkillBallViewData data, int visibleIndex, out bool isNewEntity)
        {
            isNewEntity = false;

            if (entitiesByRuntimeId.TryGetValue(data.RuntimeId, out ActionUISkillBallEntity existed))
            {
                return existed;
            }

            if (entityPrefab == null)
            {
                Debug.LogWarning("ActionUISkillBallBar has no entity prefab.", this);
                return null;
            }

            ActionUISkillBallEntity entity = Instantiate(entityPrefab, ballEntityRoot);
            entity.gameObject.SetActive(true);
            entity.name = $"SignalBallEntity_{data.RuntimeId}_{data.Color}";
            entity.Clicked += OnEntityClicked;

            Vector2 targetPosition = GetSlotPosition(visibleIndex);
            entity.RectTransform.anchoredPosition = GetPushStartPosition(targetPosition);
            entity.PrepareSpawn();

            isNewEntity = true;
            entitiesByRuntimeId.Add(data.RuntimeId, entity);
            return entity;
        }

        private void OnEntityClicked(int runtimeId)
        {
            EntityClicked?.Invoke(runtimeId);

            if (entitiesByRuntimeId.TryGetValue(runtimeId, out ActionUISkillBallEntity entity))
            {
                SkillBallSlotClicked?.Invoke(entity.VisibleIndex);
            }
        }

        private void RemoveEntitiesNotIn(HashSet<int> nextVisibleRuntimeIds)
        {
            List<int> removeList = new();

            foreach (int runtimeId in entitiesByRuntimeId.Keys)
            {
                if (!nextVisibleRuntimeIds.Contains(runtimeId))
                {
                    removeList.Add(runtimeId);
                }
            }

            foreach (int runtimeId in removeList)
            {
                if (!entitiesByRuntimeId.TryGetValue(runtimeId, out ActionUISkillBallEntity entity))
                {
                    continue;
                }

                if (consumingRuntimeIds.Contains(runtimeId))
                {
                    continue;
                }

                if (!Application.isPlaying)
                {
                    DestroyEntity(runtimeId);
                    continue;
                }

                int capturedRuntimeId = runtimeId;
                entity.PlayDisappearTween().OnComplete(() => DestroyEntity(capturedRuntimeId));
            }
        }

        private void DestroyEntity(int runtimeId)
        {
            if (!entitiesByRuntimeId.TryGetValue(runtimeId, out ActionUISkillBallEntity entity))
            {
                return;
            }

            entitiesByRuntimeId.Remove(runtimeId);
            consumingRuntimeIds.Remove(runtimeId);

            if (entity == null)
            {
                return;
            }

            entity.KillTweens();
            entity.Clicked -= OnEntityClicked;
            DestroyEntityObject(entity.gameObject);
        }

        private Vector2 GetSlotPosition(int index)
        {
            if (index < 0)
            {
                return Vector2.zero;
            }

            Vector2 rightMostPosition = rightMostAnchor != null
                ? rightMostAnchor.anchoredPosition
                : Vector2.zero;

            return rightMostPosition + Vector2.left * (slotSpacing * index);
        }

        private Vector2 GetPushStartPosition(Vector2 targetPosition)
        {
            return targetPosition + Vector2.left * pushFromLeftOffset;
        }

        private void ClearAllEntities()
        {
            foreach (ActionUISkillBallEntity entity in entitiesByRuntimeId.Values)
            {
                if (entity == null)
                {
                    continue;
                }

                entity.KillTweens();
                entity.Clicked -= OnEntityClicked;
                DestroyEntityObject(entity.gameObject);
            }

            entitiesByRuntimeId.Clear();
            consumingRuntimeIds.Clear();
            RemoveAllGeneratedChildren();
        }

        private void RemoveOrphanGeneratedChildren()
        {
            if (ballEntityRoot == null)
            {
                return;
            }

            for (int i = ballEntityRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = ballEntityRoot.GetChild(i);
                if (!child.name.StartsWith("SignalBallEntity_"))
                {
                    continue;
                }

                ActionUISkillBallEntity entity = child.GetComponent<ActionUISkillBallEntity>();
                if (entity != null && entitiesByRuntimeId.ContainsKey(entity.RuntimeId))
                {
                    continue;
                }

                DestroyEntityObject(child.gameObject);
            }
        }

        private void RemoveAllGeneratedChildren()
        {
            if (ballEntityRoot == null)
            {
                return;
            }

            for (int i = ballEntityRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = ballEntityRoot.GetChild(i);
                if (child.name.StartsWith("SignalBallEntity_"))
                {
                    DestroyEntityObject(child.gameObject);
                }
            }
        }

        private static void DestroyEntityObject(GameObject entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(entityObject);
                return;
            }

            DestroyImmediate(entityObject);
        }
    }
}
