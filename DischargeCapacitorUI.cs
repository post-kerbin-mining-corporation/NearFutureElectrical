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
                RenderingManager.AddToPostDrawQueue(0, DrawGUI);
                FindCapacitors();
                lastResources = getTotalEc();
            }
        }

        public static void ToggleWindow()
        {
            //Debug.Log("NFT: Toggle Reactor Window");
            showWindow = !showWindow;
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
        static bool showWindow = false;
        int windowID = new System.Random().Next();
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
            gui_header.alignment = TextAnchor.MiddleLeft;
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

            windowPos = new Rect(200f, 200f, 375f, 135f);

            initStyles = true;
        }

        private void DrawGUI()
        {
            //Debug.Log("NFE: Start Capacitor UI Draw");
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            if (activeVessel != null)
            {
                if (!initStyles)
                    InitStyles();
                if (capacitorList == null)
                    FindCapacitors();
                if (showWindow)
                {

                    windowPos = GUI.Window(windowID, windowPos, Window, "Near Future Technology Capacitor Control Panel", gui_window);
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void Window(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            if (capacitorList != null && capacitorList.Count > 0)
            {

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Enable Capacitor Recharge"))
                {
                    foreach (DischargeCapacitor cap in capacitorList)
                    {
                        cap.Enable();
                    }
                }
                if (GUILayout.Button("Disable Capacitor Recharge"))
                {
                    foreach (DischargeCapacitor cap in capacitorList)
                    {
                        cap.Disable();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                automate = GUILayout.Toggle(automate, "Capacitor Automation", GUILayout.MinWidth(175f), GUILayout.MaxWidth(175f));
                GUILayout.Label(String.Format("Total Stored Charge: {0:F0}", getTotalSc()), gui_text);
                    
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                
                    GUILayout.Label("Discharge Threshold", gui_text, GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
                    threshold = GUILayout.HorizontalSlider(threshold, 1f, 50f, GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
                    GUILayout.Label(String.Format("{0:F0} Ec/s", threshold), gui_text);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X", GUILayout.MaxWidth(32f), GUILayout.MinWidth(32f), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f)))
                    {
                        ToggleWindow();
                    }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No capacitors found!");
            }
            GUI.DragWindow();
        }

        private bool automate = false;
        private List<DischargeCapacitor> capacitorList;

        int frameCounter = 0;
        private float lastResources = 999999999f;

        private float threshold = 10f;

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (automate && frameCounter > 10)
                {
                    frameCounter = 0;
                    float totalEc = getTotalEc();

                    float delta = lastResources - totalEc;

                    //Debug.Log("total: " + totalEc.ToString() + ". lastFrame: " + lastResources.ToString() + ". delta:" + delta.ToString());
                    if (delta > threshold * TimeWarp.fixedDeltaTime)
                    {
                        float chargePerSecRequired = delta * (1f / TimeWarp.fixedDeltaTime);
                        float chargeAdded = 0f;
                        bool enoughChargeAdded = false;

                        foreach (DischargeCapacitor cap in capacitorList)
                        {
                            if (!enoughChargeAdded)
                            {
                                //Debug.Log("status = " + cap.Discharging.ToString() + ", " + cap.Enabled.ToString() + ", " + cap.CurrentCharge.ToString());
                               // Debug.Log("status = " + (!cap.Discharging && cap.CurrentCharge >= cap.MaximumCharge * 0.5f).ToString());
                                // capacitor cannot be already discharging, recharging or have a low amount of charge
                                if (cap.CurrentCharge >= cap.MaximumCharge * 0.5f && !cap.Discharging)
                                {
                                    chargeAdded = chargeAdded + cap.dischargeActual;
                                    //Debug.Log("Discharged ");
                                    cap.Discharge();
                                    if (chargeAdded >= chargePerSecRequired)
                                    {
                                        enoughChargeAdded = true;
                                    }


                                }

                            }
                        }
                    }

                }

                lastResources = getTotalEc();
                frameCounter = frameCounter + 1;
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
