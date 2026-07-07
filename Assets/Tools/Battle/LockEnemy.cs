using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Battle.Data;
using UnityEngine;

namespace Tools.Battle
{
    public class LockEnemy
    {
        private BattleManager battleManager => BattleManager.Instance;

        [ShowInInspector] private readonly List<(CharacterProperty character, int totalScore)> _visibleEnemyRank = new();
        private readonly List<CharacterProperty> _visibleEnemies = new();
        private readonly List<(int index, float angle)> _visibleEnemyAngles = new();
        private readonly List<(int index, float distance)> _visibleEnemyDistances = new();

        public IReadOnlyList<(CharacterProperty character, int totalScore)> VisibleEnemyRank => _visibleEnemyRank;

        private CharacterProperty _lockTarget;
        private CharacterProperty _playerTarget;

        public CharacterProperty CurrentLockTarget => _lockTarget;

        public void SetPlayerTarget(CharacterProperty target)
        {
            _playerTarget = target;
        }

        public Transform GetLockTarget()
        {
            return GetLockTargetProperty()?.targetTransform;
        }

        public CharacterProperty GetLockTargetProperty()
        {
            RefreshVisibleEnemyRank();
            _lockTarget = _visibleEnemyRank.Count > 0 ? _visibleEnemyRank[0].character : null;
            return _lockTarget;
        }

        private void RefreshVisibleEnemyRank()
        {
            _visibleEnemyRank.Clear();
            _visibleEnemies.Clear();

            if (battleManager == null)
            {
                return;
            }

            EnsurePlayerTarget();
            if (_playerTarget?.targetTransform == null)
            {
                return;
            }

            foreach (var character in battleManager.Data.Values)
            {
                if (IsValidEnemy(character))
                {
                    _visibleEnemies.Add(character);
                }
            }

            if (_visibleEnemies.Count == 0)
            {
                return;
            }

            var enemyAngleScore = CalculateEnemyAngleScores(_visibleEnemies);
            var enemyDistanceScore = CalculateEnemyDistanceScores(_playerTarget, _visibleEnemies);

            for (var i = 0; i < _visibleEnemies.Count; i++)
            {
                _visibleEnemyRank.Add((_visibleEnemies[i], enemyAngleScore[i] + enemyDistanceScore[i]));
            }

            _visibleEnemyRank.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));
        }

        private bool IsValidEnemy(CharacterProperty character)
        {
            return character != null &&
                   !character.isPlayer &&
                   !character.dead &&
                   character.isActive &&
                   character.targetTransform != null;
        }

        private void EnsurePlayerTarget()
        {
            if (_playerTarget?.targetTransform != null || battleManager == null)
            {
                return;
            }

            foreach (var character in battleManager.Data.Values)
            {
                if (character != null && character.isPlayer && character.targetTransform != null)
                {
                    _playerTarget = character;
                    return;
                }
            }
        }

        private int[] CalculateEnemyAngleScores(IReadOnlyList<CharacterProperty> visibleEnemies)
        {
            _visibleEnemyAngles.Clear();
            for (var i = 0; i < visibleEnemies.Count; i++)
            {
                var enemy = visibleEnemies[i];
                _visibleEnemyAngles.Add((
                    i,
                    Vector3.Angle(
                        _playerTarget.targetTransform.forward,
                        new Vector3(
                            enemy.targetTransform.position.x - _playerTarget.targetTransform.position.x,
                            0,
                            enemy.targetTransform.position.z - _playerTarget.targetTransform.position.z)
                    )));
            }

            _visibleEnemyAngles.Sort((a, b) => a.angle.CompareTo(b.angle));
            var visibleEnemyAngleScore = new int[visibleEnemies.Count];
            for (var i = 0; i < _visibleEnemyAngles.Count; i++)
            {
                var visibleEnemyAngle = _visibleEnemyAngles[i];
                switch (i)
                {
                    case 0:
                        visibleEnemyAngleScore[visibleEnemyAngle.index] = 3;
                        break;
                    case 1:
                        visibleEnemyAngleScore[visibleEnemyAngle.index] = 2;
                        break;
                    default:
                        visibleEnemyAngleScore[visibleEnemyAngle.index] = 1;
                        break;
                }
            }

            return visibleEnemyAngleScore;
        }

        private int[] CalculateEnemyDistanceScores(CharacterProperty player, IReadOnlyList<CharacterProperty> visibleEnemies)
        {
            _visibleEnemyDistances.Clear();
            for (var i = 0; i < visibleEnemies.Count; i++)
            {
                var enemy = visibleEnemies[i];
                _visibleEnemyDistances.Add((
                    i,
                    Vector3.Distance(player.targetTransform.position, enemy.targetTransform.position)
                ));
            }

            _visibleEnemyDistances.Sort((a, b) => a.distance.CompareTo(b.distance));
            var visibleEnemyDistanceScore = new int[visibleEnemies.Count];
            for (var i = 0; i < _visibleEnemyDistances.Count; i++)
            {
                var visibleEnemyDistance = _visibleEnemyDistances[i];
                switch (i)
                {
                    case 0:
                        visibleEnemyDistanceScore[visibleEnemyDistance.index] = 3;
                        break;
                    case 1:
                        visibleEnemyDistanceScore[visibleEnemyDistance.index] = 2;
                        break;
                    default:
                        visibleEnemyDistanceScore[visibleEnemyDistance.index] = 1;
                        break;
                }
            }

            return visibleEnemyDistanceScore;
        }
    }
}
