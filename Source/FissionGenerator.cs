/// FissionGenerator
/// ---------------------------------------------------
/// A generator module that consumes availableHeat from a FissionReactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionGenerator: FissionConsumer
    {

        // Maximum power generation
        [KSPField(isPersistant = false)]
        public float PowerGeneration = 100f;

        // Current power generation
        [KSPField(isPersistant = true)]
        public float CurrentGeneration = 0f;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Generation")]
        public string GeneratorStatus;

        public void FixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {

              if (Status)
              {
                  double generated = (double)(Mathf.Clamp01(CurrentHeatUsed / HeatUsed) * PowerGeneration);
                  double amt = this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * generated);

                  if (double.IsNaN(generated))
                      generated = 0.0;

                  GeneratorStatus = String.Format("{0:F1} Ec/s", generated);
              }
              else
                  GeneratorStatus = "Offline";
          }

        }


    }
}
