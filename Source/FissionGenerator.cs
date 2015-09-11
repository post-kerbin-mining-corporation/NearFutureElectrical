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

        public void FixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {

            if (Status)
            {
              double generated = (double)(Mathf.Clamp01(CurrentHeatUsed/HeatUsed) * PowerGeneration);
              double amt = this.part.RequestResource("ElectricCharge", TimeWarp.fixedDeltaTime*generated );

            }
          }

        }


    }
}
