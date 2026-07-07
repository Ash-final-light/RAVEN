using Tools.CustomObject.Interface;
using UnityEngine;
using VContainer;

namespace Package
{
    public class testpackage:MonoBehaviour
    {
        [Inject]
        private IPackageObjectProvider packageObjectProvider;
        [Inject]
        private IPackageObjectManager packageObjectManager;
        private void Start()
        {
            /*packageObjectProvider.GetObjectInfoById(1);
            packageObjectManager.AddObjectById(2,1);
            packageObjectManager.AddObjectById(3,1);*/
        }
    }
}