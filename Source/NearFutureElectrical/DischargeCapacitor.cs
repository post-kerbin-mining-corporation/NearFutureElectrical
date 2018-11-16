/// DischargeCapacitor
/// ---------------------------------------------------
/// A module that discharges
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NearFutureElectrical.UI;
using KSP.Localization;

namespace NearFutureElectrical
{
    public class DischargeCapacitor : PartModule
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

        // amount of maximumStoredCharge
        [KSPField(isPersistant = false)]
        public float DischargeRateMinimumScalar = 0.5f;

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
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Discharge Capacitor")]
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

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Discharge Rate"), UI_FloatRange(minValue = 50f, maxValue = 100f, stepIncrement = 0.1f)]
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
            return Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_PartInfo", DischargeRate.ToString("F2"), ChargeRate.ToString("F2"), (ChargeRatio * 100f).ToString("F2"));
        }
        public string GetModuleTitle()
        {
            return "Capacitor";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_ModuleName");
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            capacityState = Utils.SetUpAnimation(ChargeAnimation, this.part);

            // Set up the UI slider
            var range = (UI_FloatRange)this.Fields["dischargeActual"].uiControlEditor;
            range.minValue = DischargeRate * DischargeRateMinimumScalar;
            range.maxValue = DischargeRate;

            range = (UI_FloatRange)this.Fields["dischargeActual"].uiControlFlight;
            range.minValue = DischargeRate * DischargeRateMinimumScalar;
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

            // Prepare the localization
            Events["ShowCapacitorControl"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_ToggleUI"));
            Events["Discharge"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Event_Discharge"));
            Events["Enable"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Event_EnableCharge"));
            Events["Disable"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Event_DisableCharge"));

            Actions["DischargeAction"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_Discharge"));
            Actions["EnableAction"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_EnableCharge"));
            Actions["DisableAction"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_DisableCharge"));
            Actions["ToggleAction"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_ToggleCharge"));
            Actions["TogglePanelAction"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Action_ToggleUI"));

            Fields["dischargeActual"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_DischargeRate"));
            Fields["CapacitorStatus"].guiName = string.Format("{0}", Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status"));

            if (HighLogic.LoadedSceneIsFlight)
            {
                DoCatchup();

                assignedGroups = new List<int>();
                for (int i = 0; i < CapacitorGroups.Length; i++)
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
                    Utils.Log(String.Format("Recharged capacitor in background: {0}", Planetarium.fetch.time - lastUpdateTime));
                    int ECID = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
                    double ec = 0d;
                    double outEc = 0d;
                    part.GetConnectedResourceTotals(ECID, out ec, out outEc, true);
                    if (ec / outEc >= 0.25d)
                    {
                        float amtScaled = Mathf.Clamp((float)(Planetarium.fetch.time - lastUpdateTime) * ChargeRate, 0f, MaximumCharge);

                        //Utils.Log(String.Format("Recharged: {0}", -amtScaled * ChargeRatio));
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

            dischargeActual = Mathf.Clamp(dischargeActual, DischargeRate * DischargeRateMinimumScalar, DischargeRate);

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

                double result = this.part.RequestResource("StoredCharge", amt);
                this.part.RequestResource("ElectricCharge", -result);

                CapacitorStatus = Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status_Discharging", dischargeActual.ToString("F2"));

                // if the amount returned is zero, disable discharging
                if (CurrentCharge <= 0.000001f)
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
                    CapacitorStatus = Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status_Charging", ChargeRate.ToString("F2"));
                }
                else
                {
                    CapacitorStatus = Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status_NoPower");
                }
                lastUpdateTime = Planetarium.fetch.time;
            }
            else if (CurrentCharge == 0f)
            {
                CapacitorStatus = Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status_Empty");
            }
            else
            {
                CapacitorStatus = Localizer.Format("#LOC_NFElectrical_ModuleDischargeCapacitor_Field_Status_Ready");
            }





        }




    }
}
