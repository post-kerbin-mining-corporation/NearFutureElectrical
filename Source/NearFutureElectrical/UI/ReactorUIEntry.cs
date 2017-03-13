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
      // Color of the temperature bar
      private Color nominalColor = Color.green;
      private Color criticalColor = new Color(1.0f, 102f / 255f, 0f);
      private Color meltdownColor = Color.red;

      private bool advancedMode = false;

      private ReactorUI host;
      private FissionReactor reactor;
      private FissionGenerator generator;

      // Constructor
      public ReactorUIEntry(FissionReactor toDraw, ReactorUI uihost)
      {
          host = uihost;
        reactor = toDraw;
        generator = reactor.GetComponent<FissionGenerator>();
      }

      // Draw the main control area
      private void DrawMainControls()
      {

        GUILayout.BeginHorizontal();
        // STATIC: Icon
        Rect iconRect = GUILayoutUtility.GetRect(64f, 64f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconAtlas, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconRect);

        // STATIC: UI Name
        GUILayout.BeginVertical();
        GUILayout.Label(reactor.UIName, host.GUIResources.GetStyle("header_basic"), GUILayout.MaxHeight(32f), GUILayout.MinHeight(32f), GUILayout.MaxWidth(80f), GUILayout.MinWidth(80f));
        

        GUILayout.BeginHorizontal();
        // BUTTON: Toggle
        bool current = reactor.ModuleIsActive();
        bool toggled = GUILayout.Toggle(reactor.ModuleIsActive(),"ON",host.GUIResources.GetStyle("button_toggle"));
        if (current != toggled)
        {
            reactor.ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
        }
        GUILayout.FlexibleSpace();
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

      // Draw the noninteractable components
      private void DrawReadout()
      {
        GUILayout.BeginVertical();


        // READOUT: Heat production
        GUILayout.BeginHorizontal();
        Rect iconRect = GUILayoutUtility.GetRect(20f, 20f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("fire").iconAtlas, host.GUIResources.GetIcon("fire").iconRect);
        GUILayout.Label(String.Format("{0:F1} kW", reactor.AvailablePower), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(50f), GUILayout.MinWidth(50f));
        GUILayout.Space(10f);
        // READOUT: Energy production
        iconRect = GUILayoutUtility.GetRect(20f, 20f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("lightning").iconAtlas, host.GUIResources.GetIcon("lightning").iconRect);
        if (generator != null)
        {
            GUILayout.Label(String.Format("{0:F1} Ec/s", generator.CurrentGeneration), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(50f), GUILayout.MinWidth(50f));
        }
        else
        {
            GUILayout.Label(String.Format("N/A"), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(50f), GUILayout.MinWidth(50f));
        }
        // READOUT: Lifetime remaining
        iconRect = GUILayoutUtility.GetRect(20f, 20f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("timer").iconAtlas, host.GUIResources.GetIcon("timer").iconRect);
        GUILayout.Label(String.Format("{0}", reactor.FuelStatus), host.GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(50f), GUILayout.MinWidth(50f));
        GUILayout.EndHorizontal();
        // PROGRESS BAR: Temperature bar
        GUILayout.BeginHorizontal();
        iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("thermometer").iconAtlas, host.GUIResources.GetIcon("thermometer").iconRect);
        GUILayout.FlexibleSpace();
        float coreTemp = (float)reactor.GetCoreTemperature();
        float meltdownTemp = reactor.MaximumTemperature;
        float nominalTemp = reactor.NominalTemperature;
        float criticalTemp = reactor.CriticalTemperature;

        Vector2 readoutSize = new Vector2(180f, 35f);
        Vector2 barBackgroundSize = new Vector2(165f, 10f);
        Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * (coreTemp / meltdownTemp), 7f);

        Rect readoutRect = GUILayoutUtility.GetRect(readoutSize.x, readoutSize.y);
        Rect barBackgroundRect = new Rect(0f, 5f, barBackgroundSize.x, barBackgroundSize.y);
        Rect barForeroundRect = new Rect(0f, 6f, barForegroundSize.x, barForegroundSize.y);

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
            GUI.Label(new Rect(barBackgroundRect.xMax+7f , 3f, 40f, 20f), String.Format("{0:F0} K", coreTemp), host.GUIResources.GetStyle("text_basic"));

            GUI.DrawTextureWithTexCoords(new Rect(nominalXLoc - 7f, 10f, 15f, 20f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect);
            GUI.DrawTextureWithTexCoords(new Rect(criticalXLoc - 7f, 10f, 15f, 20f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect);

         GUI.EndGroup();
         GUILayout.EndHorizontal();
         GUILayout.EndVertical();
      }

      // Draw the basic control set
      private void DrawBasicControls()
      {
        // SLIDER: Throttle
        GUILayout.BeginHorizontal();
        Rect iconRect = GUILayoutUtility.GetRect(32f, 32f);
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("throttle").iconRect);
        reactor.CurrentPowerPercent = GUILayout.HorizontalSlider(reactor.CurrentPowerPercent, 0f, 100f, GUILayout.MinWidth(75f));
        GUILayout.Label(String.Format("{0:F0}%", reactor.CurrentPowerPercent), host.GUIResources.GetStyle("text_basic"));
        GUILayout.EndHorizontal();

        // PROGRESS BAR: Adjusted Throttle
        if (reactor.FollowThrottle)
        {
          GUILayout.BeginHorizontal();
          iconRect = GUILayoutUtility.GetRect(32f, 32f);
          GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("thermometer").iconRect);

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
      }

      // Draw the advanced control set
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
      // Draws the button that unhides the advanced controls
      private void DrawAdvancedControlButton(bool maximized)
      {
        GUILayout.BeginHorizontal();
        if (maximized)
        {
          if (GUILayout.Button("[-] ADVANCED", host.GUIResources.GetStyle("header_basic")))
          {
            advancedMode = false;
          }
        }
        else
        {
          if (GUILayout.Button("[+] ADVANCED", host.GUIResources.GetStyle("header_basic")))
          {
            advancedMode = true;
          }
        }
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
        DrawAdvancedControlButton(advancedMode);
        if (advancedMode)
        {
          DrawAdvancedControls();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();






      }
    }

}
