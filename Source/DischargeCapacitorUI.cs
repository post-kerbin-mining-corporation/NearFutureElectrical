using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    [KSPAddon(KSPAddon.Startup.Flight, false)] 
    public class DischargeCapacitorUI:MonoBehaviour
    {
        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                RenderingManager.AddToPostDrawQueue(0, DrawCapacitorGUI);
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
           // Debug.Log("NFE: Capacitor Manager: Found " + capacitorList.Count() + " capacitors");
        }

        // GUI VARS
        // ---------- 
        public Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        public Vector2 scrollPosition = Vector2.zero;
        static bool showCapWindow = false;
        int windowID = new System.Random(325671).Next();
        bool initStyles = false;



        // styles
        GUIStyle progressBarBG;

        GUIStyle gui_bg;
        GUIStyle gui_text;
        GUIStyle gui_header;
        GUIStyle gui_window;

        GUIStyle gui_btn_shutdown;
        GUIStyle gui_btn_start;
        // Set up the GUI styles
        private void InitStyles()
        {
            gui_window = new GUIStyle(HighLogic.Skin.window);
            gui_header = new GUIStyle(HighLogic.Skin.label);
            gui_header.fontStyle = FontStyle.Bold;
            gui_header.alignment = TextAnchor.UpperLeft;
            gui_header.stretchWidth = true;

            gui_text = new GUIStyle(HighLogic.Skin.label);
            gui_text.fontSize = 11;
            gui_text.alignment = TextAnchor.MiddleLeft;
            gui_bg = new GUIStyle(HighLogic.Skin.textArea);
            gui_bg.active = gui_bg.hover = gui_bg.normal;

            gui_btn_shutdown = new GUIStyle(HighLogic.Skin.button);
            gui_btn_shutdown.wordWrap = true;
            gui_btn_shutdown.normal.textColor = XKCDColors.RedOrange;
            gui_btn_shutdown.alignment = TextAnchor.MiddleCenter;

            gui_btn_start = new GUIStyle(gui_btn_shutdown);
            gui_btn_start.normal.textColor = XKCDColors.Green;

            progressBarBG = new GUIStyle(HighLogic.Skin.textField);
            progressBarBG.active = progressBarBG.hover = progressBarBG.normal;

            windowPos = new Rect(200f, 200f, 550f, 315f);

            initStyles = true;
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
                    gui_window.padding.top = 5;
                    
                    windowPos = GUI.Window(windowID, windowPos, CapacitorWindow, new GUIContent(), gui_window);
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void CapacitorWindow(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Installed Capacitors", gui_header, GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f));
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
                    
                    GUILayout.Label(String.Format("Current total recharge rate: {0:F2}/s",GetAllChargeRatesCurrent()), 
                        gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));

                    GUILayout.FlexibleSpace();

                    GUILayout.Label(String.Format("Current total discharge rate: {0:F2}/s",GetAllDischargeRatesCurrent()), 
                        gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));

                    
                    
                    if (GUILayout.Button("Discharge all"))
                    {
                        DischargeAll();
                    }
                        
                   
                GUILayout.EndHorizontal();
                
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(525f), GUILayout.Height(175f));
                GUILayout.BeginVertical();
                    //windowPos.height = 175f + 70f;
                    foreach (DischargeCapacitor c in capacitorList)
                    {
                        DrawCapacitor(c);
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

       
        private List<DischargeCapacitor> capacitorList;


        private void DrawCapacitor(DischargeCapacitor cap)
        {
            GUILayout.BeginHorizontal(gui_bg);
            // Capacitor Name Field
            GUILayout.Label(cap.part.partInfo.title, gui_header, GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f));
            // Properties
            GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:F0}% Charged", GetChargePercent(cap)), gui_text);
                GUILayout.Label(String.Format("{0:F0} Sc/s",  GetCurrentRate(cap)), gui_text);
            GUILayout.EndVertical();
            // Changeables
            
            GUILayout.BeginVertical();
            // Bar
            GUILayout.BeginHorizontal();
                GUILayout.Label("Customize Discharge Rate", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
                cap.dischargeSlider = GUILayout.HorizontalSlider(cap.dischargeSlider, 50f, 100f, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
                GUILayout.Label(String.Format("Rate: {0:F0} Ec/s", cap.dischargeActual), gui_text);
            GUILayout.EndHorizontal();
            // Buttons
            GUILayout.BeginHorizontal();
                cap.Enabled = GUILayout.Toggle(cap.Enabled, "Recharge Enabled");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Discharge ", GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f)))
                {
                    cap.Discharge();
                }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();

        }
        // Gets the current charge or discharge rate of a capacitor
        private float GetCurrentRate(DischargeCapacitor cap)
        {
            if (cap.Discharging)
            {
                return -cap.DischargeRate;
            } else if (cap.Enabled && cap.CurrentCharge < cap.MaximumCharge)
            {
                return cap.ChargeRate*cap.ChargeRatio;
            } else
            {
                return 0f;
            }
        }
        // Gets a capacitor's percent charge
        private float GetChargePercent(DischargeCapacitor cap)
        {
            return (cap.CurrentCharge / cap.MaximumCharge) *100f;
        }

        private float GetAllChargeRatesCurrent()
        {
            float chargeRate = 0f;
            foreach (DischargeCapacitor cap in capacitorList)
            {
                if (cap.Enabled && cap.CurrentCharge < cap.MaximumCharge)
                    chargeRate = chargeRate + cap.ChargeRate*cap.ChargeRatio;
            }
            return chargeRate;
        }
        private float GetAllDischargeRatesCurrent()
        {
            float dischargeRate = 0f;
            foreach (DischargeCapacitor cap in capacitorList)
            {
                if (cap.Discharging)
                    dischargeRate = dischargeRate + cap.dischargeActual;
            }
            return dischargeRate;
        }
        
        private void DischargeAll()
        {
            foreach (DischargeCapacitor cap in capacitorList)
            {
                cap.Discharge();
            }
        }
        private void ChargeAll()
        {
            foreach (DischargeCapacitor cap in capacitorList)
            {
                cap.Enable();
            }
        }
        private void StopChargeAll()
        {
             foreach (DischargeCapacitor cap in capacitorList)
            {
                cap.Disable();
            }
        }
        

        private float getTotalEc()
        {
            if (FlightGlobals.ActiveVessel.parts.Count == 0)
            {
                return 0f;
            }

            List<PartResource> resources = new List<PartResource>();
            FlightGlobals.ActiveVessel.parts[0].GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id, ResourceFlowMode.ALL_VESSEL, resources);
            float totalEc = 0f;
            foreach (PartResource res in resources)
            {
                totalEc = (float)res.amount + totalEc;
            }
            return totalEc;
        }
        private float getTotalSc()
        {
            if (FlightGlobals.ActiveVessel.parts.Count == 0)
            {
                return 0f;
            }

            List<PartResource> resources = new List<PartResource>();
            FlightGlobals.ActiveVessel.parts[0].GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("StoredCharge").id, ResourceFlowMode.ALL_VESSEL, resources);
            float totalEc = 0f;
            foreach (PartResource res in resources)
            {
                totalEc = (float)res.amount + totalEc;
            }
            return totalEc;
        }
    }
}
