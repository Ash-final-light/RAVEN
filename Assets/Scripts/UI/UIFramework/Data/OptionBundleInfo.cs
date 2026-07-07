using System.Collections.Generic;
using System.Linq;

namespace UI.UIFramework.Data
{
    /// <summary>
    /// Excel表格数据
    /// </summary>
    public class OptionBundleInfo
    {
        public string OptionParent;
        public string OptionChilds;
    }

    public class OptionBundle
    {
        public string OptionParent;
        public List<string> OptionChilds;
    }

    public static class OptionBundleMethod
    {
        public static OptionBundle ToOptionBundle(this OptionBundleInfo info)
        {
            return new OptionBundle
            {
                OptionParent = info.OptionParent,
                OptionChilds = new List<string>(info.OptionChilds.Split(',').Select(s => s.Trim()))
            };
        }
    }
}
