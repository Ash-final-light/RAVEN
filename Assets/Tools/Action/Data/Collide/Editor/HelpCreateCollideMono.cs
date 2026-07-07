#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Action.Data.Collide;
using Tools.Action.Data;
using Tools.Action.Data.Collide;
using Tools.Action.Data.Collide.EditorHelp;

namespace Action.Data.Collide.Editor
{
    public static class CreateCollideMenu
    {
        [MenuItem("GameObject/CollideMono/Box Collide", false, 10)]
        static void CreateBoxCollide(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("BoxCollide(Editor)");
            var collideMono = go.AddComponent<EditorBindBoxCollideMono>();
            collideMono.parent = go.transform;
            collideMono.detectionClipData = new ActionCollideDetectionClipData();
            collideMono.detectionClipData.type = ActionCollideDetectionType.Box;
            collideMono.detectionClipData.data = new ActionCollideDetectionBoxData();
            collideMono.detectionClipData.channelId = Guid.NewGuid().ToString();
            collideMono.ModifyDetectionData();
            
            // 设置对象的父对象
            GameObject parent = GetSelectedParent();
            if(parent != null)
            {
                go.transform.SetParent(parent.transform, false);
            }
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create BoxCollide");
        }

        [MenuItem("GameObject/CollideMono/Sphere Collide", false, 11)]
        static void CreateSphereCollide(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("SphereCollide(Editor)");
            var collideMono = go.AddComponent<EditorBindSphereCollideMono>();
            collideMono.parent = go.transform;
            collideMono.detectionClipData = new ActionCollideDetectionClipData();
            collideMono.detectionClipData.type = ActionCollideDetectionType.Sphere;
            collideMono.detectionClipData.data = new ActionCollideDetectionSphereData();
            collideMono.detectionClipData.channelId = Guid.NewGuid().ToString();
            collideMono.ModifyDetectionData();
            
            // 设置对象的父对象
            GameObject parent = GetSelectedParent();
            if(parent != null)
            {
                go.transform.SetParent(parent.transform, false);
            }
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create SphereCollide");
        }

        [MenuItem("GameObject/CollideMono/Sector Collide", false, 12)]
        static void CreateSectorCollide(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("SectorCollide(Editor)");
            var collideMono = go.AddComponent<EditorBindSectorCollideMono>();
            collideMono.parent = go.transform;
            collideMono.detectionClipData = new ActionCollideDetectionClipData();
            collideMono.detectionClipData.type = ActionCollideDetectionType.Sector;
            collideMono.detectionClipData.data = new ActionCollideDetectionSectorData();
            collideMono.detectionClipData.channelId = Guid.NewGuid().ToString();
            collideMono.ModifyDetectionData();
            
            // 设置对象的父对象
            GameObject parent = GetSelectedParent();
            if(parent != null)
            {
                go.transform.SetParent(parent.transform, false);
            }
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create SectorCollide");
        }

        // 获取当前选中的对象作为父对象
        private static GameObject GetSelectedParent()
        {
            if(Selection.activeTransform != null)
            {
                return Selection.activeTransform.gameObject;
            }
            return null;
        }
    }
}
#endif