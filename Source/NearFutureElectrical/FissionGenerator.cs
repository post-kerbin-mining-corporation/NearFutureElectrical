/// FissionGenerator
/// ---------------------------------------------------
/// A generator module that consumes availableHeat from a FissionReactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

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

        public override string GetInfo()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_PartInfo", PowerGeneration.ToString("F0"));
        }
        public string GetModuleTitle()
        {
            return "FissionGenerator";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_ModuleName");
        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            Fields["GeneratorStatus"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_Field_GeneratorStatus");
        }

        public void FixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {

              if (Status)
              {

                  double generated = (double)(Mathf.Clamp01(CurrentHeatUsed / HeatUsed) * PowerGeneration);

                  double delta = 0d;
                  for (int i = 0; i < this.vessel.parts.Count; i++)

                  {
                      if (this.vessel.parts[i].Resources.Get(PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id) != null)
                           delta += this.vessel.parts[i].Resources.Get(PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id).maxAmount -
                                this.vessel.parts[i].Resources.Get(PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id).amount;
                  }

                  double generatedActual = Math.Min(delta, TimeWarp.fixedDeltaTime * generated);

                  double amt = this.part.RequestResource("ElectricCharge",  -generatedActual);

                  if (double.IsNaN(generated))
                      generated = 0.0;

                  CurrentGeneration = (float)generated;
                  GeneratorStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_Field_GeneratorStatus_Normal", generated.ToString("F1"));
              }
              else
                  GeneratorStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_Field_GeneratorStatus_Offline");
          }

        }


    }
}
