using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace NearFutureElectrical
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ReactorUI:MonoBehaviour
    {
        Vessel activeVessel;
        int partCount = 0;

        public void Start()
        {
            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();

            if (HighLogic.LoadedSceneIsFlight)
            {
                //RenderingManager.AddToPostDrawQueue(0, DrawCapacitorGUI);
                FindReactors();
                Utils.LogWarn(windowID.ToString());
            }
        }

        public static void ToggleReactorWindow()
        {

            //Debug.Log("NFT: Toggle Reactor Window");
            showReactorWindow = !showReactorWindow;
        }

        public void FindReactors()
        {
            activeVessel = FlightGlobals.ActiveVessel;
            partCount = activeVessel.parts.Count;

            //Debug.Log("NFE: Capacitor Manager: Finding Capcitors");
            List<FissionReactor> unsortedReactorList = new List<FissionReactor>();
            // Get all parts
            List<Part> allParts = FlightGlobals.ActiveVessel.parts;
            for (int i = 0; i<allParts.Count; i++)
            {
                FissionReactor toAdd = allParts[i].GetComponent<FissionReactor>();
                if (toAdd != null)
                {
                        unsortedReactorList.Add(toAdd);

                }
            }

            //sort
            reactorList = unsortedReactorList.OrderByDescending(x => x.HeatGeneration).ToList();
            reactorList = unsortedReactorList;
           // Debug.Log("NFE: Capacitor Manager: Found " + capacitorList.Count() + " capacitors");
        }

        // GUI VARS
        // ----------
        public Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        public Vector2 scrollPosition = Vector2.zero;
        static bool showReactorWindow = false;
        int windowID = new System.Random(3256231).Next();
        bool initStyles = false;

        // Stock toolbar button
        private static ApplicationLauncherButton stockToolbarButton = null;

        // styles
        GUIStyle progressBarBG;
        GUIStyle progressBarFG;

        GUIStyle gui_bg;
        GUIStyle gui_text;
        GUIStyle gui_header;
        GUIStyle gui_header2;
        GUIStyle gui_toggle;

        GUIStyle gui_window;

        GUIStyle gui_btn_shutdown;
        GUIStyle gui_btn_start;

        Texture notchTexture;

        // Set up the GUI styles
        private void InitStyles()
        {
            gui_window = new GUIStyle(HighLogic.Skin.window);
            gui_header = new GUIStyle(HighLogic.Skin.label);
            gui_header.fontStyle = FontStyle.Bold;
            gui_header.alignment = TextAnchor.UpperLeft;
            gui_header.fontSize = 12;
            gui_header.stretchWidth = true;

            gui_header2 = new GUIStyle(gui_header);
            gui_header2.alignment = TextAnchor.MiddleLeft;

            gui_text = new GUIStyle(HighLogic.Skin.label);
            gui_text.fontSize = 11;
            gui_text.alignment = TextAnchor.MiddleLeft;

            gui_bg = new GUIStyle(HighLogic.Skin.textArea);
            gui_bg.active = gui_bg.hover = gui_bg.normal;

            gui_toggle = new GUIStyle(HighLogic.Skin.toggle);
            gui_toggle.normal.textColor = gui_header.normal.textColor;

            progressBarBG = new GUIStyle(HighLogic.Skin.textField);
            progressBarBG.active = progressBarBG.hover = progressBarBG.normal;

            progressBarFG = new GUIStyle(HighLogic.Skin.button);
            progressBarFG.active = progressBarBG.hover = progressBarBG.normal;
            progressBarFG.border = progressBarBG.border;
            progressBarFG.padding = progressBarBG.padding;

            notchTexture = (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/reactor_ui_notch", false);

            windowPos = new Rect(200f, 200f, 610f, 315f);

            initStyles = true;
        }
        public void Awake()
        {
            Utils.Log("UI: Awake");
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
            }
            DrawReactorGUI();
        }


        private void DrawReactorGUI()
        {
            //Debug.Log("NFE: Start Capacitor UI Draw");
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            if (activeVessel != null)
            {
                if (!initStyles)
                    InitStyles();
                if (reactorList == null)
                    FindReactors();
                if (showReactorWindow)
                {
                    // Debug.Log(windowPos.ToString());
                    GUI.skin = HighLogic.Skin;
                    gui_window.padding.top = 5;

                    windowPos = GUI.Window(windowID, windowPos, ReactorWindow, new GUIContent(), gui_window);
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void ReactorWindow(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Nuclear Reactors", gui_header, GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(120f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.MaxWidth(26f), GUILayout.MinWidth(26f), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f)))
                {
                    ToggleReactorWindow();
                }
            GUILayout.EndHorizontal();

            if (reactorList != null && reactorList.Count > 0)
            {

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MinWidth(600f), GUILayout.MinHeight(271f));
                    GUILayout.BeginVertical();
                        //windowPos.height = 175f + 70f;
                        for (int i = 0; i <reactorList.Count; i++)
                        {
                            DrawReactor(reactorList[i]);
                        }
                   GUILayout.EndVertical();
               GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No nuclear reactors found!");
            }
            GUI.DragWindow();
        }


        private List<FissionReactor> reactorList;


        private void DrawReactor(FissionReactor reactor)
        {
            FissionGenerator gen = reactor.GetComponent<FissionGenerator>();

            GUILayout.BeginHorizontal(gui_bg);
            GUILayout.BeginVertical();
            // Capacitor Name Field
            GUILayout.Label(reactor.part.partInfo.title, gui_header, GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f), GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            bool y = reactor.ModuleIsActive();
            bool x = GUILayout.Toggle(reactor.ModuleIsActive(),"Active",gui_toggle);
            if (x != y)
            {
                reactor.ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            // Changeables

            GUILayout.BeginVertical();

            //GUILayout.Label("Core Temperature", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Reactor Status", gui_header2, GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
            if (reactor.FollowThrottle)
            {
                GUILayout.Label(String.Format("Actual: {0:F0}%", reactor.ActualPowerPercent), gui_text);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            reactor.CurrentPowerPercent = GUILayout.HorizontalSlider(reactor.CurrentPowerPercent, 0f, 100f, GUILayout.MinWidth(150f));
            GUILayout.Label(String.Format("{0:F0}%", reactor.CurrentPowerPercent), gui_text);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("Heat Output: {0:F1} kW", reactor.AvailablePower), gui_text, GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
            if (gen != null)
            {
                GUILayout.Label(String.Format("Power Output: {0:F1} Ec/s", gen.CurrentGeneration), gui_text, GUILayout.MaxWidth(140f), GUILayout.MinWidth(140f));
            }
            else
            {
                GUILayout.Label(String.Format("Power Output: No Generator"), gui_text, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Core Status", gui_header2, GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
            float coreTemp = (float)reactor.GetCoreTemperature();
            float meltdownTemp = reactor.MaximumTemperature;
            float nominalTemp = reactor.NominalTemperature;
            float criticalTemp = reactor.CriticalTemperature;

            float tempAreaWidth = 250f;
            float tempBarWidth = 210f;
            Rect tempArea = GUILayoutUtility.GetRect(tempAreaWidth, 60f);
            Rect barArea = new Rect(20f, 20f, tempBarWidth, 40f);

            float nominalXLoc = nominalTemp / meltdownTemp * tempBarWidth;
            float criticalXLoc = criticalTemp / meltdownTemp * tempBarWidth;
            float tempBarFGSize = tempBarWidth*(coreTemp/meltdownTemp);

            Color barColor = new Color();
            if (coreTemp <= nominalTemp)
                barColor = Color.green;
            else if (coreTemp <= criticalTemp)
                barColor = Color.Lerp(Color.green, new Color(1.0f, 102f / 255f, 0f), (coreTemp - nominalTemp) / (criticalTemp - nominalTemp));
            else
                barColor = Color.Lerp(new Color(1.0f, 102f / 255f, 0f), Color.red, (coreTemp - criticalTemp) / (meltdownTemp - criticalTemp));

                GUI.BeginGroup(tempArea);
                GUI.Box(new Rect(0f,10f,tempBarWidth,10f),"", progressBarBG);
                GUI.color = barColor;
                GUI.Box(new Rect(0f, 11f, tempBarFGSize, 7f), "", progressBarFG);
                GUI.color = Color.white;
                GUI.Label(new Rect(tempBarWidth+7f, 8f, 40f, 20f), String.Format("{0:F0} K", coreTemp), gui_text);

                GUI.Label(new Rect(0f + nominalXLoc - 13f, 16f, 22f, 25f), notchTexture);
                GUI.Label(new Rect(0f + criticalXLoc - 13f, 16f, 22f, 25f), notchTexture);

                GUI.Label(new Rect(nominalXLoc - 46f, 25f, 40f, 20f), String.Format("{0:F0} K", nominalTemp), gui_text);
                GUI.Label(new Rect(9f + criticalXLoc, 25f, 40f, 20f), String.Format("{0:F0} K", criticalTemp), gui_text);

               // GUI.Label(new Rect(20f+tempBarWidth, 30f, 40f, 20f), String.Format("{0:F0} K", meltdownTemp), gui_text);
             GUI.EndGroup();
             GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Safety Status", gui_header2, GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
            reactor.CurrentSafetyOverride = GUILayout.HorizontalSlider(reactor.CurrentSafetyOverride, 700f, 6000f, GUILayout.MinWidth(150f));
            GUILayout.Label(String.Format("{0:F0} K", reactor.CurrentSafetyOverride), gui_text);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Label("Fuel Status", gui_header2, GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
                GUILayout.Label(reactor.FuelStatus, gui_text);
            GUILayout.EndHorizontal();


            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

        }

        void Update()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                if (activeVessel != null)
                {
                    if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
                    {
                        ResetAppLauncher();
                    }
                }
                else
                {
                    ResetAppLauncher();
                }

            }
            if (activeVessel != null)
            {
                if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
                {
                    ResetAppLauncher();

                }
            }
        }

        void ResetAppLauncher()
        {
            FindReactors();
            if (stockToolbarButton == null)
            {
                if (reactorList.Count > 0)
                {
                    stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarButtonToggle,
                    OnToolbarButtonToggle,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/reactor_toolbar_off", false));
                }
                else
                {
                }
            }
            else
            {
                if (reactorList.Count > 0)
                {
                }
                else
                {
                    showReactorWindow = false;
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                }
            }

        }

         // Stock toolbar handling methods
        public void OnDestroy()
        {

            // Remove the stock toolbar button
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (stockToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
            }

        }

        private void OnToolbarButtonToggle()
        {
            ToggleReactorWindow();
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showReactorWindow ? "NearFutureElectrical/UI/reactor_toolbar_on" : "NearFutureElectrical/UI/reactor_toolbar_off", false));
        }


        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && stockToolbarButton == null && reactorList.Count > 0)
            {
                stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarButtonToggle,
                    OnToolbarButtonToggle,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/reactor_toolbar_off", false));
            }
        }

        void OnGUIAppLauncherDestroyed()
        {
            if (stockToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                stockToolbarButton = null;
            }
        }

        void onAppLaunchToggleOff()
        {
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/reactor_toolbar_off", false));
        }

        void DummyVoid() { }
    }


}
