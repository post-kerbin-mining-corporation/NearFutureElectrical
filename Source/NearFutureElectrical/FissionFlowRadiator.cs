using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace NearFutureElectrical
{
    class FissionFlowRadiator: PartModule
    {
        [KSPField(isPersistant = false)]
        public float exhaustCooling = 0f;

        [KSPField(isPersistant = false)]
        public float passiveCooling = 0f;

        [KSPField(isPersistant = false)]
        public float CoolingDecayRate = 1000f;

        [KSPField(isPersistant = false)]
        public float CoolingScalingZero = 500f;

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Rejected")]
        public string RadiatorStatus;

        private float currentCooling = 0f;
        private ModuleCoreHeat core;

        public override string GetInfo()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleFissionFlowRadiator_PartInfo", exhaustCooling.ToString("F0"), passiveCooling.ToString("F0"));
        }
        public string GetModuleTitle()
        {
            return "FissionRadiator";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleFissionFlowRadiator_ModuleName");
        }

        public void Start()
        {
          if (HighLogic.LoadedSceneIsFlight)
          {
            core = this.GetComponent<ModuleCoreHeat>();
          }
          Fields["RadiatorStatus"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionFlowRadiator_Field_RadiatorStatus");
        }

        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
              ConsumeEnergy();
            }
        }

        public void ChangeRadiatorTransfer(float scale)
        {
            float targetCooling = Mathf.Clamp(scale*exhaustCooling + passiveCooling, 0.0f, exhaustCooling);
            if (targetCooling > currentCooling)
                currentCooling = targetCooling;
            else
                currentCooling = Mathf.MoveTowards(currentCooling, targetCooling, TimeWarp.fixedDeltaTime*CoolingDecayRate);

            RadiatorStatus = String.Format("{0:F0} {1}", currentCooling, Localizer.Format("#LOC_NFElectrical_Units_kW"));
        }

        private void ConsumeEnergy()
        {
            if (core != null)
            {

                float maxTemp = (float)core.CoreTempGoal;
                float coreTemp =(float)core.CoreTemperature;

                if (coreTemp > maxTemp)
                {
                    float scale = Mathf.Clamp01(CoolingScalingZero * (coreTemp / maxTemp - 1f));
                    core.AddEnergyToCore(-currentCooling *50f*TimeWarp.fixedDeltaTime* scale);
                }




            }
        }
    }
}
