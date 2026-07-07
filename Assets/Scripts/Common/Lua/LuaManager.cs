using System;
using System.IO;
using UnityEngine;
using VContainer.Unity;
using XLua;

namespace Common
{
    public class LuaManager: IStartable, ITickable, IDisposable
    {
        private LuaEnv luaEnv;
        private bool disposed;

        /// <summary>
        /// 得到lua当中的_G
        /// </summary>
        public LuaTable Global
        {
            get { return luaEnv.Global; }
        }
        public LuaEnv GetLuaEnv()
        {
            return luaEnv;
        }
        public void Init()
        {
            if(luaEnv != null)
            {
                return;
            }

            disposed = false;
            luaEnv = new LuaEnv();
            //加载lua脚本重定向
            luaEnv.AddLoader(MyLoader);
            luaEnv.AddLoader(MyLoaderFromAB);
            DoLuaFile("Main");
        }
        private byte[] MyLoader(ref string filePath)
        {
            //传进去的是lua脚本文件名
            string path = Application.dataPath + "/Lua/" + filePath + ".lua";
            
            //File知识点
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                Debug.Log("文件不存在"+filePath);
            }
            
            //用其中方法去加载文件
            return null;
        }
        
        //lua脚本会放在ab包中，加载ab包中的lua脚本
        //ab包中加载文本，后续还是有限制.lua
        //后缀改成ab包
        private byte[] MyLoaderFromAB(ref string filePath)
        {
            //load from ab
            TextAsset lua = AbManager.Instance.LoadRes<TextAsset>("lua", filePath+".lua");
            if (lua != null)
            {
                return lua.bytes;
            }
            else
            {
                Debug.Log("ab包中加载lua脚本失败"+filePath);
            }
            return null;
        }
        
        public void DoLuaFile(string filename)
        {
            string str = $"require('{filename}')";
            Dosting(str);
        }
        private void Dosting(string str)
        {
            if (luaEnv == null)
            {
                Debug.Log("luaEnv为空");
                return;
            }
            luaEnv.DoString(str);
        }
        
        public object[] DoLuaFileWithReturn(string filename)
        {
            // 注意：require 会缓存脚本。如果脚本内容改变，require 不会重新执行。
            // 如果是 Buff 这种需要产生新实例的，建议用 DoString(File.ReadAllText) 或保持 require 但确保 Lua 返回的是类/构造函数
            string str = $"return require('{filename}')"; 
            return DoStringWithReturn(str);
        }

        private object[] DoStringWithReturn(string str)
        {
            if (luaEnv == null)
            {
                Debug.LogError("luaEnv为空");
                return null;
            }
            return luaEnv.DoString(str);
        }

        public void Tick()
        {
            if (luaEnv == null)
            {
                Debug.Log("luaEnv为空");
                return;
            }
            luaEnv.Tick();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (luaEnv == null)
            {
                return;
            }

            luaEnv.Tick();
            luaEnv.FullGc();
            luaEnv.Dispose();
            luaEnv = null;
        }

        public void Start() => Init();
    }
}
