

namespace NearFutureElectrical.UI
{
  public class UIResources
  {

    private Dictionary<string, AtlasIcon> iconList;
    private Dictionary<string, GUIStyle> styleList;

    private Texture generalIcons;
    private Texture reactorIcons;

    public AtlasIcon GetIcon(string name)
    {
      return iconList[name];
    }

    public GUIStyle GetStyle(string name)
    {
      return styleList[name];
    }

    public class UIComponents()
    {
      CreateIconList();
      CreateStyleList();
    }

    // Iniitializes the icon database
    private CreateIconList()
    {
      generalIcons = (Texture)GameDatabase.Instance.GetTexture("NearElectrical/UI/icon_general", false);
      reactorIcons = (Texture)GameDatabase.Instance.GetTexture("NearFutureElectrical/UI/icon_reactor", false);

      iconList = new Dictionary<string, AtlasIcon>();

      // Add the general icons
      iconList.Add("lightning", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("fire", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("thermometer", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("timer", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("notch", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("gear", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));

      // Add the reactor icons
      iconList.Add("reactor_basic", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("reactor_large", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));
      iconList.Add("reactor_plant", new AltasIcon(generalIcons, 0.0, 0.0, 0.25, 0.25));

    }

    // Initializes all the styles
    private CreateStyleList()
    {
        styleList = new Dictionary<string, GUIStyle>();

        GUIStyle draftStyle;

        // Window
        draftStyle = new GUIStyle(HighLogic.Skin.window);
        styleList.Add("window_main", new GUIStyle(draftStyle));
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
        draftStyle.fontSize = 11;
        draftStyle.alignment = TextAnchor.MiddleLeft;
        styleList.Add("text_basic", new GUIStyle(draftStyle));
        // Area Background
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.active = gui_bg.hover = gui_bg.normal;
        styleList.Add("block_background", new GUIStyle(draftStyle));
        // Toggle
        draftStyle = new GUIStyle(HighLogic.Skin.toggle);
        draftStyle.normal.textColor = gui_header.normal.textColor;
        styleList.Add("button_toggle", new GUIStyle(draftStyle));
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

  }

  // Represents an atlased icon via a source texture and rectangle
  public class AtlasIcon()
  {
    public Texture iconAtlas;
    public Rect iconRect;

    public AtlasIcon(Texture theAtlas, bl_x, bl_y, x_size, y_size)
    {
      iconAtlas = theAtlas;
      iconRect = new Rect(bl_x, bl_y, x_size, y_size);
    }
  }

}
