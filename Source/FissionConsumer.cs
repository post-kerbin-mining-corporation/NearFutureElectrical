/// ReactorConsumer
/// ---------------------------------------------------
/// Base thing to consume output from a fission reactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionConsumer: PartModule
    {

        // consumer status on/off
        [KSPField(isPersistant = false)]
        public bool Status = true;

        // Order in which to take power. Low is first.
        [KSPField(isPersistant = false)]
        public float Priority = 1;

        // Amount of heat used to produce above power
        [KSPField(isPersistant = false)]
        public float HeatUsed = 500f;

        // current amount of heat being used
        [KSPField(isPersistant = true)]
        public float CurrentHeatUsed = 0f;

        public void SetHeatInput(float heatIn)
        {
          CurrentHeatUsed = heatIn;
        }



    }
}
