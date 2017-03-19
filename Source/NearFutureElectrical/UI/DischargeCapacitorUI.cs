using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using NearFutureElectrical;

namespace NearFutureElectrical.UI
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DischargeCapacitorUI:MonoBehaviour
    {
        Vessel activeVessel;
        int partCount = 0;

        private List<CapacitorUIEntry> uiCapacitors;

        private List<DischargeCapacitor> capacitorList;


        private UIResources resources;
        public UIResources GUIResources { get { return resources; } }

        public void Start()
        {
            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();
            if (HighLogic.LoadedSceneIsFlight)
            {
                //RenderingManager.AddToPostDrawQueue(0, DrawCapacitorGUI);
                FindCapacitors();
                Utils.LogWarn(windowID.ToString());
            }
        }

        public static void ToggleCapWindow()
        {

            //Debug.Log("NFT: Toggle Reactor Window");
            showCapWindow = !showCapWindow;
        }

        public void FindCapacitors()
        {
            activeVessel = FlightGlobals.ActiveVessel;
            partCount = activeVessel.parts.Count;
            //Debug.Log("NFE: Capacitor Manager: Finding Capcitors");
            List<DischargeCapacitor> unsortedCapacitorList = new List<DischargeCapacitor>();
            // Get all parts
            List<Part> allParts = FlightGlobals.ActiveVessel.parts;
            foreach (Part pt in allParts)
            {
                PartModuleList modules = pt.Modules;
                for (int i = 0; i < modules.Count; i++)
                {
                    PartModule curModule = modules.GetModule(i);
                    if (curModule.ClassName == "DischargeCapacitor")
                    {
                        unsortedCapacitorList.Add(curModule.GetComponent<DischargeCapacitor>());
                    }
                }
            }

            //sort
            capacitorList = unsortedCapacitorList.OrderByDescending(x => x.dischargeActual).ToList();
            capacitorList = unsortedCapacitorList;

            uiCapacitors= new List<CapacitorUIEntry>();
            foreach (DischargeCapacitor capacitor in capacitorList)
            {
              uiCapacitors.Add(new CapacitorUIEntry(capacitor, this));
            }
           // Debug.Log("NFE: Capacitor Manager: Found " + capacitorList.Count() + " capacitors");
        }

        // GUI VARS
        // ----------
        public Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        public Vector2 scrollPosition = Vector2.zero;
        static bool showCapWindow = false;
        int windowID = new System.Random(325671).Next();
        bool initStyles = false;

        // Stock toolbar button
        private static ApplicationLauncherButton stockToolbarButton = null;


        // Set up the GUI styles
        private void InitStyles()
        {
            resources = new UIResources();
            windowPos = new Rect(200f, 200f, 550f, 315f);
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
            DrawCapacitorGUI();
        }


        private void DrawCapacitorGUI()
        {
            //Debug.Log("NFE: Start Capacitor UI Draw");
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            if (activeVessel != null)
            {
                if (!initStyles)
                    InitStyles();
                if (capacitorList == null)
                    FindCapacitors();
                if (showCapWindow)
                {
                    // Debug.Log(windowPos.ToString());
                    GUI.skin = HighLogic.Skin;
                    //gui_window.padding.top = 5;

                    windowPos = GUI.Window(windowID, windowPos, CapacitorWindow, new GUIContent(), resources.GetStyle("window_main"));
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void CapacitorWindow(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Capacitors", resources.GetStyle("header_basic"), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f), GUILayout.MinWidth(120f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.MaxWidth(26f), GUILayout.MinWidth(26f), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f)))
            {
                ToggleCapWindow();
            }
            GUILayout.EndHorizontal();

            if (capacitorList != null && capacitorList.Count > 0)
            {

                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                        if (GUILayout.Button("Enable all charging"))
                        {
                            ChargeAll();
                        }
                        if (GUILayout.Button("Disable all charging"))
                        {
                            StopChargeAll();
                        }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label(String.Format("Current total recharge rate: {0:F2}/s",GetAllChargeRatesCurrent()),
                        resources.GetStyle("text_basic"), GUILayout.MaxWidth(950f), GUILayout.MinWidth(190f));



                    GUILayout.Label(String.Format("Current total discharge rate: {0:F2}/s",GetAllDischargeRatesCurrent()),
                        resources.GetStyle("text_basic"), GUILayout.MaxWidth(190f), GUILayout.MinWidth(190f));
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Discharge all"))
                    {
                        DischargeAll();
                    }


                GUILayout.EndHorizontal();

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(525f), GUILayout.Height(175f));
                GUILayout.BeginVertical();
                    //windowPos.height = 175f + 70f;
                    for (int i = 0; i < uiCapacitors.Count; i++)
                    {
                      uiCapacitors[i].Draw();
                    }

               GUILayout.EndVertical();
               GUILayout.EndScrollView();



            }
            else
            {
                GUILayout.Label("No capacitors found!");
            }
            GUI.DragWindow();
        }

        private float GetAllChargeRatesCurrent()
        {
            float chargeRate = 0f;
            for(int i = 0; i <capacitorList.Count; i++)
            {
                if (capacitorList[i].Enabled && capacitorList[i].CurrentCharge < capacitorList[i].MaximumCharge)
                    chargeRate = chargeRate + capacitorList[i].ChargeRate*capacitorList[i].ChargeRatio;
            }
            return chargeRate;
        }
        private float GetAllDischargeRatesCurrent()
        {
            float dischargeRate = 0f;
            for(int i = 0; i <capacitorList.Count; i++)
            {
                if (capacitorList[i].Discharging)
                    dischargeRate = dischargeRate + capacitorList[i].dischargeActual;
            }
            return dischargeRate;
        }

        private void DischargeAll()
        {
            for(int i = 0; i <capacitorList.Count; i++)
            {
                capacitorList[i].Discharge();
            }
        }
        private void ChargeAll()
        {
            for(int i = 0; i <capacitorList.Count; i++)
            {
                capacitorList[i].Enable();
            }
        }
        private void StopChargeAll()
        {
             for(int i = 0; i <capacitorList.Count; i++)
            {
                capacitorList[i].Disable();
            }
        }


        private float getTotalEc()
        {
            if (FlightGlobals.ActiveVessel.parts.Count == 0)
            {
                return 0f;
            }

            double ec = 0;
            double maxEc = 0;
            FlightGlobals.ActiveVessel.resourcePartSet.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id, out ec, out maxEc, true);
            return (float)ec;
        }
        private float getTotalSc()
        {
            if (FlightGlobals.ActiveVessel.parts.Count == 0)
            {
                return 0f;
            }

            double sc = 0;
            double maxSc = 0;
            FlightGlobals.ActiveVessel.resourcePartSet.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition("StoredCharge").id, out sc, out maxSc, true);
            return (float)sc;
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
            FindCapacitors();
            if (stockToolbarButton == null)
            {
                if (capacitorList.Count > 0)
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
                if (capacitorList.Count > 0)
                {
                }
                else
                {
                    showCapWindow = false;
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
            ToggleCapWindow();
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showCapWindow ? "NearFutureElectrical/UI/cap_toolbar_on" : "NearFutureElectrical/UI/cap_toolbar_off", false));

        }


        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && stockToolbarButton == null && capacitorList.Count > 0)
            {
                stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarButtonToggle,
                    OnToolbarButtonToggle,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/cap_toolbar_off", false));
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
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/cap_toolbar_off", false));
        }

        void DummyVoid() { }
    }
}
