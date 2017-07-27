/// ModuleCoreHeat
/// ---------------------------------------------------
/// A version of ModuleCoreHeat that does not do any kind of catchup for
/// its thermal parameters

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using NearFutureElectrical.UI;
using KSP.Localization;

namespace NearFutureElectrical
{
    public class ModuleCoreHeatNoCachup: ModuleCoreHeat
    {
      public override void OnSave(ConfigNode node)
      {
        lastFlux = 0d;
        base.OnSave(node);
      }
    }
}
