using System.Collections.Generic;
using UnityEngine;

namespace Tools.Action.Data.Bullet
{
    [System.Serializable]
    public class BulletLauncher
    {
        public BulletModel model;
        public GameObject caster;
        public Vector3 firePosition;
        public float fireDegree;
        public float speed;
        public float duration;
        public BulletTargetingFunction targetFunction;
        public BulletTween tween;
        public bool useFireDegreeForever;
        public float canHitAfterCreated;
        public Dictionary<string, object> param;

    }
}