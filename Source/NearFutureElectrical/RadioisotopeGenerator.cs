/// RadioactiveStorageContainer.cs
/// ---------------------------
// A radioisotope generator that decays over time
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using KSP.Localization;

namespace NearFutureElectrical
{
    public class ModuleRadioisotopeGenerator : PartModule
    {
        // Power generated at max
        [KSPField(isPersistant = false)]
        public float BasePower = 1f;

        // GUI elements
        [KSPField(guiName = "Half Life", isPersistant = false, guiActiveEditor = true, guiActive = true, guiUnits = " y")]
        public float HalfLife = 16f;

        [KSPField(guiName = "Power Output", isPersistant = false, guiActiveEditor = true, guiActive = true, guiUnits = " Ec/s")]
        private float ActualPower = 0;

        [KSPField(guiName = "Efficiency", isPersistant = false, guiActiveEditor = false, guiActive = true, guiUnits = "%")]
        public float PercentPower = 100f;

        // Easy mode never lets power drop below a certain %
        [KSPField(isPersistant = true)]
        public bool EasyMode = true;

        // Percent for cutoff
        [KSPField(isPersistant = false)]
        public float EasyModeCutoff = 0.05f;

        private void FixedUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                // Generate power
                part.RequestResource("ElectricCharge", -ActualPower * TimeWarp.fixedDeltaTime);
                // Decay and update efficiency
                PercentPower = AmountRemaining() * 100;
                ActualPower = AmountRemaining() * BasePower;
            }
        }


        public override string GetInfo()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleRadioisotopeGenerator_PartInfo",
              BasePower.ToString("F2"),
              HalfLife.ToString("F0"));
        }
        public string GetModuleTitle()
        {
            return "RTG";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleRadioisotopeGenerator_ModuleName");
        }
        public override void OnStart(PartModule.StartState state)
        {
            Fields["HalfLife"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleRadioisotopeGenerator_Field_HalfLife");
            Fields["PercentPower"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleRadioisotopeGenerator_Field_PercentPower");
            Fields["ActualPower"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleRadioisotopeGenerator_Field_ActualPower");
        }
        // Computes the amount remaining
        private float AmountRemaining()
        {
            double totalYears = 0d;
            // If we blew up for some reason
            if (vessel == null)
                totalYears = 0d;
            else
                totalYears = vessel.missionTime;
            totalYears = Utils.CalculateDecimalYears(totalYears);
            float amountRemaining = (float)Math.Pow(2,(-totalYears)/HalfLife);

            if (EasyMode)
                amountRemaining = Mathf.Clamp(amountRemaining,EasyModeCutoff,1f);

            return amountRemaining;
        }
    }

}
