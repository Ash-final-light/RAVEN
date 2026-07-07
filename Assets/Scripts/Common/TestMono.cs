using System;
using Sirenix.OdinInspector;
using Tools.Buff;
using UnityEngine;
using VContainer;

namespace Common
{
    public class TestMono:MonoBehaviour
    {
        [Inject] private LuaManager luaManager;
        [Inject,ShowInInspector] private BuffManager buffManager;

        private void Start()
        {
            InvokeRepeating(nameof(UpdateBuff),0,1f);
        }
        private void UpdateBuff()
        {
            buffManager.UpdateBuff(1f);
        }
    }
}