/// Resources
/// -------------------------
/// Doesn't do much but store GUI textures. More will come!
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public static class Resources
    {
        // foreground for progress bars
        public static Texture2D gui_progressbar = new Texture2D(16, 16);
        public static Texture2D gui_show = new Texture2D(16, 16);   


        // Styles


        // Load that texture!
        public static void LoadResources()
        {
            //gui_progressbar.LoadImage(KSP.IO.File.ReadAllBytes<NearFutureElectrical>("ui_progress.png"));
           // gui_show.LoadImage(KSP.IO.File.ReadAllBytes<NearFutureElectrical>("ui_show.png"));
        }
    }
}
