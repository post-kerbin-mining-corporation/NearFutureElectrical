
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace NearFutureElectrical
{
    //[KSPModule("RadioisotopeGenerator")]
    public class ModuleRadioisotopeGenerator : PartModule
    {
        [KSPField(isPersistant = false)]
        public float BasePower = 1f;
        [KSPField(guiName = "Half Life", isPersistant = false, guiActiveEditor = true, guiActive = true, guiUnits = " y")]
        public float HalfLife = 16f;

        // information output
        [KSPField(guiName = "Power Output", isPersistant = false, guiActiveEditor = true, guiActive = true, guiUnits = " Ec/s")]
        private float ActualPower = 0;


        [KSPField(guiName = "Efficiency", isPersistant = false, guiActiveEditor = false, guiActive = true, guiUnits = "%")]
        public float PercentPower = 100f;

        // Easy mode never lets power drop below 1%
        [KSPField(isPersistant = true)]
        public bool EasyMode = true;

        public override void OnFixedUpdate()
        {
            // Generate power
            part.RequestResource("ElectricCharge", -ActualPower * TimeWarp.fixedDeltaTime);
            // Decay and update efficiency
            PercentPower = AmountRemaining() * 100;
            ActualPower = AmountRemaining() * BasePower;
        }


        public override string GetInfo()
        {
            return "Generates power constantly, but decays over time \n\n" +
                String.Format("Power Generated: {0:F2} Ec/s",BasePower) + "\n" +
                String.Format("Half-Life: {0:F0} y", HalfLife);
        }



        private float AmountRemaining()
        {
            double totalYears = 0d;
            if (vessel == null)
            totalYears = 0d;
            else
            totalYears = vessel.missionTime;

            totalYears = Utils.CalculateDecimalYears(totalYears);

            float amountRemaining = (float)Math.Pow(2,(-totalYears)/HalfLife);

            if (EasyMode)
                amountRemaining = Mathf.Clamp(amountRemaining,0.05f,1f);

            return amountRemaining;
        }
    }

}

