using Sirenix.OdinInspector;
using UnityEngine;

namespace Test
{
    public class TestInstance:MonoBehaviour
    {
        public GameObject prefab;
        public Transform parent;
        public Vector3 position = Vector3.one;
        public Quaternion rotation;
        public Vector3 scale;
        
        private GameObject instance;
        [Button("Create")]
        public void Create()
        {
            instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(parent);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = scale;
        }

        [Button("Transform")]
        public void Transform()
        {
            instance.transform.SetParent(null);
        }
    }
}