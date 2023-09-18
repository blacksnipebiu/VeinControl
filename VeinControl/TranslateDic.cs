using System.Collections.Generic;

namespace VeinControl
{
    public static class TranslateDic
    {
        private static Dictionary<string, string> TranslateDict;
        public static string Translate(this string s) => Localization.language != Language.zhCN && TranslateDict.ContainsKey(s) && TranslateDict[s].Length > 0 ? TranslateDict[s] : s;
        public static void RegallTranslate()
        {
            TranslateDict = new Dictionary<string, string>
            {
                { "启动/关闭", "On/Off" },
                { "移动矿堆失败", "Failed to move the pile" },
                { "当前纬度过高，为避免出错，无法移动矿堆", "The current latitude is too high to move the pile to avoid errors" },
                { "确定", "recognize" },
                { "移动所有矿", "Move all veins" },
                { "切割出", "Cut out" },
                { "整理为", "Organized as" },
                { "个", "veins" },
                { "行", "lines" },
                { "合并油井", "Merge Oil" },
                { "合并矿脉", "Merge VeinGroup" },
                { "矿脉工具", "Vein Control Tool" },
                { "不添加", "No Add Vein" },
                { "警告：不要将油井移动到极点附近", "WARNING:Don'T move veins near poles" },
                { "ctrl+鼠标左键:移动单矿", "ctrl+LeftMouse：Move Vein" },
                { "alt+鼠标左键:移动矿堆", "alt+LeftMouse：Move the vein group" },
                { "shift+鼠标左键:移动所有矿", "shift+LeftMouse：Move all veins" },
                { "alt+x+鼠标左键:切割矿脉", "alt+x+LeftMouse：Cut the veins from vein group" },
                { "X:进入拆除模式可以拆除矿脉", "x:Remove vein on Dismantle Mode" },
                { "~(Esc键下方):合并/不合并", "~:Merge or not merge " }
            };
        }
    }
}
