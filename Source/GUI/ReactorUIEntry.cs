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
    public class ReactorUIEntry
    {

      private bool advancedMode = false;

      private ReactorUI host;
      private FissionReactor reactor;
      private FissionGenerator generator;

      // initialize the UI component
      public ReactorUIEntry(FissionReactor toDraw, ReactorUI host)
      {
        reactor = toDraw;
        generator = reactor.GetComponent<FissionGenerator>();
      }

      private void DrawAdvancedControls()
      {
        // Auto-shutdown slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Safety Status",  host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
        reactor.CurrentSafetyOverride = GUILayout.HorizontalSlider(reactor.CurrentSafetyOverride, 700f, reactor.MaximumTemperature, GUILayout.MinWidth(150f));
        GUILayout.Label(String.Format("{0:F0} K", reactor.CurrentSafetyOverride), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
            GUILayout.Label("Fuel Status",  host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
            GUILayout.Label(reactor.FuelStatus, host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();

        // Time warp cutoff slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Safety Status",  host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
        reactor.CurrentSafetyOverride = GUILayout.HorizontalSlider(reactor.CurrentSafetyOverride, 700f, reactor.MaximumTemperature, GUILayout.MinWidth(150f));
        GUILayout.Label(String.Format("{0:F0} K", reactor.CurrentSafetyOverride), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
            GUILayout.Label("Fuel Status",  host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
            GUILayout.Label(reactor.FuelStatus, host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Reactor Status", host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
        if (reactor.FollowThrottle)
        {
            GUILayout.Label(String.Format("Actual: {0:F0}%", reactor.ActualPowerPercent), host.GUIResources.GetStyle("text_basic"));
        }
        GUILayout.EndVertical();
      }

      private void DrawBasicControls()
      {

        // Power production
        GUILayout.BeginHorizontal();
        GUILayout.Label(String.Format("Heat Output: {0:F1} kW", reactor.AvailablePower), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
        if (generator != null)
        {
            GUILayout.Label(String.Format("Power Output: {0:F1} Ec/s", generator.CurrentGeneration), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(140f), GUILayout.MinWidth(140f));
        }
        else
        {
            GUILayout.Label(String.Format("Power Output: No Generator"), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
        }
        GUILayout.EndHorizontal();
        // Lifetime remaining

        // Temperature bar
        GUILayout.BeginHorizontal();
        GUILayout.Label("Core Status", host.GUIResources.GetStyle("header_center"), GUILayout.MaxWidth(110f), GUILayout.MinWidth(110f));
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
            GUI.Box(new Rect(0f,10f,tempBarWidth,10f),"", host.GUIResources.GetStyle("bar_background"));
            GUI.color = barColor;
            GUI.Box(new Rect(0f, 11f, tempBarFGSize, 7f), "", ost.GUIResources.GetStyle("bar_foreround"));
            GUI.color = Color.white;
            GUI.Label(new Rect(tempBarWidth+7f, 8f, 40f, 20f), String.Format("{0:F0} K", coreTemp), host.GUIResources.GetStyle("text_basic"));

            //GUI.Label(new Rect(0f + nominalXLoc - 13f, 16f, 22f, 25f), notchTexture);
            //GUI.Label(new Rect(0f + criticalXLoc - 13f, 16f, 22f, 25f), notchTexture);

            GUI.Label(new Rect(nominalXLoc - 46f, 25f, 40f, 20f), String.Format("{0:F0} K", nominalTemp), host.GUIResources.GetStyle("text_basic"));
            GUI.Label(new Rect(9f + criticalXLoc, 25f, 40f, 20f), String.Format("{0:F0} K", criticalTemp), host.GUIResources.GetStyle("text_basic"));

         GUI.EndGroup();
         GUILayout.EndHorizontal();

        // Throttle Slider
        GUILayout.BeginHorizontal();
        reactor.CurrentPowerPercent = GUILayout.HorizontalSlider(reactor.CurrentPowerPercent, 0f, 100f, GUILayout.MinWidth(150f));
        GUILayout.Label(String.Format("{0:F0}%", reactor.CurrentPowerPercent), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();
      }

      private void DrawMainControls()
      {

        GUILayout.BeginHorizontal();
        // Icon
        Rect iconRect = GUILayoutUtility.GetRect(64f, 64f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("reactor_basic").iconAtlas, host.GUIResources.GetIcon("reactor_basic").iconRect);


        // Name
        GUILayout.BeginVertical();
        GUILayout.Label(reactor.UIName, host.GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f), GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        // On/off
        bool y = reactor.ModuleIsActive();
        bool x = GUILayout.Toggle(reactor.ModuleIsActive(),"Active",host.GUIResources.GetStyle("button_toggle"));
        if (x != y)
        {
            reactor.ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
        }

        // Settings Button
        if (GUILayout.Button("Geary Thing"))
        {
          ReactorUI.ShowFocusedReactorSettings(reactor);
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();



      }

      // Draw the UI component
      public void Draw()
      {

        GUILayout.BeginHorizontal();


        DrawMainControls();

        GUILayout.BeginVertical();
        DrawBasicControls();
        if (advancedMode)
        {
          DrawAdvancedControls();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();






      }
    }

}
