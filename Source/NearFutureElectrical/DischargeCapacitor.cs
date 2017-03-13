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
        // The trigger group this capacitor belongs to
        [KSPField(isPersistant = true)]
        public string CapacitorGroups = "0";

        // Is capacitor online
        [KSPField(isPersistant = true)]
        public bool Enabled;
        // Is capacitor discharging
        [KSPField(isPersistant = true)]
        public bool Discharging;

        // Does discharge generate heat?
        [KSPField(isPersistant = false)]
        public bool DischargeGeneratesHeat = false;

        // Heat generated
        [KSPField(isPersistant = false)]
        public float HeatRate = 0f;

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

        [KSPField(isPersistant = true)]
        public bool FirstLoad = true;

        [KSPField(isPersistant = true)]
        public double lastUpdateTime = 0;

        private AnimationState[] capacityState;
        private List<int> assignedGroups = new List<int>();

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
        [KSPAction("Toggle Capacitor Panel")]
        public void TogglePanelAction(KSPActionParam param)
        {
            ShowCapacitorControl();
        }

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Discharge Rate"), UI_FloatRange(minValue = 50f, maxValue = 100f , stepIncrement = 0.1f)]
        public float dischargeActual = 100f;



        public bool GetGroupMembership(int toTest)
        {
          if (assignedGroups.Contains(toTest))
            return true;
          return false;
        }

        public void AssignToGroup(int toAssign)
        {
          if (!assignedGroups.Contains(toAssign))
            assignedGroups.Add(toAssign);
        }
        public void RemoveFromGroup(int toRemove)
        {
          if (assignedGroups.Contains(toRemove))
            assignedGroups.Remove(toRemove);
        }

        public override string GetInfo()
        {
            return String.Format("Maximum Discharge Rate: {0:F2}/s", DischargeRate) + "\n" + String.Format("Charge Rate: {0:F2}/s", ChargeRate) + "\n" + String.Format("Efficiency: {0:F2}%", ChargeRatio*100f);
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            capacityState = Utils.SetUpAnimation(ChargeAnimation, this.part);

            // Set up the UI slider
            var range = (UI_FloatRange)this.Fields["dischargeActual"].uiControlEditor;
            range.minValue = DischargeRate/2f;
            range.maxValue = DischargeRate;

            range = (UI_FloatRange)this.Fields["dischargeActual"].uiControlFlight;
            range.minValue = DischargeRate/2f;
            range.maxValue = DischargeRate;

            // Set up the discharge rate
            if (FirstLoad)
            {
              this.dischargeActual = DischargeRate;
              FirstLoad = false;
            }

            for (int i = 0; i < capacityState.Length; i++)
            {
                capacityState[i].normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
              DoCatchup();

              assignedGroups = new List<int>();
              for (int i =0; i< CapacitorGroups.Length ;i++)
              {
                assignedGroups.Add((int)CapacitorGroups[i]);
              }
            }

        }
        private void DoCatchup()
        {
          if (lastUpdateTime < Planetarium.fetch.time)
          {
            if (Enabled && !Discharging)
            {
                Utils.Log(String.Format("Recharged capacitor in background: {0}", Planetarium.fetch.time -lastUpdateTime));
              int ECID = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
              double ec = 0d;
              double outEc = 0d;
              part.GetConnectedResourceTotals(ECID, out ec, out outEc, true);
              if (ec / outEc >= 0.25d)
                {
                  float amtScaled = Mathf.Clamp((float)( Planetarium.fetch.time -lastUpdateTime) * ChargeRate, 0f, MaximumCharge);

                  Utils.Log(String.Format("Recharged: {0}", -amtScaled * ChargeRatio));
                  this.part.RequestResource("StoredCharge", -amtScaled * ChargeRatio, ResourceFlowMode.NO_FLOW);

                }
            }
          }
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
                for (int i = 0; i < capacityState.Length; i++)
                {

                    capacityState[i].normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
                }


                float amt = TimeWarp.fixedDeltaTime * dischargeActual;

                if (DischargeGeneratesHeat && TimeWarp.CurrentRate <= 100f)
                {
                    this.part.AddThermalFlux((double)HeatRate);
                }

                this.part.RequestResource("StoredCharge", amt);
                this.part.RequestResource("ElectricCharge", -amt);

                CapacitorStatus = String.Format("Discharging: {0:F2}/s", dischargeActual);

                // if the amount returned is zero, disable discharging
                if (CurrentCharge <= 0f)
                {
                    Discharging = false;

                }
            }
            else if (Enabled && CurrentCharge < MaximumCharge)
            {
                for (int i = 0; i < capacityState.Length; i++)
                {
                    capacityState[i].normalizedTime = 1 - (-CurrentCharge / MaximumCharge);
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
                lastUpdateTime = Planetarium.fetch.time;
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
