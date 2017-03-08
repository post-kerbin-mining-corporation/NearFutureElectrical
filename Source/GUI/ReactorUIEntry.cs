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

      private Color nominalColor = Color.green;
      private Color criticalColor = new Color(1.0f, 102f / 255f, 0f);
      private Color meltdownColor = Color.red;

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
      private void DrawMainControls()
      {

        GUILayout.BeginHorizontal();
        // STATIC: Icon
        Rect iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconAtlas, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconRect);

        // STATIC: UI Name
        GUILayout.BeginVertical();
        GUILayout.Label(reactor.UIName, host.GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f), GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        // BUTTON: Toggle
        bool y = reactor.ModuleIsActive();
        bool x = GUILayout.Toggle(reactor.ModuleIsActive(),"Active",host.GUIResources.GetStyle("button_toggle"));
        if (x != y)
        {
            reactor.ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
        }

        // BUTTON: Settings
        iconRect = GUILayoutUtility.GetRect(32f, 32f);
        if (GUI.Button(iconRect, "", host.GUIResources.GetStyle("button_overlaid")))
        {
          ReactorUI.ShowFocusedReactorSettings(reactor);
        }
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("gear").iconAtlas, host.GUIResources.GetIcon("gear").iconRect);

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

      }


      private void DrawReadout()
      {
        GUILayout.BeginVertical();


        // READOUT: Heat production
        GUILayout.BeginHorizontal();
        Rect iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("fire").iconAtlas, host.GUIResources.GetIcon("fire").iconRect;
        GUILayout.Label(String.Format("{0:F1} kW", reactor.AvailablePower), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
        GUILayout.Space(10f);
        // READOUT: Energy production
        iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("lightning").iconAtlas, host.GUIResources.GetIcon("lightning").iconRect;
        if (generator != null)
        {
            GUILayout.Label(String.Format("{0:F1} Ec/s", generator.CurrentGeneration), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(140f), GUILayout.MinWidth(140f));
        }
        else
        {
            GUILayout.Label(String.Format("N/A"), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(100f), GUILayout.MinWidth(100f));
        }
        // READOUT: Lifetime remaining
        iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("timer").iconAtlas, host.GUIResources.GetIcon("timer").iconRect;
        GUILayout.Label(String.Format("{0}", reactor.FuelStatus), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(120f), GUILayout.MinWidth(120f));
        GUILayout.EndHorizontal();
        // PROGRESS BAR: Temperature bar
        GUILayout.BeginHorizontal();
        iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("thermometer").iconAtlas, host.GUIResources.GetIcon("thermometer").iconRect;

        float coreTemp = (float)reactor.GetCoreTemperature();
        float meltdownTemp = reactor.MaximumTemperature;
        float nominalTemp = reactor.NominalTemperature;
        float criticalTemp = reactor.CriticalTemperature;

        Vector2 readoutSize = new Vector2(250f, 60f);
        Vector2 barBackgroundSize = new Vector2(210f, 10f);
        Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * (coreTemp / meltdownTemp), 7f);

        Rect readoutRect = GUILayoutUtility.GetRect(readoutSize.x, readoutSize.y);
        Rect barBackgroundRect = new Rect(0f, 10f, barBackgroundSize.x, barBackgroundSize.y);
        Rect barForeroundRect = new Rect(0f, 11f, barForegroundSize.x, barForegroundSize.y);

        float nominalXLoc = nominalTemp / meltdownTemp * barBackgroundRect.width + barBackgroundRect.xMin;
        float criticalXLoc = criticalTemp / meltdownTemp * barBackgroundRect.width + barBackgroundRect.xMin;

        Color barColor = new Color();

        if (coreTemp <= nominalTemp)
            barColor = nominalColor;
        else if (coreTemp <= criticalTemp)
            barColor = Color.Lerp(nominalColor, criticalColor, (coreTemp - nominalTemp) / (criticalTemp - nominalTemp));
        else
            barColor = Color.Lerp(criticalColor, meltdownColor, (coreTemp - criticalTemp) / (meltdownTemp - criticalTemp));

            GUI.BeginGroup(readoutRect);
            GUI.Box(barBackgroundRect, "", host.GUIResources.GetStyle("bar_background"));
            GUI.color = barColor;
            GUI.Box(barForeroundRect, "", host.GUIResources.GetStyle("bar_foreground"));
            GUI.color = Color.white;
            GUI.Label(new Rect(barBackgroundRect.xMax+7f , 8f, 40f, 20f), String.Format("{0:F0} K", coreTemp), host.GUIResources.GetStyle("text_basic"));

            GUI.DrawTextureWithTexCoords(new Rect(nominalXLoc - 13f, 16f, 22f, 25f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect;
            GUI.DrawTextureWithTexCoords(new Rect(criticalXLoc - 13f, 16f, 22f, 25f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect;

         GUI.EndGroup();
         GUILayout.EndHorizontal();
         GUILayout.EndVertical();





      }

      private void DrawBasicControls()
      {
        // SLIDER: Throttle
        GUILayout.BeginHorizontal();
        Rect iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("throttle").iconRect;
        reactor.CurrentPowerPercent = GUILayout.HorizontalSlider(reactor.CurrentPowerPercent, 0f, 100f, GUILayout.MinWidth(75f));
        GUILayout.Label(String.Format("{0:F0}%", reactor.CurrentPowerPercent), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();

        // PROGRESS BAR: Adjusted Throttle
        if (reactor.FollowThrottle)
        {
          GUILayout.BeginHorizontal();
          iconRect = GUILayoutUtility.GetRect(32f, 32f);
          GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("thermometer").iconRect;

          float powerFraction = reactor.ActualPowerPercent/100f;

          Vector2 readoutSize = new Vector2(150f, 20f);
          Vector2 barBackgroundSize = new Vector2(130f, 10f);
          Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * powerFraction, 7f);

          Rect readoutRect = GUILayoutUtility.GetRect(readoutSize.x, readoutSize.y);
          Rect barBackgroundRect = new Rect(0f, 0f, barBackgroundSize.x, barBackgroundSize.y);
          Rect barForeroundRect = new Rect(0f, 0f, barForegroundSize.x, barForegroundSize.y);

          Color barColor = Color.green;

          GUI.BeginGroup(readoutRect);
          GUI.Box(barBackgroundRect, "", host.GUIResources.GetStyle("bar_background"));
          GUI.color = barColor;
          GUI.Box(barForeroundRect, "", host.GUIResources.GetStyle("bar_foreground"));
          GUI.color = Color.white;
           GUI.EndGroup();

           GUILayout.Label(String.Format("{0:F0}%", reactor.ActualPowerPercent), host.GUIResources.GetStyle("text_basic"));
           GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

      }

      private void DrawAdvancedControls()
      {
        // SLIDER: Shutdown Temperature
        GUILayout.BeginHorizontal();
        reactor.AutoShutdown = GUILayout.Toggle(reactor.AutoShutdown, "Safety Override", host.GUIResources.GetStyle("button_toggle"));

        reactor.CurrentSafetyOverride = GUILayout.HorizontalSlider(reactor.CurrentSafetyOverride, 700f, reactor.MaximumTemperature, GUILayout.MinWidth(150f));
        GUILayout.Label(String.Format("{0:F0} K", reactor.CurrentSafetyOverride), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();

        // SLIDER: Time Warp Shutdown
        GUILayout.BeginHorizontal();
        reactor.TimewarpShutdown = GUILayout.Toggle(reactor.TimewarpShutdown, "Warp Shutdown", host.GUIResources.GetStyle("button_toggle"));
        reactor.CurrentSafetyOverride = GUILayout.HorizontalSlider(reactor.CurrentSafetyOverride, 700f, reactor.MaximumTemperature, GUILayout.MinWidth(150f));
        GUILayout.Label(String.Format("{0:F0} K", reactor.CurrentSafetyOverride), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();
      }



      // Draw the UI component
      public void Draw()
      {

        GUILayout.BeginHorizontal();


        DrawMainControls();
        DrawReadout();

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
