using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Action.Data.Collide.EditorHelp;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Action.Data.Collide
{
    public class BindSectorCollideMono:BindCollideMono,ICollideObj
    {
        protected float insideRadius;
        protected float outsideRadius;

        protected float height;
        // 角度偏移量
        protected float angelPivot;
        // 角度范围
        protected float angle;
        
        List<Vector3> _startPoints = new List<Vector3>();
        List<Vector3> _endPoints = new List<Vector3>();
        List< ValueTuple<Vector3, Vector3>> _raycasts = new List<ValueTuple<Vector3, Vector3>>();
        
        [Button("设置参数")]
        public void SetParam(ActionCollideDetectionBaseData baseData)
        {
            var sectorData = baseData as ActionCollideDetectionSectorData;
            if (sectorData == null)
                return;
            center = transform;
            
            insideRadius = sectorData.insideRadius;
            outsideRadius = sectorData.outsideRadius;
            height = sectorData.height;
            angelPivot = sectorData.anglePivot;
            angle = sectorData.angle;
            localPosition = sectorData.localPosition;
            localRotation = sectorData.localRotation;
        }
        public void SetParam(Transform center,ActionCollideDetectionBaseData baseData)
        {
            var sectorData = baseData as ActionCollideDetectionSectorData;
            if (sectorData == null)
                return;
            this.center = center;
            
            insideRadius = sectorData.insideRadius;
            outsideRadius = sectorData.outsideRadius;
            height = sectorData.height;
            angelPivot = sectorData.anglePivot;
            angle = sectorData.angle;
            localPosition = sectorData.localPosition;
            localRotation = sectorData.localRotation;
        }
        
        //[Button("检测")]
        public void Detect(Action<Collider> callback, int layerMask)
        {
            var boxSize = new Vector3()
            {
                x = outsideRadius * 2,
                y = height,
                z = outsideRadius * 2
            };
            //Debug.Log("layerMask:" + layerMask);
           // 先用Box检测，进行初次筛选
           var boxResults = Physics.OverlapBox(Position, boxSize / 2, Rotation,layerMask);
           // 检测碰撞体是否处于外半径内或是已处于检测间隔期内
           var checkResults = new List<Collider>();
           foreach (var result in boxResults)
           {
               if (!result)
               {
                   continue;
               }

               var closestPoint = result.ClosestPoint(center.position);
               var distanceDirection = closestPoint - center.position;
               var sqrDistance = distanceDirection.sqrMagnitude;

               // 过滤外半径外的碰撞体
               if (sqrDistance > outsideRadius * outsideRadius)
               {
                   continue;
               }

               checkResults.Add(result);
           }
           // 如果粗略检测没有碰撞体，则直接返回
            if (checkResults.Count == 0)
            {
                return;
            }

            // 最后检测内半径到外半径之间的碰撞体，按照角度检测，规定180度检测一个周期的采样
            var angleStep = 90f;
            angle = Mathf.Clamp(angle, 0f, 360f);
            var startAngle =  angelPivot + -angle / 2;
            var endAngle = angelPivot + angle / 2;
            var forwardDirection = Rotation * Vector3.forward;
            while (startAngle < endAngle && checkResults.Count != 0)
            {
                var newStartAngle = startAngle;
                var newEndAngle = Mathf.Min(newStartAngle + angleStep, endAngle);

                // 计算起始线线段和结束线线段的方向
                var axisDirection = Rotation * Vector3.up;
                var startLineDirection = Quaternion.AngleAxis(newStartAngle, axisDirection) * forwardDirection;
                var endLineDirection = Quaternion.AngleAxis(newEndAngle, axisDirection) * forwardDirection;

                // 多次采样从起始线段的点向结束线段的点发射碰撞体
                var radiusStep = 0.5f;
                var newRadius = insideRadius;
                var stop = false;
                while (!stop && checkResults.Count != 0)
                {
                    // 如果处于扇形圆心就不发射碰撞体
                    if (Mathf.Approximately(newRadius, 0f))
                    {
                        newRadius += radiusStep;
                        continue;
                    }

                    // 在外半径上发射碰撞体
                    if (newRadius >= outsideRadius)
                    {
                        newRadius = outsideRadius;
                        stop = true;
                    }

                    // 这里起始线段的指定点向结束线段的指定点发射碰撞体
                    var startPoint = center.position + newRadius * startLineDirection;
                    var endPoint = center.position + newRadius * endLineDirection;
                    var direction = endPoint - startPoint;
                    var raycastHits = Physics.BoxCastAll(startPoint,
                        new Vector3(0.01f, height, 0.01f), direction,
                        Quaternion.LookRotation(direction), direction.magnitude);
                    _startPoints.Add(startPoint);
                    _endPoints.Add(endPoint);
                    _raycasts.Add(new ValueTuple<Vector3, Vector3>(startPoint, direction));

                    foreach (var raycastHit in raycastHits)
                    {
                        var result = raycastHit.collider;
                        if (!result)
                        {
                            continue;
                        }

                        // 如果是之前检测到的碰撞体，则认为处于内半径到外半径之间
                        if (checkResults.Contains(result))
                        {
                            callback?.Invoke(result);
                            // 移除已经检测到的碰撞体，不在后续检测
                            checkResults.Remove(result);
                            //Debug.Log($"[Test] 碰撞体{result.gameObject.name}");
                        }
                    }

                    newRadius += radiusStep;
                }

                startAngle += angleStep;
            }
        }

        public void CancelDetect()
        {
            throw new NotImplementedException();
        }

        public override void DrawGizmos()
        {
            if (center == null)
                return;
            //draw meshes
            var mesh = DebugDrawMeshCollide.Generate3DSectorMesh(insideRadius,outsideRadius,angelPivot,angle,Vector3.forward,height);
            Gizmos.color = Color.red;
            var matrix = new Matrix4x4();
            matrix.SetTRS(
                Position,
                Rotation,
                Vector3.one
            );
            Gizmos.matrix = matrix;
            Gizmos.DrawWireMesh(mesh, Vector3.zero, Quaternion.identity);
        }

        public void ResetStatus()
        {
            Debug.Log("Reset");
            center = null;
            localPosition = Vector3.zero;
            localRotation = Quaternion.identity;
            insideRadius = 0;
            outsideRadius = 0;
            angelPivot = 0;
            angle = 0;
            height = 0;
            _startPoints.Clear();
            _endPoints.Clear();
            _raycasts.Clear();
            gameObject.SetActive(false);
        }
    }
}