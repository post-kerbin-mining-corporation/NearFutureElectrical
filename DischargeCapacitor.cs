/// DischargeCapacitor
/// ---------------------------------------------------
/// A module that discharges 
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    class DischargeCapacitor : PartModule
    {

        // Is capacitor online
        [KSPField(isPersistant = true)]
        public bool Enabled;
        // Is capacitor discharging
        [KSPField(isPersistant = true)]
        public bool Discharging;

        // Discharge Rate
        [KSPField(isPersistant = false)]
        public float DischargeRate = 10f;
        // Charge Rate
        [KSPField(isPersistant = false)]
        public float ChargeRate;
        // Amount of Ec per storedCharge
        [KSPField(isPersistant = false)]
        public float ChargeRatio;
        // amount of maximumStoredCharge
        [KSPField(isPersistant = false)]
        public float MaximumCharge;

        [KSPField(isPersistant = false)]
        public string ChargeAnimation;


        // Capacitor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string CapacitorStatus;


        // Discharge capacitor
        [KSPEvent(guiActive = true, guiName = "Discharge Capacitor")]
        public void Discharge()
        {
            if (CurrentCharge > 0f)
            {
                Discharging = true;
            }
        }
        // charge on/off
        [KSPEvent(guiActive = true, guiName = "Enable Recharge", active = true)]
        public void Enable()
        {
            Enabled = true;
        }
        [KSPEvent(guiActive = true, guiName = "Disable Recharge", active = false)]
        public void Disable()
        {
           
            Enabled = false;
        }

        /// UI ACTIONS
        /// --------------------
        /// Toggle control panel
        [KSPEvent(guiActive = true, guiName = "Toggle Capacitor Control", active = true)]
        public void ShowCapacitorControl()
        {
            DischargeCapacitorUI.ToggleCapWindow();
        }
            

        [KSPAction("Discharge Capacitor")]
        public void DischargeAction(KSPActionParam param) { Discharge(); }

        [KSPAction("Enable Charging")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Disable Charging")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Charging")]
        public void ToggleAction(KSPActionParam param)
        {
            Enabled = !Enabled;
        }

        // Tweakable parameters
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Percent Power"), UI_FloatRange(minValue = 50f, maxValue = 100f , stepIncrement = 0.1f)]
        public float dischargeSlider = 100f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Adjusted Discharge Rate")]
        public float dischargeActual = 100f;

        private AnimationState[] capacityState;

        public override string GetInfo()
        {
            return String.Format("Maximum Discharge Rate: {0:F2}/s", DischargeRate) + "\n" + String.Format("Charge Rate: {0:F2}/s", ChargeRate) + "\n" + String.Format("Efficiency: {0:F2}%", ChargeRatio*100f);
        }



        

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            capacityState = Utils.SetUpAnimation(ChargeAnimation, this.part);

            foreach (AnimationState cState in capacityState)
            {
                cState.normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
            }
           

            

        }


        private void Update()
        {
            dischargeActual = (dischargeSlider / 100f) * DischargeRate;
        }

        public override void OnUpdate()
        {
   
            if (Events["Enable"].active == Enabled || Events["Disable"].active != Enabled)
            {
                Events["Disable"].active = Enabled;
                Events["Enable"].active = !Enabled;

           }
            
        }

        public float CurrentCharge
        {
            get
            {
                return (float)this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition("StoredCharge").id).amount;
            }
            
        }
        public override void OnFixedUpdate()
        {
            if (Discharging)
            {
                foreach (AnimationState cState in capacityState)
                {

                    cState.normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
                }


                float requested_amt = TimeWarp.fixedDeltaTime * (dischargeSlider/100f )* DischargeRate;

                // Don't try to insert more ElectricCharge than there was StoredCharge available
                float amt = this.part.RequestResource("StoredCharge", requested_amt);
                this.part.RequestResource("ElectricCharge", -amt);

                CapacitorStatus = String.Format("Discharging: {0:F2}/s", DischargeRate*(dischargeSlider / 100f));

                // if the amount returned is zero, disable discharging
                if (CurrentCharge <= 0f)
                {
                    Discharging = false;
                    
                }
            }
            else if (Enabled && CurrentCharge < MaximumCharge)
            {
                foreach (AnimationState cState in capacityState)
                {
                    cState.normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
                }
                double amt = this.part.RequestResource("ElectricCharge", TimeWarp.fixedDeltaTime * ChargeRate);

                if (amt > 0d)
                {
                    this.part.RequestResource("StoredCharge", -amt * ChargeRatio);
                    CapacitorStatus = String.Format("Recharging: {0:F2}/s", ChargeRate);
                }
                else
                {
                    CapacitorStatus = String.Format("Not enough ElectricCharge!");
                }
            }
            else if (CurrentCharge == 0f)
            {
                CapacitorStatus = "Discharged!";
            } else 
            {
                CapacitorStatus = String.Format("Ready");
            }


           
           
            
        }
        
       
        

    }
}
