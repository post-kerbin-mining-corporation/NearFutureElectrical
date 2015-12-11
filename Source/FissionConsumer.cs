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

        // Amount of heat used at most by the consumer
        [KSPField(isPersistant = false)]
        public float HeatUsed = 500f;

        //
        [KSPField(isPersistant = false)]
        public float Efficiency = 0.5f;

        // current amount of heat being used
        [KSPField(isPersistant = true)]
        public float CurrentHeatUsed = 0f;

        [KSPField(isPersistant = true)]
        public float Setting = 0f;

        public void SetPowerSetting(float setting)
        {
            Setting = setting;
        }

        public void SetHeatInput(float heatIn)
        {
            CurrentHeatUsed = heatIn;

        }
        public float GetWaste()
        {
            return (CurrentHeatUsed * (1f-Efficiency) );
        }

        // Given a certain amount of heat, returns the amount left over
        public float ConsumeHeat(float heatAvailable)
        {
          if (heatAvailable >= HeatUsed)
              CurrentHeatUsed = HeatUsed;
          else
              CurrentHeatUsed = Mathf.Clamp(heatAvailable,0f,10000000f);

          return heatAvailable - CurrentHeatUsed;
        }

    }
}
