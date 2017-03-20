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

        private List<ReactorUIEntry> uiReactors;

        // GUI VARS
        private int mainWindowID = new System.Random(3256231).Next();
        private int popupWindowID = new System.Random(3256251).Next();
        private Rect windowPos = new Rect(200f, 200f, 500f, 200f);
        private Rect popupWindowPos = new Rect(200f, 200f, 200f, 180f);
        private Vector2 scrollPosition = Vector2.zero;

        private int iconID;
        private static string textVariable;

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
            if (!showReactorWindow)
              showFocusedWindow = false;
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
            uiReactors = new List<ReactorUIEntry>();
            foreach (FissionReactor reactor in reactorList)
            {
              uiReactors.Add(new ReactorUIEntry(reactor, this));
            }

        }

        // Set up the GUI styles
        private void InitStyles()
        {

            resources = new UIResources();
            windowPos = new Rect(200f, 200f, 715f, 315f);

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
                //Utils.LogWarn(indowID.ToString());
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
                      GUILayout.Window(popupWindowID, popupWindowPos, ReactorPopup, new GUIContent(), GUIResources.GetStyle("window_main") );

                    } else
                    {
                    }

                    windowPos = GUILayout.Window(mainWindowID, windowPos, ReactorWindow, new GUIContent(), GUIResources.GetStyle("window_main"), GUILayout.MinHeight(150f), GUILayout.MaxHeight(315f));
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }

        // Draws the popup windows
        private void ReactorPopup(int windowId)
        {
          GUILayout.Label("Customize Reactor", GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(21f), GUILayout.MinHeight(21f), GUILayout.MinWidth(150f));
          Rect windowRect = GUILayoutUtility.GetRect(200f,180f);

          Rect iconRect = new Rect(0f, 16f, 64f, 64f);

          Rect chooserAreaRect = new Rect(72f, 0f, 90f, 90f);
          Rect textAreaRect = new Rect(0f, 93f, 180f, 30f);

          Rect acceptButtonRect = new Rect(130f, 125f, 24f, 24f);
          Rect cancelButtonRect = new Rect(160f, 125f, 24f, 24f);

          Texture sharedIcon = GUIResources.GetIcon("reactor_1").iconAtlas;

          GUI.BeginGroup(windowRect);

          // Big icon
          GUI.DrawTextureWithTexCoords(iconRect, sharedIcon, GUIResources.GetReactorIcon(iconID).iconRect);

          // Text Area
          textVariable = GUI.TextArea(textAreaRect, textVariable, 150, GUIResources.GetStyle("text_area"));

          // Select icon area
          GUI.BeginGroup(chooserAreaRect);
          DrawChoiceIcon(0, sharedIcon, 0, 0);
          DrawChoiceIcon(1, sharedIcon, 1, 0);
          DrawChoiceIcon(2, sharedIcon, 2, 0);
          DrawChoiceIcon(3, sharedIcon, 0, 1);
          DrawChoiceIcon(4, sharedIcon, 1, 1);
          DrawChoiceIcon(5, sharedIcon, 2, 1);
          DrawChoiceIcon(6, sharedIcon, 0, 2);
          DrawChoiceIcon(7, sharedIcon, 1, 2);
          DrawChoiceIcon(8, sharedIcon, 2, 2);
          GUI.EndGroup();

          // Cancel/Accept
          GUI.color = resources.GetColor("accept_color");
          if (GUI.Button(acceptButtonRect, "", GUIResources.GetStyle("button_accept")))
          {
            focusedReactor.UIName = textVariable;
            focusedReactor.UIIcon = iconID;
            showFocusedWindow = false;
          }
          GUI.DrawTextureWithTexCoords(acceptButtonRect, GUIResources.GetIcon("accept").iconAtlas, GUIResources.GetIcon("accept").iconRect);
          GUI.color = resources.GetColor("cancel_color");
          if (GUI.Button(cancelButtonRect, "", GUIResources.GetStyle("button_cancel")))
          {
            showFocusedWindow = false;
          }
          GUI.DrawTextureWithTexCoords(cancelButtonRect, GUIResources.GetIcon("cancel").iconAtlas, GUIResources.GetIcon("cancel").iconRect);
          GUI.color = Color.white;

          GUI.EndGroup();
        }

        private void DrawChoiceIcon(int id, Texture texture, int x_id, int y_id)
        {
            Rect iconRect = new Rect(x_id * 34f +2, y_id * 34f +2, 28f, 28f);
            Rect buttonRect = new Rect(x_id * 34f, y_id * 34f, 32f, 32f);

          if (GUI.Button(buttonRect, "", GUIResources.GetStyle("button_overlaid")))
            iconID = id;
          GUI.DrawTextureWithTexCoords(iconRect, texture, GUIResources.GetReactorIcon(id).iconRect);
        }

        private void DrawHeaderArea()
        {
          GUILayout.BeginHorizontal();
          GUILayout.Label("Reactor Control Panel (Near Future Electrical v0.8.7)", GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(350f));
              GUILayout.FlexibleSpace();
              Rect buttonRect = GUILayoutUtility.GetRect(22f, 22f);
              GUI.color = resources.GetColor("cancel_color");
              if (GUI.Button(buttonRect, "", GUIResources.GetStyle("button_cancel")))
              {
                  ToggleReactorWindow();
              }

              GUI.DrawTextureWithTexCoords(buttonRect, GUIResources.GetIcon("cancel").iconAtlas, GUIResources.GetIcon("cancel").iconRect);
              GUI.color = Color.white;
          GUILayout.EndHorizontal();
        }

        // Draws the main window
        private void ReactorWindow(int windowId)
        {
            DrawHeaderArea();

            if (reactorList != null && reactorList.Count > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MinWidth(700f), GUILayout.MinHeight(251f));
                GUILayout.Space(3f);
                GUILayout.BeginVertical();
                //windowPos.height = 175f + 70f;
                for (int i = 0; i < uiReactors.Count; i++)
                {
                    uiReactors[i].Draw();
                }
                GUILayout.EndVertical();
               GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No nuclear reactors found");
            }
            GUI.DragWindow();
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
