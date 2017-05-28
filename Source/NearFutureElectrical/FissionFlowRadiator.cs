using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace NearFutureElectrical
{
    class FissionFlowRadiator: ModuleActiveRadiator
    {
        [KSPField(isPersistant = false)]
        public float exhaustCooling = 0f;

        [KSPField(isPersistant = false)]
        public float passiveCooling = 0f;

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Rejected")]
        public string RadiatorStatus;

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

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            Fields["RadiatorStatus"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionFlowRadiator_Field_RadiatorStatus");
        }

        void Update()
        {
            // oh god.
            if (ticker > 60)
            {
                base.Activate();
                ticker = 0;
            }
            else
            {
                ticker = ticker + 1;
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                foreach (BaseField fld in base.Fields)
                {
                    if (fld.name == "Cooling")
                    {
                        fld.guiActive = false;
                        fld.guiActiveEditor = false;
                    }
                }
                if (Events["Activate"].active == true)
                    Events["Activate"].active = false;
                if (Events["Shutdown"].active == true)
                    Events["Shutdown"].active = false;
            }
        }

        public void ChangeRadiatorTransfer(float scale)
        {
            base.maxEnergyTransfer = (scale*exhaustCooling + passiveCooling) * 50d;
            RadiatorStatus = String.Format("{0:F0} kW", scale * exhaustCooling + passiveCooling);
        }
    }
}
