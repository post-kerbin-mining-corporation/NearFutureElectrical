using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using NearFutureElectrical;
using KSP.Localization;

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
        public Rect windowPos = new Rect(200f, 200f, 500f, 100f);
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

                    windowPos = GUILayout.Window(windowID, windowPos, CapacitorWindow, new GUIContent(), resources.GetStyle("window_main"), GUILayout.MinHeight(120f), GUILayout.MaxHeight(315f), GUILayout.ExpandHeight(true));
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void CapacitorWindow(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            DrawHeaderArea();

            if (capacitorList != null && capacitorList.Count > 0)
            {

                DrawGlobalControls();
                float scrollHeight = Mathf.Min(capacitorList.Count * 68f, 6f * 68f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(480f), GUILayout.Height(scrollHeight));
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
                GUILayout.Label(Localizer.Format("#LOC_NFElectrical_CapacitorUI_NotInstalled"));
            }
            GUI.DragWindow();
        }

        private void DrawHeaderArea()
        {
          GUILayout.BeginHorizontal();
          GUILayout.Label(String.Format("{0} ({1})", 
              Localizer.Format("#LOC_NFElectrical_CapacitorUI_Title"),
              Localizer.Format("#LOC_NFElectrical_Version")), GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(350f));
          GUILayout.FlexibleSpace();
          Rect buttonRect = GUILayoutUtility.GetRect(22f, 22f);
          GUI.color = resources.GetColor("cancel_color");
          if (GUI.Button(buttonRect, "", GUIResources.GetStyle("button_cancel")))
          {
              ToggleCapWindow();
          }
          GUI.color = Color.white;
          GUI.DrawTextureWithTexCoords(buttonRect, GUIResources.GetIcon("cancel").iconAtlas, GUIResources.GetIcon("cancel").iconRect);

          GUILayout.EndHorizontal();
        }
        private void DrawGlobalControls()
        {
          GUILayout.BeginHorizontal();
          Rect controlRect = GUILayoutUtility.GetRect(300f, 60f);
          Rect dischargeButtonRect = new Rect (0f, 0f, 48f, 48f);
          Rect chargeAllOnButtonRect = new Rect (56f, 0f, 22f, 22f);
          Rect chargeAllOffButtonRect = new Rect (56f, 24f, 22f, 22f);

          Rect currentRechargeRateRect = new Rect (90f, 2f, 180f, 20f);
          Rect currentDischargeRateRect = new Rect (90f, 26f, 180f, 20f);


          GUI.BeginGroup(controlRect);
          GUI.color = GUIResources.GetColor("capacitor_blue");
          if (GUI.Button(dischargeButtonRect, ""))
          {
            DischargeAll();
          }
          GUI.DrawTextureWithTexCoords(dischargeButtonRect, GUIResources.GetIcon("capacitor_discharge").iconAtlas, GUIResources.GetIcon("capacitor_discharge").iconRect);
          GUI.color = GUIResources.GetColor("accept_color");
          if (GUI.Button(chargeAllOnButtonRect, ""))
          {
            ChargeAll();
          }
          GUI.DrawTextureWithTexCoords(chargeAllOnButtonRect, GUIResources.GetIcon("capacitor_charging").iconAtlas, GUIResources.GetIcon("capacitor_charging").iconRect);
          GUI.color = GUIResources.GetColor("cancel_color");
          if (GUI.Button(chargeAllOffButtonRect, ""))
          {
            StopChargeAll();
          }
          GUI.DrawTextureWithTexCoords(chargeAllOffButtonRect, GUIResources.GetIcon("capacitor_charging").iconAtlas, GUIResources.GetIcon("capacitor_charging").iconRect);

          GUI.color = Color.white;

          GUI.Label(currentRechargeRateRect, String.Format("{0} {1:F2} EC/s", Localizer.Format("#LOC_NFElectrical_CapacitorUI_SummaryRecharge"), GetAllChargeRatesCurrent()),
              resources.GetStyle("header_basic"));
          GUI.Label(currentDischargeRateRect, String.Format("{0} {1:F2}/s", Localizer.Format("#LOC_NFElectrical_CapacitorUI_SummaryDischarge"), GetAllDischargeRatesCurrent()),
              resources.GetStyle("header_basic"));


          GUI.EndGroup();
          GUILayout.EndHorizontal();
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
