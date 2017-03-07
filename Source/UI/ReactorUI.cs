using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace NearFutureElectrical.UI
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ReactorUI:MonoBehaviour
    {
        // Vessel-related variables
        private Vessel activeVessel;
        private int partCount = 0;

        private List<FissionReactor> reactorList;

        static bool showReactorWindow = false;
        static bool showFocusedWindow = false;
        static FissionReactor focusedReactor;

        private List<ReactorUIItem> uiReactors;

        // GUI VARS
        private int mainWindowID = new System.Random(3256231).Next();
        private int popupWindowID = new System.Random(3256231).Next();
        private Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        private Rect popupWindowPos = new Rect(200f, 200f, 200f, 200f);
        private Vector2 scrollPosition = Vector2.zero;

        private string textVariable;


        bool initStyles = false;
        private UIResources resources;

        // Stock toolbar button
        private static ApplicationLauncherButton stockToolbarButton = null;

        public UIResources GUIResources { get {return resources;}}

        public static void ShowFocusedReactorSettings(FissionReactor reactor)
        {
          showFocusedWindow = true;
          focusedReactor = reactor;
          textVariable = reactor.UIName;
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
            // Create ui objects
            uiReactors = new List<ReactorUIItem>();
            for (FissionReactor reactor in reactorList)
            {
              uiReactors.Add(new ReactorUIItem(reactor, this));
            }

        }

        // Set up the GUI styles
        private void InitStyles()
        {

            resources = new UIResources();
            windowPos = new Rect(200f, 200f, 610f, 315f);

            initStyles = true;
        }
        private void Awake()
        {
            Utils.Log("UI: Awake");
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
        }

        private void Start()
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
                    GUI.skin = HighLogic.Skin;

                    if (showFocusedWindow)
                    {
                      GUI.Window(popupWindowID, windowPos, new GUIContent(), GUIResources.GetStyle("window_main") );
                      GUI.enabled = false;
                    } else
                    {
                      GUI.enabled = true;
                    }

                    windowPos = GUI.Window(mainWindowID, windowPos, ReactorWindow, new GUIContent(), GUIResources.GetStyle("window_main"));
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }

        private void ReactorPopup(int windowId)
        {
          textVariable = GUILayout.TextArea(textVariable, 150);
          GUILayout.BeginHorizontal();
          if (GUILayout.Button("Accept"))
          {
            focusedReactor.UIName = textVariable;
            showFocusedWindow = false;
          }
          if (GUILayout.Button("Cancel"))
          {
            showFocusedWindow = false;
          }
          GUILayout.EndHorizontal();
        }
        // GUI function for the window
        private void ReactorWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Nuclear Reactors", GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(120f));
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
                        for (int i = 0; i <uiReactors.Count; i++)
                        {
                            uiReactors[i].Draw();
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

        private void ReactorOptionsWindow()
        {

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
