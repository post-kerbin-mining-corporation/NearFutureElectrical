using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using NearFutureElectrical;

namespace NearFutureElectrical.UI
{

    public class CapacitorUIEntry
    {
      // Color of the temperature bar

      private DischargeCapacitorUI host;
      private DischargeCapacitor capacitor;

      // Constructor
      public CapacitorUIEntry(DischargeCapacitor toDraw, DischargeCapacitorUI uihost)
      {
          host = uihost;
         capacitor = toDraw;
      }

      // Draw the main control area
      private void DrawMainControls()
      {

        Rect controlRect = GUILayoutUtility.GetRect(100f, 32f);
        Rect iconRect = new Rect(0f, 0f, 32f, 32f);
        Rect titleRect = new Rect(32f, 0f, 68f, 32f);

        GUI.BeginGroup(controlRect);
        // STATIC: Icon
        GUI.DrawTextureWithTexCoords(iconRect, host.GUIResources.GetCapacitorIcon(0).iconAtlas, host.GUIResources.GetCapacitorIcon(0).iconRect);
        // STATIC: UI Name
        GUI.Label(titleRect, capacitor.part.partInfo.title, host.GUIResources.GetStyle("header_basic"));
        GUI.EndGroup();
      }

      // DrawControl bit
      private void DrawReadout()
      {
        Rect controlRect = GUILayoutUtility.GetRect(220f, 32f);

        Rect dischargeButtonRect = new Rect(0f, 0f, 32f, 32f);

        Rect dischargeRateIconRect = new Rect(32f, 0f, 20f, 16f);
        Rect dischargeRateSliderRect = new Rect(48f, 0f, 100f, 16f);
        Rect dischargeRateTextRect = new Rect(148f, 0f, 60f, 16f);

        Rect chargeBarIconRect = new Rect(32f, 16f, 16f, 16f);
        Rect chargeBarPanelRect = new Rect(48f, 16f, 100f, 16f);
        Rect chargeBarTextRect = new Rect(148f, 16f, 60f, 16f);

        GUI.BeginGroup(controlRect);

        // BUTTON: Discharge button
        GUI.DrawTextureWithTexCoords(dischargeButtonRect, host.GUIResources.GetIcon("lightning").iconAtlas, host.GUIResources.GetIcon("lightning").iconRect);
        if (GUI.Button(dischargeButtonRect, ""))
        {
          capacitor.Discharge();
        }

        // SLIDER: Discharge rate
        GUI.DrawTextureWithTexCoords(dischargeRateIconRect, host.GUIResources.GetIcon("throttle").iconAtlas, host.GUIResources.GetIcon("throttle").iconRect);
        capacitor.dischargeActual = GUI.HorizontalSlider(dischargeRateSliderRect, capacitor.dischargeActual, capacitor.DischargeRate/2f, capacitor.DischargeRate);
        GUI.Label(dischargeRateTextRect, String.Format("{0:F0} Ec/s", capacitor.dischargeActual), host.GUIResources.GetStyle("text_basic"));

        // PROGRESS BAR: Charge fraction bar
        GUI.DrawTextureWithTexCoords(dischargeRateIconRect, host.GUIResources.GetIcon("capacitor_charge").iconAtlas, host.GUIResources.GetIcon("capacitor_charge").iconRect);

        Vector2 barBackgroundSize = new Vector2(90f, 10f);
        Vector2 barForegroundSize = new Vector2(barBackgroundSize.x * (GetChargePercent()/100.0f), 7f);

        Rect barBackgroundRect = new Rect(0f, 5f, barBackgroundSize.x, barBackgroundSize.y);
        Rect barForeroundRect = new Rect(0f, 6f, barForegroundSize.x, barForegroundSize.y);

        Color barColor = new Color();
            GUI.BeginGroup(chargeBarPanelRect);
            GUI.Box(barBackgroundRect, "", host.GUIResources.GetStyle("bar_background"));
            GUI.color = Color.green;
            GUI.Box(barForeroundRect, "", host.GUIResources.GetStyle("bar_foreground"));
            GUI.color = Color.white;
         GUI.EndGroup();
         GUI.Label(chargeBarTextRect, String.Format("{0}% ({1} Sc/s)", GetChargePercent(), GetCurrentRate()), host.GUIResources.GetStyle("text_basic"));
         GUI.EndGroup();
      }

      // Draw the basic control set
      private void DrawBasicControls()
      {
        Rect controlRect = GUILayoutUtility.GetRect(130f, 64f);

          GUI.BeginGroup(controlRect);

        GUI.EndGroup();
      }

      // Draw the UI component
      public void Draw()
      {

        GUILayout.BeginHorizontal();
        DrawMainControls();
        DrawBasicControls();
        GUILayout.EndHorizontal();
      }

      // Gets the current charge or discharge rate of a capacitor
      private float GetCurrentRate()
      {
          if (capacitor.Discharging)
          {
              return -capacitor.DischargeRate;
          } else if (capacitor.Enabled && capacitor.CurrentCharge < capacitor.MaximumCharge)
          {
              return capacitor.ChargeRate*capacitor.ChargeRatio;
          } else
          {
              return 0f;
          }
      }
      // Gets a capacitor's percent charge
      private float GetChargePercent()
      {
          return (capacitor.CurrentCharge / capacitor.MaximumCharge) *100f;
      }

    }

}
