using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NearFutureElectrical;
using KSP.UI.Screens;

namespace NearFutureElectrical.UI
{
  public class UIResources
  {

    private Dictionary<string, AtlasIcon> iconList;
    private Dictionary<string, GUIStyle> styleList;
    private Dictionary<string, Color> colorList;

    private Texture generalIcons;
    private Texture reactorIcons;
    private Texture capacitorIcons;

    // Get any color, given its name
    public Color GetColor(string name)
    {
        return colorList[name];
    }

    // Get any icon, given its name
    public AtlasIcon GetIcon(string name)
    {
      return iconList[name];
    }

    // Get a reactor icon, given its ID
    public AtlasIcon GetReactorIcon(int id)
    {
      return iconList[String.Format("reactor_{0}",id+1)];
    }
    // Get a capacitor icon, given its ID
    public AtlasIcon GetCapacitorIcon(int id)
    {
      return iconList[String.Format("capacitor_{0}",id+1)];
    }

    // Get a style, given its name
    public GUIStyle GetStyle(string name)
    {
      return styleList[name];
    }

    // Constructor
    public UIResources()
    {
      CreateIconList();
      CreateStyleList();
      CreateColorList();
    }

    // Iniitializes the icon database
    private void CreateIconList()
    {
      generalIcons = (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/icon_general", false);
      reactorIcons = (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/icon_reactor", false);
      capacitorIcons = (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/icon_capacitor", false);

      iconList = new Dictionary<string, AtlasIcon>();

      // Add the general icons
      iconList.Add("lightning", new AtlasIcon(generalIcons, 0.00f, 0.75f, 0.25f, 0.25f));
      iconList.Add("fire", new AtlasIcon(generalIcons, 0.25f, 0.75f, 0.25f, 0.25f));
      iconList.Add("thermometer", new AtlasIcon(generalIcons, 0.50f, 0.75f, 0.25f, 0.25f));
      iconList.Add("timer", new AtlasIcon(generalIcons, 0.75f, 0.75f, 0.25f, 0.25f));

      iconList.Add("notch", new AtlasIcon(generalIcons, 0.0f, 0.50f, 0.25f, 0.25f));
      iconList.Add("gear", new AtlasIcon(generalIcons, 0.25f, 0.50f, 0.25f, 0.25f));
      iconList.Add("capacitor_charge", new AtlasIcon(generalIcons, 0.50f, 0.50f, 0.25f, 0.25f));
      iconList.Add("throttle", new AtlasIcon(generalIcons, 0.75f, 0.50f, 0.25f, 0.25f));

      iconList.Add("throttle_auto", new AtlasIcon(generalIcons, 0.00f, 0.25f, 0.25f, 0.25f));
      iconList.Add("heat_limit", new AtlasIcon(generalIcons, 0.25f, 0.25f, 0.25f, 0.25f));
      iconList.Add("warp_limit", new AtlasIcon(generalIcons, 0.50f, 0.25f, 0.25f, 0.25f));
      iconList.Add("capacitor_rate", new AtlasIcon(generalIcons, 0.75f, 0.25f, 0.25f, 0.25f));

      iconList.Add("capacitor_charging", new AtlasIcon(generalIcons, 0.0f, 0.0f, 0.25f, 0.25f));
      iconList.Add("capacitor_discharge", new AtlasIcon(generalIcons, 0.25f, 0.0f, 0.25f, 0.25f));
      iconList.Add("accept", new AtlasIcon(generalIcons, 0.50f, 0.0f, 0.25f, 0.25f));
      iconList.Add("cancel", new AtlasIcon(generalIcons, 0.75f, 0.0f, 0.25f, 0.25f));

      // Add the reactor icons
      iconList.Add("reactor_1", new AtlasIcon(reactorIcons, 0.00f, 0.66f, 0.33f, 0.33f));
      iconList.Add("reactor_2", new AtlasIcon(reactorIcons, 0.33f, 0.66f, 0.33f, 0.33f));
      iconList.Add("reactor_3", new AtlasIcon(reactorIcons, 0.66f, 0.66f, 0.33f, 0.33f));
      iconList.Add("reactor_4", new AtlasIcon(reactorIcons, 0.00f, 0.33f, 0.33f, 0.33f));
      iconList.Add("reactor_5", new AtlasIcon(reactorIcons, 0.33f, 0.33f, 0.33f, 0.33f));
      iconList.Add("reactor_6", new AtlasIcon(reactorIcons, 0.66f, 0.33f, 0.33f, 0.33f));
      iconList.Add("reactor_7", new AtlasIcon(reactorIcons, 0.00f, 0.00f, 0.33f, 0.33f));
      iconList.Add("reactor_8", new AtlasIcon(reactorIcons, 0.33f, 0.00f, 0.33f, 0.33f));
      iconList.Add("reactor_9", new AtlasIcon(reactorIcons, 0.66f, 0.00f, 0.33f, 0.33f));

      // Add the capacitor icons
      iconList.Add("capacitor_1", new AtlasIcon(capacitorIcons, 0.00f, 0.66f, 0.33f, 0.33f));
      iconList.Add("capacitor_2", new AtlasIcon(capacitorIcons, 0.33f, 0.66f, 0.33f, 0.33f));
      iconList.Add("capacitor_3", new AtlasIcon(capacitorIcons, 0.66f, 0.66f, 0.33f, 0.33f));
      iconList.Add("capacitor_4", new AtlasIcon(capacitorIcons, 0.00f, 0.33f, 0.33f, 0.33f));
      iconList.Add("capacitor_5", new AtlasIcon(capacitorIcons, 0.33f, 0.33f, 0.33f, 0.33f));
      iconList.Add("capacitor_6", new AtlasIcon(capacitorIcons, 0.66f, 0.33f, 0.33f, 0.33f));
      iconList.Add("capacitor_7", new AtlasIcon(capacitorIcons, 0.00f, 0.00f, 0.33f, 0.33f));
      iconList.Add("capacitor_8", new AtlasIcon(capacitorIcons, 0.33f, 0.00f, 0.33f, 0.33f));
      iconList.Add("capacitor_9", new AtlasIcon(capacitorIcons, 0.66f, 0.00f, 0.33f, 0.33f));

    }

    // Initializes all the styles
    private void CreateStyleList()
    {
        styleList = new Dictionary<string, GUIStyle>();

        GUIStyle draftStyle;

        // Window
        draftStyle = new GUIStyle(HighLogic.Skin.window);
        draftStyle.padding = new RectOffset(draftStyle.padding.left, draftStyle.padding.right, 2, draftStyle.padding.bottom);
        styleList.Add("window_main", new GUIStyle(draftStyle));
        // Box
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.normal.background = null;
        styleList.Add("item_box", new GUIStyle(draftStyle));
        // Header1
        draftStyle = new GUIStyle(HighLogic.Skin.label);
        draftStyle.fontStyle = FontStyle.Bold;
        draftStyle.alignment = TextAnchor.UpperLeft;
        draftStyle.fontSize = 12;
        draftStyle.stretchWidth = true;
        styleList.Add("header_basic", new GUIStyle(draftStyle));
        // Header 2
        draftStyle.alignment = TextAnchor.MiddleLeft;
        styleList.Add("header_center", new GUIStyle(draftStyle));
        // Basic text
        draftStyle = new GUIStyle(HighLogic.Skin.label);
        draftStyle.fontSize = 12;
        draftStyle.alignment = TextAnchor.MiddleLeft;
        styleList.Add("text_basic", new GUIStyle(draftStyle));
        // Text area
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.fontSize = 11;
        styleList.Add("text_area", new GUIStyle(draftStyle));
        // Area Background
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.padding = new RectOffset(0,0,0,0);
        styleList.Add("block_background", new GUIStyle(draftStyle));
        // Toggle
        draftStyle = new GUIStyle(HighLogic.Skin.toggle);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_toggle", new GUIStyle(draftStyle));
        // Overlaid button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_overlaid", new GUIStyle(draftStyle));
        // Accept button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_accept", new GUIStyle(draftStyle));
        // Cancel button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_cancel", new GUIStyle(draftStyle));
        // Progress bar
        // background
        draftStyle = new GUIStyle(HighLogic.Skin.textField);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        styleList.Add("bar_background", new GUIStyle(draftStyle));
        // foreground
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.border = GetStyle("bar_background").border;
        draftStyle.padding = GetStyle("bar_background").padding;
        styleList.Add("bar_foreground", new GUIStyle(draftStyle));
    }
    void CreateColorList()
    {
      colorList = new Dictionary<string, Color>();

      colorList.Add("cancel_color", new Color(208f / 255f, 131f / 255f, 86f / 255f));
      colorList.Add("accept_color", new Color(209f / 255f, 250f / 255f, 146f / 255f));
      colorList.Add("capacitor_blue", new Color(104f / 255f, 167f / 255f, 209f / 255f));
      colorList.Add("readout_green", new Color(173f / 255f, 228f / 255f, 85f / 255f));

    }
  }

  // Represents an atlased icon via a source texture and rectangle
  public class AtlasIcon
  {
    public Texture iconAtlas;
    public Rect iconRect;

    public AtlasIcon(Texture theAtlas, float bl_x, float bl_y, float x_size, float y_size)
    {
      iconAtlas = theAtlas;
      iconRect = new Rect(bl_x, bl_y, x_size, y_size);
    }
  }

}
