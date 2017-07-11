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
        int ticker = 0;

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
            currentCooling = (scale*exhaustCooling + passiveCooling);
            RadiatorStatus = String.Format("{0:F0} kW", scale * exhaustCooling + passiveCooling);
        }

        private void ConsumeEnergy()
        {
            if (core != null)
            {
              core.AddEnergyToCore(currentCooling*TimeWarp.fixedDeltaTime);
            }
        }
    }
}
