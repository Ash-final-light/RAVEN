using System;
using Common.Constant;
using Tools.DesignMode;
using Tools.InputHandle;
using UI.UIFramework.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
    public class GameManager:MonoSingleton<GameManager>
    {
        
        public static GameStatus CurGameStatus { get; private set; }
        public static SceneName CurScene { get; private set; }
        
        public static void SetGameStatus(GameStatus _status)
        {
            //设置game status
            CurGameStatus = _status;

            //设置Time scale
            if (CurGameStatus == GameStatus.Paused)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;

            //设置input action map
            if (CurGameStatus == GameStatus.Playing)
                PlayerInputManager.Instance.SwitchCurrentActionMap(InputConstants.GamePlayMap);
            else
                PlayerInputManager.Instance.SwitchCurrentActionMap(InputConstants.UIMap);
        }

        public static void SwitchScene(SceneName _scene)
        {
            CurScene = _scene;
            SceneManager.Instance.LoadScene(_scene.ToString(),LoadSceneMode.Single,null);
            SceneManager.Instance.UnloadSceneAsync(CurScene.ToString());
        }
    }
}