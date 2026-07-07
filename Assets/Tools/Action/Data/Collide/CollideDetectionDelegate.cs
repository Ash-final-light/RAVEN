using Unity.VisualScripting;
using UnityEngine;

namespace Tools.Action.Data.Collide
{
    //不同碰撞绑定不同事件，一个碰撞体仅绑定一个事件，不支持解绑
    public class CollideDetectionDelegate
    {
        public ActionCollideDetectionBaseData baseData;
        public Transform parentTransform; // 父对象
        public Transform ownerTransform; // 检测中心
        public MonoBehaviour collideObj;
        public CollideAbility collideAbility;
        private int _layerMask;
     
        public void Init(CollideAbility collideAbility,Transform parent,Transform owner,ActionCollideDetectionBaseData baseData,int layerMask)
        {
            this.collideAbility = collideAbility;
            parentTransform = parent;
            ownerTransform = owner;
            this.baseData = baseData;
            _layerMask = layerMask;
        }

        public void BindCollide(System.Action<Collider> onCollideEvent)
        {
            switch (baseData)
            {
                case ActionCollideDetectionSelfData selfData:
                    var selfcollide = ownerTransform.AddComponent<BindSelfCollideMono>();
                    selfcollide.transform.parent = parentTransform;
                    selfcollide.SetParam(selfData);
                    selfcollide.Detect(onCollideEvent,_layerMask);
                    collideObj = selfcollide;
                    break;
                case ActionCollideDetectionBoxData boxData:
                    var boxcollide = collideAbility._boxColliderPool.Get((boxcollide)=>boxcollide.gameObject.SetActive(true));
                    boxcollide.transform.parent = parentTransform;
                    boxcollide.SetParam(ownerTransform,boxData);
                    collideObj = boxcollide;
                    break;
                case ActionCollideDetectionSphereData sphereData:
                    var spherecollide = collideAbility._sphereColliderPool.Get((spherecollide)=>spherecollide.gameObject.SetActive(true));
                    spherecollide.transform.parent = parentTransform;
                    spherecollide.SetParam(ownerTransform,sphereData);
                    collideObj = spherecollide;
                    break;
                case ActionCollideDetectionSectorData sectorData:
                    var sectorcollide = collideAbility._sectorColliderPool.Get((sectorcollide)=>sectorcollide.gameObject.SetActive(true));
                    sectorcollide.transform.parent = parentTransform;
                    sectorcollide.SetParam(ownerTransform,sectorData);
                    collideObj = sectorcollide;
                    break;
                default:
                    break;
            }
        }
        
        public void Tick(System.Action<Collider> onCollideEvent)
        {
            switch (collideObj)
            {
                case ICollideObj collideObj:
                    collideObj.Detect(onCollideEvent,_layerMask);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// TODO:碰撞体回归池子
        /// </summary>
        public void Destroy()
        {
            if(!collideObj)
                return;
            switch (collideObj)
            {
               case BindBoxCollideMono boxcollide:
                    collideAbility._boxColliderPool.Release(boxcollide,(boxcollide)=>boxcollide.ResetStatus());
                    break;
                case BindSphereCollideMono spherecollide:
                    collideAbility._sphereColliderPool.Release(spherecollide,(spherecollide)=>spherecollide.ResetStatus()); 
                    break;
                case BindSectorCollideMono sectorcollide:
                    collideAbility._sectorColliderPool.Release(sectorcollide,(sectorcollide)=>sectorcollide.ResetStatus());
                    break;
                default:
                    break;
            }
            collideAbility._collideDetectionDelegatePool.Release(this);
            collideObj = null;
        }
    }
}