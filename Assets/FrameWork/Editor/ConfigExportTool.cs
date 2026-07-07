using System.IO;
using UnityEditor;
using UnityEngine;

// 放在Editor文件夹，Unity编辑器菜单
namespace FrameWork.Editor
{
    public class ConfigExportTool
    {
        #region 原有：快速打开配置目录（保留，新增角色Excel目录可扩展，此处保持原有结构）

        [MenuItem("Window/打开配置目录/Excel目录")]
        public static void OpenExcelDir()
        {
            string excelDir = Path.Combine(Application.dataPath, "GameData/Excel");
            Directory.CreateDirectory(excelDir);
            Application.OpenURL($"file:///{excelDir}");
        }

        [MenuItem("Window/打开配置目录/Bin输出目录")]
        public static void OpenJsonDir()
        {
            string jsonDir = Path.Combine(Application.dataPath, "StreamingAssets/Config");
            Directory.CreateDirectory(jsonDir);
            Application.OpenURL($"file:///{jsonDir}");
        }

        #endregion
    }
}