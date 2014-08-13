using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    [KSPAddon(KSPAddon.Startup.Flight, false)] 
    public class FissionGeneratorUI:MonoBehaviour
    {
        public void Start()
        {
            RenderingManager.AddToPostDrawQueue(0, DrawGUI);
            FindReactors();
        }

        public static void ToggleWindow()
        {
            //Debug.Log("NFT: Toggle Reactor Window");
            showWindow = !showWindow;
        }

        // Finds all reactors on ship
        public void FindReactors()
        {
            //Debug.Log("NFT: Reactor UI: Finding reactors");
            reactorList = new List<FissionGenerator>();
            // Get all parts
            List<Part> allParts = FlightGlobals.ActiveVessel.parts;
            foreach (Part pt in allParts)
            {
                PartModuleList modules = pt.Modules;
                for (int i = 0; i < modules.Count; i++)
                {
                    PartModule curModule = modules.GetModule(i);
                    if (curModule.ClassName == "FissionGenerator")
                    {
                        reactorList.Add(curModule.GetComponent<FissionGenerator>());
                    }
                }
            }
            //Debug.Log("NFT: Reactor UI: Found " + reactorList.Count() + " reactors");
        }

        // GUI VARS
        // ---------- 

        static bool showWindow = false;

        public Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        public Vector2 scrollPosition = Vector2.zero;

        int windowID = new System.Random().Next();
        bool initStyles = false;

        private List<FissionGenerator> reactorList;

        // styles
        GUIStyle progressBarBG;

        GUIStyle gui_bg;
        GUIStyle gui_text;
        GUIStyle gui_header;
        GUIStyle gui_window;


        // Set up the GUI styles
        private void InitStyles()
        {
            gui_window = new GUIStyle(HighLogic.Skin.window);
            gui_header = new GUIStyle(HighLogic.Skin.label);
            gui_header.fontStyle = FontStyle.Bold;
            gui_header.alignment = TextAnchor.MiddleLeft;
            gui_header.stretchWidth = true;

            gui_text = new GUIStyle(HighLogic.Skin.label);
            gui_text.fontSize = 11;
            gui_text.alignment = TextAnchor.MiddleLeft;
            gui_bg = new GUIStyle(HighLogic.Skin.textArea);
            gui_bg.active = gui_bg.hover = gui_bg.normal;

            progressBarBG = new GUIStyle(HighLogic.Skin.textField);
            progressBarBG.active = progressBarBG.hover = progressBarBG.normal;

            windowPos = new Rect(200f, 200f, 550f, 200f);

            initStyles = true;
        }

       
        // Draw the GUI
        protected void DrawGUI()
        {
            Vessel activeVessel = FlightGlobals.ActiveVessel;
            
            if (activeVessel != null)
            {
                //Debug.Log("NFPP: Start Reactor UI Draw");
                if (!initStyles)
                    InitStyles();
                if (reactorList == null)
                    FindReactors();

                if (showWindow)
                {
                    // Debug.Log(windowPos.ToString());

                    windowPos = GUI.Window(windowID, windowPos, Window, "Near Future Technology Fission Reactor Control Panel", gui_window);
                }
               // Debug.Log("NFPP: Stop Reactor UI Draw");
            }
         
        }

        // GUI function for the window
        private void Window(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            if (reactorList != null && reactorList.Count > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(500f), GUILayout.Height(175f));
                GUILayout.BeginVertical();

                windowPos.height = 175f + 70f;
                foreach (FissionGenerator gen in reactorList)
                {
                    DrawReactor(gen);
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.MaxWidth(32f), GUILayout.MinWidth(32f), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f)))
                {
                    ToggleWindow();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No reactors found!");
            }
            GUI.DragWindow();
        }

        private void DrawReactor(FissionGenerator gen)
        {

            GUI.enabled = true;

            GUILayout.BeginVertical(gui_bg);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Resources.gui_show, GUILayout.MaxWidth(32f), GUILayout.MinWidth(32f), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f)))
            {
                gen.ToggleHighlight();

            }

            GUILayout.Label(gen.part.partInfo.title, gui_header, GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MinWidth(150f), GUILayout.MaxWidth(150f));

            gen.Enabled = GUILayout.Toggle(gen.Enabled, "Reactor Enabled");
            gen.SafetyLimit = GUILayout.Toggle(gen.SafetyLimit, "Safety Limit");

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.MinWidth(250f), GUILayout.MaxWidth(250f));
            GUILayout.Label("Reactor Power", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
            gen.CurrentPowerPercent = GUILayout.HorizontalSlider(gen.CurrentPowerPercent, gen.MinPowerPercent, 1.0f, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
            GUILayout.Label(String.Format("{0:F0}%", gen.CurrentPowerPercent * 100f), gui_text);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.MinWidth(250f), GUILayout.MaxWidth(250f));
            GUILayout.Label("Thermal Power", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
            GUILayout.Box("", HighLogic.Skin.horizontalSlider, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
            Rect last = GUILayoutUtility.GetLastRect();
            last.xMin = last.xMin + last.width * 0.05f;
            last.width = last.width * 0.9f * (gen.currentThermalPower / gen.ThermalPower);
            last.yMin = last.yMin + last.height * 0.05f;
            last.height = last.height * 0.8f;
            GUI.color = XKCDColors.Orangeish;
            GUI.DrawTexture(last, Resources.gui_progressbar);
            GUI.color = Color.white;
            GUILayout.Label(String.Format("{0:F0} kW", gen.currentThermalPower), gui_text);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.MinWidth(250f), GUILayout.MaxWidth(250f));
            GUILayout.Label("Core Temperature", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
            GUILayout.Box("", HighLogic.Skin.horizontalSlider, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
            last = GUILayoutUtility.GetLastRect();
            last.xMin = last.xMin + last.width * 0.05f;
            last.width = last.width * 0.9f * (gen.CurrentCoreTemperature / gen.MeltdownCoreTemperature);
            last.yMin = last.yMin + last.height * 0.05f;
            last.height = last.height * 0.8f;
            GUI.color = Color.Lerp(XKCDColors.Green, XKCDColors.RedOrange, gen.CurrentCoreTemperature / gen.MeltdownCoreTemperature);
            GUI.DrawTexture(last, Resources.gui_progressbar);
            GUI.color = Color.white;
            GUILayout.Label(String.Format("{0:F0} K", gen.CurrentCoreTemperature), gui_text);


            GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Core Lifetime (Current): " + gen.FindTimeRemaining(gen.BurnRate * gen.CoreTemperatureRatio), gui_text);
            GUILayout.Label("Core Lifetime (Full Power): " + gen.FindTimeRemaining(gen.BurnRate), gui_text);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }
    }
}
