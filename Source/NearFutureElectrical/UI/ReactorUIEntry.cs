using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using NearFutureElectrical;

namespace NearFutureElectrical.UI
{

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

        Rect controlRect = GUILayoutUtility.GetRect(165f, 64f);
        Rect iconRect = new Rect(0f, 0f, 64f, 64f);
        Rect titleRect = new Rect(64f, 0f, 101f, 32f);
        Rect toggleRect = new Rect(64f, 32f, 32f, 32f);
        Rect settingsButtonRect = new Rect(133f, 36f, 20f, 20f);

        GUI.BeginGroup(controlRect);
        // STATIC: Icon
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconAtlas, host.GUIResources.GetReactorIcon(reactor.UIIcon).iconRect);

        // STATIC: UI Name
        GUI.Label(titleRect, reactor.UIName, host.GUIResources.GetStyle("header_basic"));

        // BUTTON: Toggle
        bool current = reactor.ModuleIsActive();
        bool toggled = GUI.Toggle(toggleRect, reactor.ModuleIsActive(),"ON",host.GUIResources.GetStyle("button_toggle"));
        if (current != toggled)
        {
            reactor.ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
        }

        // BUTTON: Settings
        if (GUI.Button(settingsButtonRect, "", host.GUIResources.GetStyle("button_overlaid")))
        {
          ReactorUI.ShowFocusedReactorSettings(reactor);
        }
        GUI.DrawTextureWithTexCoords(settingsButtonRect, host.GUIResources.GetIcon("gear").iconAtlas, host.GUIResources.GetIcon("gear").iconRect);

        GUI.EndGroup();
      }

      // Draw the noninteractable components
      private void DrawReadout()
      {
        Rect controlRect = GUILayoutUtility.GetRect(270f, 64f);
        Rect heatIconRect = new Rect(0f, 0f, 20f, 20f);
        Rect heatTextRect = new Rect(20f, 1f, 60f, 20f);
        Rect powerIconRect = new Rect(80f, 0f, 20f, 20f);
        Rect powerTextRect = new Rect(100f, 1f, 60f, 20f);
        Rect lifetimeIconRect = new Rect(160f, 0f, 20f, 20f);
        Rect lifetimeTextRect = new Rect(182f, 1f, 110f, 20f);

        Rect temperatureIconRect = new Rect(0f, 30f, 20f, 20f);
        Rect temperaturePanelRect = new Rect(20f, 30f, 210f, 30f);

        GUI.BeginGroup(controlRect);

        // READOUT: Heat production
        GUI.DrawTextureWithTexCoords(heatIconRect, host.GUIResources.GetIcon("fire").iconAtlas, host.GUIResources.GetIcon("fire").iconRect);
        GUI.Label(heatTextRect, String.Format("{0:F1} kW", reactor.AvailablePower), host.GUIResources.GetStyle("text_basic"));

        // READOUT: Energy production
        GUI.DrawTextureWithTexCoords(powerIconRect, host.GUIResources.GetIcon("lightning").iconAtlas, host.GUIResources.GetIcon("lightning").iconRect);
        if (generator != null)
        {
            GUI.Label(powerTextRect, String.Format("{0:F1} Ec/s", generator.CurrentGeneration), host.GUIResources.GetStyle("text_basic"));
        }
        else
        {
            GUI.Label(powerTextRect, String.Format("N/A"), host.GUIResources.GetStyle("text_basic"));
        }

        // READOUT: Lifetime remaining
        GUI.DrawTextureWithTexCoords(lifetimeIconRect, host.GUIResources.GetIcon("timer").iconAtlas, host.GUIResources.GetIcon("timer").iconRect);
        GUI.Label(lifetimeTextRect, String.Format("{0}", reactor.FuelStatus), host.GUIResources.GetStyle("text_basic"));

        // PROGRESS BAR: Temperature bar

        GUI.DrawTextureWithTexCoords(temperatureIconRect, host.GUIResources.GetIcon("thermometer").iconAtlas, host.GUIResources.GetIcon("thermometer").iconRect);
        float coreTemp = (float)reactor.GetCoreTemperature();
        float meltdownTemp = reactor.MaximumTemperature;
        float nominalTemp = reactor.NominalTemperature;
        float criticalTemp = reactor.CriticalTemperature;


        Vector2 barBackgroundSize = new Vector2(165f, 10f);
        Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * (coreTemp / meltdownTemp), 7f);

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

            GUI.BeginGroup(temperaturePanelRect);
            GUI.Box(barBackgroundRect, "", host.GUIResources.GetStyle("bar_background"));
            GUI.color = barColor;
            GUI.Box(barForeroundRect, "", host.GUIResources.GetStyle("bar_foreground"));
            GUI.color = Color.white;
            GUI.Label(new Rect(barBackgroundRect.xMax+7f , 1f, 50f, 20f), String.Format("{0:F0} K", coreTemp), host.GUIResources.GetStyle("text_basic"));

            GUI.DrawTextureWithTexCoords(new Rect(nominalXLoc - 7f, 5f, 15f, 20f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect);
            GUI.DrawTextureWithTexCoords(new Rect(criticalXLoc - 7f, 5f, 15f, 20f), host.GUIResources.GetIcon("notch").iconAtlas, host.GUIResources.GetIcon("notch").iconRect);

         GUI.EndGroup();
         GUI.EndGroup();
      }

      // Draw the basic control set
      private void DrawBasicControls()
      {
          GUILayout.Button(" BASIC CONTROLS", host.GUIResources.GetStyle("header_basic"));
        Rect controlRect;
        if (reactor.FollowThrottle)
          controlRect = GUILayoutUtility.GetRect(180f, 64f);
        else
          controlRect = GUILayoutUtility.GetRect(180f, 24f);
        Rect throttleIconRect = new Rect(0f, 0f, 20f, 20f);
        Rect throttleSliderRect = new Rect(28f, 5f, 100f, 20f);
        Rect throttleTextRect = new Rect(135f, 2f, 40f, 20f);

        Rect realThrottleIconRect = new Rect(0f, 30f, 20f, 20f);
        Rect realThrotlePanelRect = new Rect(23f, 30f, 110f, 20f);
        Rect realThrotleTextRect = new Rect(133f, 30f, 40f, 20f);

        GUI.BeginGroup(controlRect);

        // SLIDER: Throttle
        GUI.DrawTextureWithTexCoords(throttleIconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("throttle").iconRect);
        reactor.CurrentPowerPercent = GUI.HorizontalSlider(throttleSliderRect, reactor.CurrentPowerPercent, 0f, 100f);
        GUI.Label(throttleTextRect, String.Format("{0:F0}%", reactor.CurrentPowerPercent), host.GUIResources.GetStyle("text_basic"));

        // PROGRESS BAR: Adjusted Throttle
        if (reactor.FollowThrottle)
        {
          GUI.DrawTextureWithTexCoords(realThrottleIconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("throttle").iconRect);

          float powerFraction = reactor.ActualPowerPercent/100f;

          Vector2 barBackgroundSize = new Vector2(80f, 10f);
          Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * powerFraction, 7f);

          Rect barBackgroundRect = new Rect(0f, 0f, barBackgroundSize.x, barBackgroundSize.y);
          Rect barForeroundRect = new Rect(0f, 0f, barForegroundSize.x, barForegroundSize.y);

          Color barColor = Color.green;

          GUI.BeginGroup(realThrotlePanelRect);
          GUI.Box(barBackgroundRect, "", host.GUIResources.GetStyle("bar_background"));
          GUI.color = barColor;
          GUI.Box(barForeroundRect, "", host.GUIResources.GetStyle("bar_foreground"));
          GUI.color = Color.white;
          GUI.EndGroup();

          GUI.Label(realThrotleTextRect, String.Format("{0:F0}%", reactor.ActualPowerPercent), host.GUIResources.GetStyle("text_basic"));

        }
        GUI.EndGroup();
      }

      // Draw the advanced control set
      private void DrawAdvancedControls()
      {

        Rect controlRect = GUILayoutUtility.GetRect(180f, 64f);

        Rect autoTempOffToggleRect = new Rect(30f, 0f, 20f, 20f);
        Rect autoTempOffIconRect = new Rect(0f, 0f, 28f, 28f);
        Rect autoTempOffSliderRect = new Rect(60f, 10f, 70f, 20f);
        Rect autoTempOffTextRect = new Rect(140f, 8f, 60f, 20f);

        Rect autoWarpOffToggleRect = new Rect(30f, 30f, 20f, 20f);
        Rect autoWarpOffIconRect = new Rect(0f, 30f, 28f, 28f);
        Rect autoWarpOffSliderRect = new Rect(60f, 40f, 70f, 20f);
        Rect autoWarpOffTextRect = new Rect(140f, 38f, 60f, 20f);

        GUI.BeginGroup(controlRect);
        // SLIDER: Shutdown Temperature

        GUI.DrawTextureWithTexCoords(autoTempOffIconRect, host.GUIResources.GetIcon("heat_limit").iconAtlas, host.GUIResources.GetIcon("heat_limit").iconRect);
        reactor.AutoShutdown = GUI.Toggle(autoTempOffToggleRect, reactor.AutoShutdown, "", host.GUIResources.GetStyle("button_toggle"));
        reactor.CurrentSafetyOverride = GUI.HorizontalSlider(autoTempOffSliderRect, reactor.CurrentSafetyOverride, 700f, reactor.MaximumTemperature);
        GUI.Label(autoTempOffTextRect, String.Format("{0:F0} K", reactor.CurrentSafetyOverride), host.GUIResources.GetStyle("text_basic"));


        // SLIDER: Time Warp Shutdown
        GUI.DrawTextureWithTexCoords(autoWarpOffIconRect, host.GUIResources.GetIcon("warp_limit").iconAtlas, host.GUIResources.GetIcon("warp_limit").iconRect);
        reactor.TimewarpShutdown = GUI.Toggle(autoWarpOffToggleRect, reactor.TimewarpShutdown, "", host.GUIResources.GetStyle("button_toggle"));
        reactor.TimewarpShutdownFactor = (int)GUI.HorizontalSlider(autoWarpOffSliderRect, reactor.TimewarpShutdownFactor, 0, TimeWarp.fetch.warpRates.Length-1);
        GUI.Label(autoWarpOffTextRect, String.Format("{0:F0}x", TimeWarp.fetch.warpRates[reactor.TimewarpShutdownFactor]), host.GUIResources.GetStyle("text_basic"));

        GUI.EndGroup();
      }
      // Draws the button that unhides the advanced controls
      private void DrawAdvancedControlButton(bool maximized)
      {
        GUILayout.BeginHorizontal();
        if (maximized)
        {
          if (GUILayout.Button("[-] ADVANCED CONTROLS", host.GUIResources.GetStyle("header_basic")))
          {
            advancedMode = false;
          }
        }
        else
        {
          if (GUILayout.Button("[+] ADVANCED CONTROLS", host.GUIResources.GetStyle("header_basic")))
          {
            advancedMode = true;
          }
        }
        GUILayout.EndHorizontal();
      }

      // Draw the UI component
      public void Draw()
      {

          GUILayout.BeginHorizontal(host.GUIResources.GetStyle("block_background"));

        GUILayout.BeginHorizontal(host.GUIResources.GetStyle("item_box"));
        DrawMainControls();
        DrawReadout();
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(host.GUIResources.GetStyle("item_box"));
        DrawBasicControls();
        DrawAdvancedControlButton(advancedMode);
        if (advancedMode)
        {
          DrawAdvancedControls();
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();






      }
    }

}
