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

        // Added EC to tanks
        [KSPField(isPersistant = false)]
        public float AddedToFuelTanks = 0f;

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

                    if (double.IsNaN(generated))
                        generated = 0.0;

                    double generatedDeltaTime = TimeWarp.fixedDeltaTime * generated;
                    AddedToFuelTanks = (float)-this.part.RequestResource("ElectricCharge", -generatedDeltaTime);
                    CurrentGeneration = (float)generated;

                    GeneratorStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_Field_GeneratorStatus_Normal", generated.ToString("F1"));
                }
                else
                {
                    CurrentGeneration = 0f;
                    AddedToFuelTanks = 0f;
                    GeneratorStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionGenerator_Field_GeneratorStatus_Offline");
                }
            }
        }
    }
}
