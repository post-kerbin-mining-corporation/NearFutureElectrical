/// FissionEngine
/// ---------------------------------------------------
/// An engine module that consumes availableHeat from a FissionReactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionEngine: FissionConsumer
    {
        // ModuleEnginesFX ID to go nuclear
        [KSPField(isPersistant = false)]
        public string EngineID;

        // Relates core temp (K) to Isp scalar
        [KSPField(isPersistant = false)]
        public FloatCurve TempIspScale = new FloatCurve();


        private FloatCurve baseIspCurve;

        private ModuleEnginesFX engineFX;
        private FissionReactor reactor;
        private ModuleCoreHeat core;

        public override string GetInfo()
        {
            return "";
        }

        public override void OnStart(PartModule.StartState state)
        {
            SetupEngines();
            SetupReactor();

        }

        private void SetupEngines()
        {
          // Locate the engine modules
          List<ModuleEnginesFX> engines = this.GetComponents<ModuleEnginesFX>().ToList();
          // Get their Isps
          foreach (ModuleEnginesFX engine in engines) {
              if (engine.engineID == EngineID)
              {
                  engineFX = engine;
                  baseIspCurve = engine.atmosphereCurve;
              }
          }
        }
        private void SetupReactor()
        {
            reactor = this.GetComponent<FissionReactor>();
            core = this.GetComponent<ModuleCoreHeat>();
        }

        public void FixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {
            if (engineFX != null && reactor != null)
            {
                // If the engine is off, it will have the maximum Isp available
                if (!engineFX.isActiveAndEnabled || (engineFX.isActiveAndEnabled && engineFX.throttleSetting > 0f))
                {
                  engineFX.atmosphereCurve = baseIspCurve;
                } else
                // Calculate Isp based on core temperature
                {
                  float CoreTemperatureRatio =  TempIspScale.Evaluate((float)core.CoreTemperature);

                  // Scale Isps
                  engineFX.atmosphereCurve = new FloatCurve();
                  engineFX.atmosphereCurve.Add(0f,baseIspCurve.Evaluate(0f)*CoreTemperatureRatio);
                  engineFX.atmosphereCurve.Add(1f,baseIspCurve.Evaluate(1f)*CoreTemperatureRatio);
                  engineFX.atmosphereCurve.Add(4f,baseIspCurve.Evaluate(4f)*CoreTemperatureRatio);

                  // The amount of heat to consume depends on mass flow rate
                  float flowRate = engineFX.fuelFlowGui;

                  // Consume the power from the core
                  core.AddEnergyToCore(-CurrentHeatUsed);
                }

            }

          }
        }

    }
}
