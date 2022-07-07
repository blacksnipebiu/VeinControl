﻿using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VeinControl
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]

    public class VeinControl : BaseUnityPlugin
    {
        private Dictionary<string, string> TranslateDict;
        public const string GUID = "cn.blacksnipe.dsp.VeinControl";
        public const string NAME = "VeinControl";
        public const string VERSION = "1.0.4";
        public const string GAME_PROCESS = "DSPGAME.exe";
        public int MaxHeight;
        public int MaxWidth;
        public int veintype;
        public bool ShowWindow;
        public bool ShowInstructionsWindow;
        public bool Window_moving;
        public bool ScaleChanging;
        public bool FirstOpen;
        public bool dropdownbutton;
        public VeinData pointveindata;
        public float Temp_Window_X;
        public float Temp_Window_Y;
        public float Temp_Window_Moving_X;
        public float Temp_Window_Moving_Y;
        public float Temp_Window_X_move;
        public float Temp_Window_Y_move;
        public float Window_Width;
        public float Window_Height;
        public float Window_X;
        public float Window_Y;
        public static ConfigEntry<int> scale;
        public static ConfigEntry<int> changeveinsposx;
        public static ConfigEntry<int> veinlines;
        public static ConfigEntry<Boolean> ControlToggol;
        public static ConfigEntry<Boolean> MergeOil;
        public static ConfigEntry<Boolean> MergeVeinGroup;
        public static ConfigEntry<KeyboardShortcut> OpenWindowKey; 
        public string getTranslate(string s) => Localization.language != Language.zhCN && TranslateDict.ContainsKey(s) && TranslateDict[s].Length > 0 ? TranslateDict[s] : s;

        private void Start()
        {
            ShowWindow = false;
            FirstOpen = true;
            veintype = 0;
            TranslateDict = new Dictionary<string, string>();
            RegallTranslate();
            changeveinsposx = Config.Bind("切割矿脉数量", "changeveinsposx", 9);
            veinlines = Config.Bind("矿物行数", "veinlines", 3);
            ControlToggol= Config.Bind("启动/关闭", "ControlToggol", true);
            MergeOil = Config.Bind("合并油井", "MergeOil", false);
            MergeVeinGroup = Config.Bind("合并矿堆", "MergeVeinGroup", false);
            OpenWindowKey = Config.Bind("打开窗口快捷键", "Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Q, KeyCode.LeftControl));
            scale = Config.Bind("大小适配", "scale", 16);
            Window_X = 450;
            Window_Y = 200;
            Temp_Window_X = Window_X;
            Temp_Window_Y = Window_Y;
            Window_Width = scale.Value *35 ;
            Window_Height = scale.Value*5+10;
            Temp_Window_Moving_X = 0;
            Temp_Window_Moving_Y = 0;
        }
        
        private void Update()
        {
            MaxHeight = Screen.height;
            MaxWidth = Screen.width;
            if (OpenWindowKey.Value.IsDown())
            {
                ShowWindow = !ShowWindow;
            }
            if(Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift))
            {
                veintype = 0;
            }
            if (ShowWindow && Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) { scale.Value++; ScaleChanging = true; }
                if (Input.GetKeyDown(KeyCode.DownArrow)) { scale.Value--; ScaleChanging = true; }
                if (scale.Value <= 5) scale.Value = 5;
                if (scale.Value > 35) scale.Value = 35;
            }
            controlVein();
        }

        public void OnGUI()
        {
            if (ShowWindow)
            {
                if (ScaleChanging || FirstOpen)
                {
                    ScaleChanging = false;
                    FirstOpen = false;
                    GUI.skin.label.fontSize = scale.Value;
                    GUI.skin.button.fontSize = scale.Value;
                    GUI.skin.toggle.fontSize = scale.Value;
                    GUI.skin.textField.fontSize = scale.Value;
                    GUI.skin.textArea.fontSize = scale.Value;
                    Window_Width = scale.Value * 15;
                    Window_Height = (scale.Value+4) * 5;
                }
                else if (!ScaleChanging && GUI.skin.toggle.fontSize != scale.Value)
                {
                    scale.Value = GUI.skin.toggle.fontSize;
                }
                Rect windowRect = new Rect(Temp_Window_X, Temp_Window_Y, Window_Width, Window_Height);
                moveWindow(ref Temp_Window_X, ref Temp_Window_Y, ref Temp_Window_X_move, ref Temp_Window_Y_move, ref Window_moving, ref Temp_Window_Moving_X, ref Temp_Window_Moving_Y, Window_Width);
                windowRect = GUI.Window(20220702, windowRect, VeinControlConfigWindow, getTranslate("矿脉工具") + "(" + VERSION + ")" + "ps:ctrl+↑↓");
                Window_X = Temp_Window_X;
                Window_Y = Temp_Window_Y;
                if (ShowInstructionsWindow)
                {
                    Rect windowRect1 = new Rect(Temp_Window_X+ Window_Width, Temp_Window_Y, Localization.language != Language.zhCN ? (scale.Value + 4) * 20 : Window_Width,  (scale.Value + 4) * 8);
                    moveWindow(ref Temp_Window_X, ref Temp_Window_Y, ref Temp_Window_X_move, ref Temp_Window_Y_move, ref Window_moving, ref Temp_Window_Moving_X, ref Temp_Window_Moving_Y, Window_Width);
                    windowRect1 = GUI.Window(20220703, windowRect1, InstructionsWindow, "");
                    Window_X = Temp_Window_X;
                    Window_Y = Temp_Window_Y;
                }
            }
        }
        
        public void VeinControlConfigWindow(int winId)
        {
            int UIHeight = GUI.skin.toggle.fontSize + 4;
            int UIWidth = scale.Value * 13;
            int Lines = 0;
            GUILayout.BeginArea(new Rect(10,30, Window_Width, Window_Height));
            if (GUI.Button(new Rect(UIWidth- UIHeight, UIHeight * Lines, UIHeight, UIHeight + 5), "?"))
            {
                ShowInstructionsWindow = !ShowInstructionsWindow;
            }
            ControlToggol.Value = GUI.Toggle(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight), ControlToggol.Value, getTranslate("启动/关闭"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight+5), getTranslate("整理为") + $" {veinlines.Value } " + getTranslate("行"));
            veinlines.Value = (int)GUI.HorizontalSlider(new Rect(0, UIHeight/2+ UIHeight * Lines++, UIWidth, UIHeight), veinlines.Value, 1, 20);

            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight+5), getTranslate("切割出") + $" {changeveinsposx.Value } "  + getTranslate("个"));
            changeveinsposx.Value = (int)GUI.HorizontalSlider(new Rect(0, UIHeight / 3 + UIHeight * Lines++, UIWidth, UIHeight), changeveinsposx.Value, 2, 72);
            MergeOil.Value = GUI.Toggle(new Rect(0, UIHeight * Lines++, UIWidth,UIHeight),MergeOil.Value, getTranslate("合并油井"));
            if (GUI.Button(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight), veintype==0? getTranslate("不添加") : LDB.ItemName(int.Parse(VeintypechineseTranslate(veintype, 1).Trim()))))
            {
                dropdownbutton = !dropdownbutton;
            }
            if (dropdownbutton)
            {
                for (int i = 0; i <= 14; i++)
                {
                    if (veintype == i) continue;
                    if (GUI.Button(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight), i == 0 ? getTranslate("不添加") : LDB.ItemName(int.Parse(VeintypechineseTranslate(i, 1).Trim()))))
                    {
                        dropdownbutton = !dropdownbutton;
                        veintype = i;
                    }
                }
            }
            Window_Height= UIHeight * (Lines+2);
            GUILayout.EndArea();
        }

        public void InstructionsWindow(int winId)
        {
            int UIHeight = GUI.skin.toggle.fontSize + 4;
            int UIWidth = (int)(Localization.language != Language.zhCN ? (scale.Value + 4) * 20 : Window_Width);
            int Lines = 0;
            GUILayout.BeginArea(new Rect(10, 30, UIWidth, (scale.Value + 4) * 8));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("ctrl+鼠标左键:移动单矿"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("alt+鼠标左键:移动矿堆"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("shift+鼠标左键:移动所有矿"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("alt+x+鼠标左键:切割矿脉"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("X:进入拆除模式可以拆除矿脉"));
            GUI.Label(new Rect(0, UIHeight * Lines++, UIWidth, UIHeight + 5), getTranslate("~(Esc键下方):合并/不合并"));

            GUILayout.EndArea();
        }
        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="x_move"></param>
        /// <param name="y_move"></param>
        /// <param name="movewindow"></param>
        /// <param name="tempx"></param>
        /// <param name="tempy"></param>
        /// <param name="x_width"></param>
        public void moveWindow(ref float x, ref float y, ref float x_move, ref float y_move, ref bool movewindow, ref float tempx, ref float tempy, float x_width)
        {
            Vector2 temp = Input.mousePosition;
            if (temp.x > x && temp.x < x + x_width && MaxHeight - temp.y > y && MaxHeight - temp.y < y + 20)
            {
                if (Input.GetMouseButton(0))
                {
                    if (!movewindow)
                    {
                        x_move = x;
                        y_move = y;
                        tempx = temp.x;
                        tempy = MaxHeight - temp.y;
                    }
                    movewindow = true;
                    x = x_move + temp.x - tempx;
                    y = y_move + (MaxHeight - temp.y) - tempy;
                }
                else
                {
                    movewindow = false;
                    tempx = x;
                    tempy = y;
                }
            }
            else if (movewindow)
            {
                movewindow = false;
                x = x_move + temp.x - tempx;
                y = y_move + (MaxHeight - temp.y) - tempy;
            }
        }

        /// <summary>
        /// 控制矿脉
        /// </summary>
        public void controlVein()
        {
            if (GameMain.mainPlayer == null || GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;
            if (Input.GetMouseButton(0))
            {
                if (GameMain.mainPlayer.controller.actionBuild.dismantleTool.active)
                {
                    if (ControlToggol.Value)
                    {
                        RemoveVeinByMouse();
                    }
                }
                else if (veintype!=0)
                {
                    AddVein(veintype, 1000000000, new Vector3());
                }
            }
            if (veintype!=0 && Input.GetMouseButton(1))
            {
                veintype = 0;
            }
            if (ControlToggol.Value && Input.GetMouseButton(0) && !GameMain.mainPlayer.controller.actionBuild.dismantleTool.active)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pointveindata = GetVeinByMouse();
                }
                if (pointveindata.amount != 0)
                {
                    if (Input.GetKeyDown(KeyCode.BackQuote))
                    {
                        MergeVeinGroup.Value = !MergeVeinGroup.Value;
                    }
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        GetAllVein(pointveindata);
                    }
                    else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.X))
                    {
                        SplitxVeinsFrom(pointveindata);
                    }
                    else if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        ChangeVeinGroupPos(pointveindata);
                    }
                    else if (Input.GetKey(KeyCode.LeftControl))
                    {
                        RaycastHit raycastHit1;
                        if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                            return;
                        Vector3 raycastpos = raycastHit1.point;
                        ChangeVeinPos(pointveindata, raycastpos);
                    }
                }
            }
        }

        /// <summary>
        /// 添加矿脉
        /// </summary>
        /// <param name="veintype"></param>
        /// <param name="number"></param>
        /// <param name="pos"></param>
        public void AddVein(int veintype, int number, Vector3 pos)
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return;
            Vector3 raycastpos = raycastHit1.point;
            if (pos.magnitude == 0)
            {
                foreach (VeinData i in pd.factory.veinPool)
                {
                    if (i.type == EVeinType.None) continue;
                    if ((raycastpos - i.pos).magnitude < 1)
                    {
                        if (i.type != EVeinType.Oil)
                        {
                            pd.factory.veinGroups[i.groupIndex].amount += 1000000000 - pd.factory.veinPool[i.id].amount;
                            pd.factory.veinPool[i.id].amount = 1000000000;
                        }
                        else
                        {
                            pd.factory.veinPool[i.id].amount += (int)(1 / VeinData.oilSpeedMultiplier);
                            pd.factory.veinGroups[i.groupIndex].amount += (int)(1 / VeinData.oilSpeedMultiplier);
                        }
                        return;
                    }
                }
                pos = raycastpos;
            }
            pos = raycastpos;
            VeinData vein = new VeinData()
            {
                amount = veintype == 7 ? (int)(1 / VeinData.oilSpeedMultiplier) : number,
                type = (EVeinType)veintype,
                pos = pos,
                productId = LDB.veins.Select(veintype).MiningItem,
                modelIndex = (short)LDB.veins.Select(veintype).ModelIndex
            };
            vein.id = pd.factory.AddVeinData(vein);
            vein.colliderId = pd.physics.AddColliderData(LDB.veins.Select(veintype).prefabDesc.colliders[0].BindToObject(vein.id, 0, EObjectType.Vein, vein.pos, Quaternion.FromToRotation(Vector3.up, vein.pos.normalized)));
            vein.modelId = pd.factoryModel.gpuiManager.AddModel(vein.modelIndex, vein.id, vein.pos, Maths.SphericalRotation(vein.pos, UnityEngine.Random.value * 360f));
            vein.minerCount = 0;
            pd.factory.AssignGroupIndexForNewVein(ref vein);
            pd.factory.veinPool[vein.id] = vein;
            pd.factory.RefreshVeinMiningDisplay(vein.id, 0, 0);
            pd.factory.RecalculateVeinGroup(pd.factory.veinPool[vein.id].groupIndex);
        }

        /// <summary>
        /// 根据鼠标射线获取矿物
        /// </summary>
        /// <returns></returns>
        public VeinData GetVeinByMouse()
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return new VeinData();
            Vector3 raycastpos = raycastHit1.point;
            foreach (VeinData i in pd.factory.veinPool)
            {
                if (i.id == 0) continue;
                if ((raycastpos - i.pos).magnitude < 2 && i.type != EVeinType.None)
                {
                    return i;
                }
            }
            return new VeinData();
        }

        /// <summary>
        /// 根据鼠标射线删除矿物
        /// </summary>
        public void RemoveVeinByMouse()
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return;
            Vector3 raycastpos = raycastHit1.point;
            foreach (VeinData i in pd.factory.veinPool)
            {
                if ((raycastpos - i.pos).magnitude < 1 && i.type != EVeinType.None)
                {
                    pd.factory.veinGroups[i.groupIndex].count--;
                    pd.factory.veinGroups[i.groupIndex].amount -= i.amount;
                    pd.factory.RemoveVeinWithComponents(i.id);
                    if (pd.factory.veinGroups[i.groupIndex].count == 0)
                    {
                        pd.factory.veinGroups[i.groupIndex].type = 0;
                        pd.factory.veinGroups[i.groupIndex].amount = 0;
                        pd.factory.veinGroups[i.groupIndex].pos = Vector3.zero;
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// 改变矿堆的位置
        /// </summary>
        /// <param name="vd">目标矿脉</param>
        public void ChangeVeinGroupPos(VeinData vd)
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return;
            Vector3 raycastpos = raycastHit1.point;
            VeinData[] veinPool = pd.factory.veinPool;
            int colliderId;
            Vector3 begin = veinPool[vd.id].pos;
            int index = 0;
            foreach (VeinData vd1 in veinPool)
            {
                if (vd1.pos == null || vd1.id <= 0) continue;
                int VeinId = vd1.id;
                if (vd1.groupIndex == veinPool[vd.id].groupIndex)
                {
                    if (vd.type == EVeinType.Oil && MergeOil.Value && vd.id != VeinId)
                    {
                        pd.factory.veinPool[vd.id].amount += vd1.amount;
                        pd.factory.veinGroups[vd.groupIndex].count--;
                        pd.factory.RemoveVeinWithComponents(vd1.id);
                        pd.factory.RecalculateVeinGroup(pd.factory.veinPool[vd.id].groupIndex);
                        pd.factory.ArrangeVeinGroups();
                        continue;
                    }
                    if (MergeVeinGroup.Value)
                    {
                        veinPool[VeinId].pos = raycastpos;
                    }
                    else
                    {
                        Vector3 temp = PostionCompute(begin, raycastpos, vd1.pos, index++, vd.type == EVeinType.Oil);
                        if (Vector3.Distance(temp, vd1.pos) < 0.01) continue;
                        veinPool[VeinId].pos = temp;
                        if (float.IsNaN(veinPool[VeinId].pos.x) || float.IsNaN(veinPool[VeinId].pos.y) || float.IsNaN(veinPool[VeinId].pos.z))
                        {
                            continue;
                        }
                    }
                    colliderId = veinPool[VeinId].colliderId;
                    pd.physics.RemoveColliderData(colliderId);
                    veinPool[VeinId].colliderId = pd.physics.AddColliderData(LDB.veins.Select((int)veinPool[VeinId].type).prefabDesc.colliders[0].BindToObject(VeinId, 0, EObjectType.Vein, veinPool[VeinId].pos, Quaternion.FromToRotation(Vector3.up, veinPool[VeinId].pos.normalized)));

                    pd.factoryModel.gpuiManager.AlterModel(veinPool[VeinId].modelIndex, veinPool[VeinId].modelId, VeinId, veinPool[VeinId].pos, Maths.SphericalRotation(veinPool[VeinId].pos, 90f));

                }
            }

            pd.factory.RecalculateVeinGroup(vd.groupIndex);
            pd.factory.ArrangeVeinGroups();
        }

        /// <summary>
        /// 改变单矿的位置
        /// </summary>
        /// <param name="vd"></param>
        /// <param name="pos"></param>
        public void ChangeVeinPos(VeinData vd, Vector3 pos)
        {
            PlanetData pd = GameMain.localPlanet;
            int VeinId = vd.id;
            VeinData[] veinPool = pd.factory.veinPool;
            veinPool[VeinId].pos = pos;
            int colliderId = veinPool[VeinId].colliderId;
            pd.physics.RemoveColliderData(colliderId);
            veinPool[VeinId].colliderId = pd.physics.AddColliderData(LDB.veins.Select((int)veinPool[VeinId].type).prefabDesc.colliders[0].BindToObject(VeinId, 0, EObjectType.Vein, veinPool[VeinId].pos, Quaternion.FromToRotation(Vector3.up, veinPool[VeinId].pos.normalized)));
            pd.factoryModel.gpuiManager.AlterModel(veinPool[VeinId].modelIndex, veinPool[VeinId].modelId, VeinId, veinPool[VeinId].pos, Maths.SphericalRotation(veinPool[VeinId].pos, 90f));
            bool leave = false;
            int origingroup = -1;
            if (pd.factory.veinGroups[veinPool[VeinId].groupIndex].count > 1)
            {
                Vector3 vector3 = pos - pd.factory.veinGroups[veinPool[VeinId].groupIndex].pos * (pd.realRadius + 2.5f);
                if (vector3.magnitude > 10.0)
                {
                    leave = true;
                    origingroup = veinPool[VeinId].groupIndex;
                    veinPool[VeinId].groupIndex = -1;
                }
            }
            else
            {
                pd.factory.veinGroups[veinPool[VeinId].groupIndex].pos = veinPool[VeinId].pos / (pd.realRadius + 2.5f);
                foreach (VeinData veindata in pd.factory.veinPool)
                {
                    if (veindata.type == veinPool[VeinId].type && veindata.groupIndex != origingroup && (veindata.pos - veinPool[VeinId].pos).magnitude < 10)
                    {
                        origingroup = veinPool[VeinId].groupIndex;
                        veinPool[VeinId].groupIndex = veindata.groupIndex;
                        pd.factory.RecalculateVeinGroup(origingroup);
                    }
                }
            }
            if (leave)
            {
                pd.factory.RecalculateVeinGroup(origingroup);
                foreach (VeinData veindata in pd.factory.veinPool)
                {
                    if (veindata.type == veinPool[VeinId].type && veindata.groupIndex != origingroup && (veindata.pos - veinPool[VeinId].pos).magnitude < 10)
                    {
                        veinPool[VeinId].groupIndex = veindata.groupIndex;
                    }
                }
                if (veinPool[VeinId].groupIndex == -1)
                {
                    veinPool[VeinId].groupIndex = (short)pd.factory.AddVeinGroup(veinPool[VeinId].type, veinPool[VeinId].pos.normalized);
                }
            }
            pd.factory.RecalculateVeinGroup(veinPool[VeinId].groupIndex);
            pd.factory.ArrangeVeinGroups();
        }

        /// <summary>
        /// 获取当前星球所有目标矿脉
        /// </summary>
        /// <param name="vd"></param>
        public void GetAllVein(VeinData vd)
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return;
            Vector3 raycastpos = raycastHit1.point;
            VeinData[] veinPool = pd.factory.veinPool;
            int colliderId;
            Vector3 begin = veinPool[vd.id].pos;
            int index = 0;
            foreach (VeinData vd1 in veinPool)
            {
                if (vd1.pos == null || vd1.id <= 0 || vd1.type != vd.type) continue;
                int VeinId = vd1.id;
                if (vd.type == EVeinType.Oil && MergeOil.Value && vd.id != VeinId)
                {
                    pd.factory.veinPool[vd.id].amount += vd1.amount;
                    pd.factory.veinGroups[vd.groupIndex].amount += vd1.amount;
                    pd.factory.veinGroups[vd1.groupIndex].count--;
                    pd.factory.RemoveVeinWithComponents(vd1.id);
                    pd.factory.RecalculateVeinGroup(pd.factory.veinPool[vd.id].groupIndex);
                    pd.factory.RecalculateVeinGroup(vd1.groupIndex);
                    pd.factory.ArrangeVeinGroups();
                    continue ;
                }
                veinPool[VeinId].pos = MergeVeinGroup.Value ? raycastpos : PostionCompute(begin, raycastpos, vd1.pos, index++, vd.type == EVeinType.Oil);
                if (float.IsNaN(veinPool[VeinId].pos.x) || float.IsNaN(veinPool[VeinId].pos.y) || float.IsNaN(veinPool[VeinId].pos.z))
                {
                    continue;
                }
                if (vd1.groupIndex != vd.groupIndex)
                {
                    int origingroup = veinPool[vd1.id].groupIndex;
                    veinPool[vd1.id].groupIndex = vd.groupIndex;
                    pd.factory.RecalculateVeinGroup(origingroup);
                    pd.factory.RecalculateVeinGroup(vd.groupIndex);
                    pd.factory.ArrangeVeinGroups();
                }
                colliderId = veinPool[VeinId].colliderId;
                pd.physics.RemoveColliderData(colliderId);
                veinPool[VeinId].colliderId = pd.physics.AddColliderData(LDB.veins.Select((int)veinPool[VeinId].type).prefabDesc.colliders[0].BindToObject(VeinId, 0, EObjectType.Vein, veinPool[VeinId].pos, Quaternion.FromToRotation(Vector3.up, veinPool[VeinId].pos.normalized)));

                pd.factoryModel.gpuiManager.AlterModel(veinPool[VeinId].modelIndex, veinPool[VeinId].modelId, VeinId, veinPool[VeinId].pos, Maths.SphericalRotation(veinPool[VeinId].pos, 90f));
            }
            pd.factory.RecalculateVeinGroup(vd.groupIndex);
            pd.factory.ArrangeVeinGroups();
        }

        /// <summary>
        /// 切割矿脉
        /// </summary>
        /// <param name="vd"></param>
        public void SplitxVeinsFrom(VeinData vd)
        {
            PlanetData pd = GameMain.localPlanet;
            RaycastHit raycastHit1;
            if (!Physics.Raycast(GameMain.mainPlayer.controller.mainCamera.ScreenPointToRay(Input.mousePosition), out raycastHit1, 800f, 8720, (QueryTriggerInteraction)2))
                return;
            if (pd.factory.veinGroups[pd.factory.veinPool[vd.id].groupIndex].count <= changeveinsposx.Value)
            {
                ChangeVeinGroupPos(vd);
                return;
            }

            Vector3 raycastpos = raycastHit1.point;
            VeinData[] veinPool = pd.factory.veinPool;
            int colliderId;
            Vector3 begin = veinPool[vd.id].pos;
            bool find = false;
            List<int> veinids = new List<int>();
            foreach (VeinData vd1 in veinPool)
            {
                if (vd1.pos == null || vd1.id <= 0) continue;
                if (vd1.groupIndex == vd.groupIndex)
                {
                    int VeinId = vd1.id;
                    if (vd.id == VeinId) find = true;
                    if (!find && veinids.Count == changeveinsposx.Value - 1) continue;
                    veinids.Add(vd1.id);
                    if (veinids.Count == changeveinsposx.Value) break;
                }
            }
            if (veinids.Count != changeveinsposx.Value) return;
            int index = 0;
            foreach (int VeinId in veinids)
            {
                veinPool[VeinId].pos = MergeVeinGroup.Value ? raycastpos : PostionCompute(begin, raycastpos, veinPool[VeinId].pos, index++);
                if (float.IsNaN(veinPool[VeinId].pos.x) || float.IsNaN(veinPool[VeinId].pos.y) || float.IsNaN(veinPool[VeinId].pos.z))
                {
                    continue;
                }
                colliderId = veinPool[VeinId].colliderId;
                pd.physics.RemoveColliderData(colliderId);
                veinPool[VeinId].colliderId = pd.physics.AddColliderData(LDB.veins.Select((int)veinPool[VeinId].type).prefabDesc.colliders[0].BindToObject(VeinId, 0, EObjectType.Vein, veinPool[VeinId].pos, Quaternion.FromToRotation(Vector3.up, veinPool[VeinId].pos.normalized)));

                pd.factoryModel.gpuiManager.AlterModel((int)veinPool[VeinId].modelIndex, veinPool[VeinId].modelId, VeinId, veinPool[VeinId].pos, Maths.SphericalRotation(veinPool[VeinId].pos, 90f));

            }
            bool leave = true;
            foreach (VeinData vd1 in veinPool)
            {
                if (veinids.Contains(vd1.id) || vd1.type != vd.type)
                {
                    continue;
                }
                else if ((pd.factory.veinPool[vd.id].pos - vd1.pos).magnitude < 5)
                {
                    leave = false;
                    break;
                }
            }
            if (leave)
            {
                int origingroup = pd.factory.veinPool[vd.id].groupIndex;
                pd.factory.veinPool[vd.id].groupIndex = (short)pd.factory.AddVeinGroup(vd.type, vd.pos.normalized);
                foreach (int veinid in veinids)
                {
                    if (veinid == vd.id) continue;
                    else
                    {
                        pd.factory.veinPool[veinid].groupIndex = pd.factory.veinPool[vd.id].groupIndex;
                    }
                }
                pd.factory.RecalculateVeinGroup(pd.factory.veinPool[vd.id].groupIndex);
                pd.factory.RecalculateVeinGroup(origingroup);
                pd.factory.ArrangeVeinGroups();
            }

        }

        /// <summary>
        /// 计算矿物位置变化
        /// </summary>
        /// <param name="begin">起始位置</param>
        /// <param name="end">终点位置</param>
        /// <param name="pointpos">改变矿脉的位置</param>
        /// <param name="index">改变矿脉的下标</param>
        /// <param name="oil">是否为原油</param>
        /// <returns></returns>
        public Vector3 PostionCompute(Vector3 begin, Vector3 end, Vector3 pointpos, int index, bool oil = false)
        {
            if (end.y > 193 || end.y < -193) return pointpos;
            if((MergeOil.Value && oil) || MergeVeinGroup.Value)
            {
                return end;
            }
            Vector3 pos1 = begin;
            Vector3 pos2 = end;
            Vector3 pos3;
            float radius = GameMain.localPlanet.realRadius;
            Quaternion quaternion2 = Maths.SphericalRotation(pos1, 0);
            float areaRadius = oil ? 15 : 1.5f;
            if (!oil)
            {
                pos2.x = (int)pos2.x;
                pos2.z = (int)pos2.z;
                pos2.y = (int)pos2.y;
                pos3 = pos1 + quaternion2 * (new Vector3(index / veinlines.Value, 0, index % veinlines.Value) * areaRadius);
            }
            else
                pos3 = pos1 - quaternion2 * (new Vector3((index / veinlines.Value) * 8, 0, index % veinlines.Value * areaRadius));
            double del1 = Math.Atan(pos1.z / pos1.x) - Math.Atan(pos2.z / pos2.x);
            double del2 = Math.Acos(pos1.y / radius) - Math.Acos(pos2.y / radius);
            double del3_1 = -Math.Atan(pos3.z / pos3.x) + del1;
            double del3_2 = Math.Acos(pos3.y / radius) - del2;
            if (del1 == double.NaN || del2 == double.NaN || del3_1 == double.NaN || del3_2 == double.NaN)
            {
                return pointpos;
            }
            pos3.x = (float)(end.x < 0 ? -Math.Abs(Math.Sin(del3_2) * Math.Cos(del3_1)) : Math.Abs(Math.Sin(del3_2) * Math.Cos(del3_1)));
            pos3.y = (float)(end.y < 0 ? -Math.Abs(Math.Cos(del3_2)) : Math.Abs(Math.Cos(del3_2)));
            pos3.z = (float)(end.z < 0 ? -Math.Abs(Math.Sin(del3_2) * Math.Sin(del3_1)) : Math.Abs(Math.Sin(del3_2) * Math.Sin(del3_1)));
            pos3.x *= radius;
            pos3.y *= radius;
            pos3.z *= radius;

            if (pos3.x == float.NaN || pos3.y == float.NaN || pos3.z == float.NaN || pos3.y > 190 || pos3.y < -190)
            {
                return pointpos;
            }
            return pos3;
        }
        
        /// <summary>
        /// 将下标翻译为名字/ID
        /// </summary>
        /// <param name="i"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string VeintypechineseTranslate(int i, int type)
        {
            if (type == 0)
            {
                if (i == 1) return "铁矿";
                if (i == 2) return "铜矿";
                if (i == 3) return "硅矿";
                if (i == 4) return "钛矿";
                if (i == 5) return "石矿";
                if (i == 6) return "碳矿";
                if (i == 7) return "原油涌泉";
                if (i == 8) return "可燃冰";
                if (i == 9) return "金伯利矿石";
                if (i == 10) return "分形硅石";
                if (i == 11) return "有机晶体";
                if (i == 12) return "光栅石";
                if (i == 13) return "刺笋结晶";
                if (i == 14) return "单极磁矿";
            }
            else if (type == 1)
            {
                if (i == 1) return "1001";
                if (i == 2) return "1002";
                if (i == 3) return "1003";
                if (i == 4) return "1004";
                if (i == 5) return "1005";
                if (i == 6) return "1006";
                if (i == 7) return "1007";
                if (i == 8) return "1011";
                if (i == 9) return "1012";
                if (i == 10) return "1013";
                if (i == 11) return "1117";
                if (i == 12) return "1014";
                if (i == 13) return "1015";
                if (i == 14) return "1016";
            }

            return "";
        }

        public void RegallTranslate()
        {
            TranslateDict.Clear();
            TranslateDict.Add("启动/关闭", "On/Off");
            TranslateDict.Add("切割出", "Cut out");
            TranslateDict.Add("整理为", "Organized as");
            TranslateDict.Add("个", "veins");
            TranslateDict.Add("行", "lines");
            TranslateDict.Add("合并油井", "Merge Oil");
            TranslateDict.Add("矿脉工具", "Vein Control Tool");
            TranslateDict.Add("不添加", "No Add Vein");
            TranslateDict.Add("ctrl+鼠标左键:移动单矿", "ctrl+LeftMouse：Move Vein");
            TranslateDict.Add("alt+鼠标左键:移动矿堆", "alt+LeftMouse：Move the vein group");
            TranslateDict.Add("shift+鼠标左键:移动所有矿", "shift+LeftMouse：Move all veins");
            TranslateDict.Add("alt+x+鼠标左键:切割矿脉", "alt+x+LeftMouse：Cut the veins from vein group");
            TranslateDict.Add("X:进入拆除模式可以拆除矿脉", "x:Remove vein on Dismantle Mode");
            TranslateDict.Add("~(Esc键下方):合并/不合并", "~:Merge or not merge ");
        }

    }
}
