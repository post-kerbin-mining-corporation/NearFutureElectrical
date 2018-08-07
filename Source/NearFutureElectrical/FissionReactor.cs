/// FissionReactor
/// ---------------------------------------------------
/// Fission Generator!
///

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using NearFutureElectrical.UI;
using KSP.Localization;

namespace NearFutureElectrical
{
    public class FissionReactor : ModuleResourceConverter
    {
        public struct ResourceBaseRatio
        {
            public string ResourceName;
            public double ResourceRatio;

            public ResourceBaseRatio(string name, double ratio)
            {
                ResourceName = name;
                ResourceRatio = ratio;
            }
        }

        /// CONFIGURABLE FIELDS
        // ----------------------
        // The icon to use in the reactor UI
        [KSPField(isPersistant = true)]
        public int UIIcon = 1;

        // The
        [KSPField(isPersistant = true)]
        public string UIName = "";

        // Whether reactor power settings should follow the throttle setting
        [KSPField(isPersistant = false)]
        public bool FollowThrottle = false;

        // Engage safety override
        [KSPField(isPersistant = true, guiActive = true, guiName = "Auto-Shutdown Temp"), UI_FloatRange(minValue = 700f, maxValue = 6000f, stepIncrement = 100f)]
        public float CurrentSafetyOverride = 1000f;

        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float HeatGeneration;

        // Nominal reactor temperature (where the reactor should live)
        [KSPField(isPersistant = false)]
        public float NominalTemperature = 900f;

        // Critical reactor temperature (core damage after this)
        [KSPField(isPersistant = false)]
        public float CriticalTemperature = 1400f;

        // Critical reactor temperature (core damage after this)
        [KSPField(isPersistant = false)]
        public float MaximumTemperature = 2000f;

        // Current reactor power setting (0-100, tweakable)
        [KSPField(isPersistant = true, guiActive = true, guiName = "Power Setting"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float CurrentPowerPercent = 100f;

        // Actual reactor power setting used (0-100, read-only)
        [KSPField(isPersistant = false)]
        public float ActualPowerPercent = 100f;

        // amount of heating power available from reactor currently
        [KSPField(isPersistant = true)]
        public float AvailablePower = 0f;

        // Name of the fuel
        [KSPField(isPersistant = false)]
        public string FuelName = "EnrichedUranium";

        // name of the overheat animation
        [KSPField(isPersistant = false)]
        public string OverheatAnimation;

        [KSPField(isPersistant = false)]
        public int smoothingInterval = 25;

        [KSPField(isPersistant = true)]
        public bool TimewarpShutdown = false;

        [KSPField(isPersistant = true)]
        public int TimewarpShutdownFactor = 5;

        // REPAIR VARIABLES
        // integrity of the core
        [KSPField(isPersistant = true)]
        public float CoreIntegrity = 100f;

        // Rate the core is damaged, in % per S per K
        [KSPField(isPersistant = false)]
        public float CoreDamageRate = 0.005f;

        // Engineer level to repair the core
        [KSPField(isPersistant = false)]
        public int EngineerLevelForRepair = 5;

        [KSPField(isPersistant = false)]
        public float MaxRepairPercent = 75;

        [KSPField(isPersistant = false)]
        public float MinRepairPercent = 10;

        [KSPField(isPersistant = false)]
        public float MaxTempForRepair = 325;

        [KSPField(isPersistant = true)]
        public bool FirstLoad = true;

        /// UI ACTIONS
        /// --------------------
        /// Toggle control panel
        [KSPEvent(guiActive = true, guiName = "Toggle Reactor Control", active = true)]
        public void ShowReactorControl()
        {
            ReactorUI.ToggleReactorWindow();
        }
        [KSPAction("Toggle Reactor Panel")]
        public void TogglePanelAction(KSPActionParam param)
        {
            ShowReactorControl();
        }

        // Try to fix the reactor
        [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Repair Reactor")]
        public void RepairReactor()
        {
            if (TryRepairReactor())
            {
                DoReactorRepair();
            }
        }
        /// DEBUG
        /// ----------------------

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_RealHeatGen")]
        public string D_RealHeat;

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_RealConsumption")]
        public string D_RealConsumption;

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_PowerScale")]
        public string D_PowerScale;

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_TempScale")]
        public string D_TempScale;

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_FudgeCap")]
        public string D_FudgeCap;

        [KSPField(isPersistant = false, guiActive = false, guiName = "D_IsHeating")]
        public string D_IsHeating;

        /// PRIVATE VARIABLES
        /// ----------------------
        private ModuleCoreHeatNoCatchup core;

        private AnimationState[] overheatStates;

        // base paramters
        private List<ResourceBaseRatio> inputs;
        private List<ResourceBaseRatio> outputs;

        private FloatCurve throttleCurve;

        private FissionEngine reactorEngine;

        /// UI FIELDS
        /// --------------------


        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Power")]
        public string ReactorOutput;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Available Power")]
        public string ThermalTransfer;

        // integrity of the core
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Temperature")]
        public string CoreTemp;

        // integrity of the core
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Health")]
        public string CoreStatus;

        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Life")]
        public string FuelStatus;

        // Sets whether auto-shutdown is possible
        public ModuleCoreHeatNoCatchup Core { get { return core; } }

        // Sets whether time wwarp shutdown is enabled
        public void SetTimewarpShutdownStatus(bool status)
        {

        }

        public override string GetInfo()
        {
            double baseRate = 0d;
            for (int i = 0; i < inputList.Count; i++)
            {
                if (inputList[i].ResourceName == FuelName)
                    baseRate = inputList[i].Ratio;
            }
            return
                Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_PartInfo",
                (HeatGeneration / 50f).ToString("F0"),
                NominalTemperature.ToString("F0"),
                CriticalTemperature.ToString("F0"),
                MaximumTemperature.ToString("F0"),
                FindTimeRemaining(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount, baseRate))
                + base.GetInfo();
        }
        public string GetModuleTitle()
        {
            return "FissionReactor";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_ModuleName");
        }
        private void SetupCore()
        {
            if (core == null)
            {
                Utils.LogError("Fission Reactor: Could not find core heat module!");
            }
            else
            {

            }


        }
        private void SetupResourceRatios()
        {

            inputs = new List<ResourceBaseRatio>();
            outputs = new List<ResourceBaseRatio>();

            for (int i = 0; i < inputList.Count; i++)
            {
                inputs.Add(new ResourceBaseRatio(inputList[i].ResourceName, inputList[i].Ratio));
            }
            for (int i = 0; i < outputList.Count; i++)
            {
                outputs.Add(new ResourceBaseRatio(outputList[i].ResourceName, outputList[i].Ratio));
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
        }


        public override void OnUpdate()
        {
            base.OnUpdate();

        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

        }
        public void OverriddenAwake()
        {
        }
        public void OverriddenStart()
        {
            var range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlEditor;
            range.minValue = 0f;
            range.maxValue = MaximumTemperature;

            range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlFlight;
            range.minValue = 0f;
            range.maxValue = MaximumTemperature;

            throttleCurve = new FloatCurve();
            throttleCurve.Add(0, 0, 0, 0);
            throttleCurve.Add(50, 20, 0, 0);
            throttleCurve.Add(100, 100, 0, 0);

            Actions["TogglePanelAction"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Action_TogglePanelAction");

            Events["ShowReactorControl"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Event_ShowReactorControl");
            Events["RepairReactor"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Event_RepairReactor");

            Fields["CurrentSafetyOverride"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CurrentSafetyOverride");
            Fields["CurrentPowerPercent"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CurrentPowerPercent");
            Fields["ReactorOutput"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput");
            Fields["ThermalTransfer"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ThermalTransfer");
            Fields["CoreTemp"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CoreTemp");
            Fields["CoreStatus"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CoreStatus");
            Fields["FuelStatus"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus");

            if (base.ModuleIsActive())
                activeFlag = true;
            else
                activeFlag = false;

            if (FirstLoad)
            {
                this.CurrentSafetyOverride = this.CriticalTemperature;
                FirstLoad = false;
            }

            if (HighLogic.LoadedScene != GameScenes.EDITOR)
            {
                core = this.GetComponent<ModuleCoreHeatNoCatchup>();



                SetupCore();
                SetupResourceRatios();

                if (OverheatAnimation != "")
                    overheatStates = Utils.SetUpAnimation(OverheatAnimation, this.part);

                if (FollowThrottle)
                    reactorEngine = this.GetComponent<FissionEngine>();

            }
            else
            {
                //this.CurrentSafetyOverride = this.NominalTemperature;
            }
        }
        public void OverriddenUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                foreach (BaseField fld in base.Fields)
                {
                    if (fld.name == "status")
                        fld.guiActive = false;

                }
                if (core != null)
                {
                    core.CoreShutdownTemp = (double)CurrentSafetyOverride + 10d;

                }
            }
        }

        bool activeFlag = false;
        int heatTicker = 0;
        public void OverriddenFixedUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (UIName == "")
                    UIName = part.partInfo.title;
                if (FollowThrottle)
                {
                    if (reactorEngine != null && reactorEngine.EngineActive())
                        ActualPowerPercent = Math.Max(throttleCurve.Evaluate(100 * this.vessel.ctrlState.mainThrottle * reactorEngine.GetThrustLimiterFraction()), CurrentPowerPercent);
                    else
                        ActualPowerPercent = CurrentPowerPercent;
                }
                else
                {
                    ActualPowerPercent = CurrentPowerPercent;

                }

                // Update reactor core integrity readout
                if (CoreIntegrity > 0)
                    CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
                else
                    CoreStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CoreStatus_Meltdown");


                // Handle core damage tracking and effects
                HandleCoreDamage();
                // Heat consumption occurs if reactor is on or off
                DoHeatConsumption();

                // IF REACTOR ON
                // =============
                if (base.ModuleIsActive())
                {
                    if (TimewarpShutdown && TimeWarp.fetch.current_rate_index >= TimewarpShutdownFactor)
                        ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));
                    if (base.ModuleIsActive() != activeFlag)
                    {
                        base.lastUpdateTime = Planetarium.GetUniversalTime();
                        heatTicker = 60;
                        activeFlag = true;
                        // Debug.Log("Turned On");
                    }

                    DoFuelConsumption();
                    DoHeatGeneration();

                }
                // IF REACTOR OFF
                // =============
                else
                {
                    if (base.ModuleIsActive() != activeFlag)
                    {
                        activeFlag = false;
                        ZeroThermal();
                        //Debug.Log("Turned Off");
                    }

                    // Update UI
                    if (CoreIntegrity <= 0f)
                    {
                        FuelStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Meltdown");
                        ReactorOutput = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput_Meltdown");
                    }
                    else
                    {
                        FuelStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Offline");
                        ReactorOutput = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput_Offline");

                    }
                }


            }
        }



        private void DoFuelConsumption()
        {
            if (_recipe != null && _recipe.Inputs != null)
            {

                // Get current resource consumption
                double rate = 0d;
                for (int i = 0; i < inputList.Count; i++)
                {
                    if (_recipe.Inputs[i].ResourceName == FuelName)
                        rate = _recipe.Inputs[i].Ratio;
                }
                // Recalculate fuel use Ratio
                // Fuel use is proportional to power setting
                RecalculateRatios(ActualPowerPercent / 100f);

                // Find the time remaining at current rate
                FuelStatus = FindTimeRemaining(
                  this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,
                  rate);
            }
        }

        // Creates heat from the reaction
        private void DoHeatGeneration()
        {
            // Generate heat from the reaction and apply it
            SetHeatGeneration((ActualPowerPercent / 100f * HeatGeneration) * CoreIntegrity / 100f);

            if (CoreIntegrity <= 0f)
            {
                FuelStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Meltdown");
                ReactorOutput = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput_Meltdown");
            }
            else
            {
                ReactorOutput = String.Format("{0:F1} {1}", ActualPowerPercent / 100f * HeatGeneration / 50f * CoreIntegrity / 100f, Localizer.Format("#LOC_NFElectrical_Units_kW"));
            }
        }

        private void DoHeatConsumption()
        {
            // save some divisions later
            float coreIntegrity = CoreIntegrity / 100f;
            float reactorThrottle = ActualPowerPercent / 100f;
            if (!base.ModuleIsActive())
                reactorThrottle = 0f;
            float maxHeatGenerationKW = HeatGeneration / 50f;

            // The core temperature where no generation is possible
            float zeroPoint = (float)part.temperature;

            // The core temperature where maximum generation is possible
            float maxPoint = NominalTemperature;

            float temperatureDiff = Mathf.Clamp((float)core.CoreTemperature - zeroPoint, 0f, NominalTemperature);

            // The fraction of generation that is possible.
            float curTempScale = Mathf.Clamp(temperatureDiff / (maxPoint - zeroPoint), 0f, 1f);

            // Fraction showing amount of power available to
            float powerScale = Mathf.Min(reactorThrottle, curTempScale) * coreIntegrity;

            AvailablePower = powerScale * maxHeatGenerationKW;

            // Allocate power to generators/engines
            if (float.IsNaN(AvailablePower))
                AvailablePower = 0f;

            AllocateThermalPower();

            // GUI
            ThermalTransfer = String.Format("{0:F1} {1}", AvailablePower, Localizer.Format("#LOC_NFElectrical_Units_kW"));
            CoreTemp = String.Format("{0:F1}/{1:F1} {2}", (float)core.CoreTemperature, NominalTemperature, Localizer.Format("#LOC_NFElectrical_Units_K"));

            D_TempScale = String.Format("{0:F4}", curTempScale);
            D_PowerScale = String.Format("{0:F4}", powerScale);
        }

        private void AllocateThermalPower()
        {
            List<FissionConsumer> consumers = GetOrderedConsumers();
            //Utils.Log("FissionReactor: START CYCLE: has " + AvailablePower.ToString() +" kW to distribute");
            float remainingPower = AvailablePower;
            // Iterate through all consumers and allocate available thermal power
            for (int i = 0; i < consumers.Count; i++)
            {
                if (consumers[i].Status)
                {
                    remainingPower = consumers[i].ConsumeHeat(remainingPower);
                    //totalWaste = totalWaste + consumer.GetWaste();

                    if (remainingPower <= 0f)
                        remainingPower = 0f;
                    //Utils.Log ("FissionReactor: Consumer left "+ remainingPower.ToString()+ " kW");
                }
            }
        }

        private List<FissionConsumer> GetOrderedConsumers()
        {
            List<FissionConsumer> consumers = this.GetComponents<FissionConsumer>().ToList();
            return consumers.OrderBy(o => o.Priority).ToList();
        }

        private void SetHeatGeneration(float heat)
        {
            if (Time.timeSinceLevelLoad > 1f)
                GeneratesHeat = true;
            else
                GeneratesHeat = false;

            if (heatTicker <= 0)
            {
                TemperatureModifier = new FloatCurve();
                TemperatureModifier.Add(0f, heat);
            }
            else
            {
                ZeroThermal();
                heatTicker = heatTicker - 1;
            }
            core.MaxCoolant = heat / 10f;
        }

        private void ZeroThermal()
        {
            base.lastHeatFlux = 0d;
            core.ZeroThermal();
            base.GeneratesHeat = false;
            TemperatureModifier = new FloatCurve();
            TemperatureModifier.Add(0f, 0f);
        }

        // track and set core damage
        private void HandleCoreDamage()
        {
            // Update reactor damage
            float critExceedance = (float)core.CoreTemperature - CriticalTemperature;

            // If overheated too much, damage the core
            if (critExceedance > 0f && TimeWarp.CurrentRate < 100f)
            {
                // core is damaged by Rate * temp exceedance * time
                CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
            }

            // Calculate percent exceedance of nominal temp
            float tempNetScale = 1f - Mathf.Clamp01((float)((core.CoreTemperature - NominalTemperature) / (MaximumTemperature - NominalTemperature)));


            if (OverheatAnimation != "")
            {
                for (int i = 0; i < overheatStates.Length; i++)

                {
                    overheatStates[i].normalizedTime = 1f - tempNetScale;
                }
            }
        }

        // Set ModuleResourceConverter ratios based on an input scale
        private void RecalculateRatios(float fuelInputScale)
        {

            for (int i = 0; i < _recipe.Inputs.Count; i++)
            {
                for (int j = 0; j < inputs.Count; j++)
                {
                    if (inputs[j].ResourceName == inputList[i].ResourceName)
                    {
                        _recipe.Inputs[i] = new ResourceRatio(inputList[i].ResourceName, inputs[j].ResourceRatio * fuelInputScale, inputList[i].DumpExcess);

                    }
                }
            }
            for (int i = 0; i < _recipe.Outputs.Count; i++)
            {
                for (int j = 0; j < outputs.Count; j++)
                {
                    if (outputs[j].ResourceName == outputList[i].ResourceName)
                    {
                        //Debug.Log("OUT: edited " + outputList[i].ResourceName + " ratio to " + (outputs[j].ResourceRatio * fuelInputScale).ToString());
                        _recipe.Outputs[i] = new ResourceRatio(outputList[i].ResourceName, inputs[j].ResourceRatio * fuelInputScale, outputList[i].DumpExcess);
                    }
                }
            }
            for (int i = 0; i < inputList.Count; i++)
            {
                //Debug.Log("IN: edited " + inputList[i].ResourceName + " ratio to " + (inputList[i].Ratio).ToString());
            }
        }



        // ####################################
        // Repairing
        // ####################################

        public bool TryRepairReactor()
        {
            if (CoreIntegrity <= MinRepairPercent)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooDamaged"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (!CheckEVAEngineerLevel(EngineerLevelForRepair))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooDamaged", EngineerLevelForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (base.ModuleIsActive())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_NotWhileRunning"),
                    5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (core.CoreTemperature > MaxTempForRepair)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooHot", MaxTempForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (CoreIntegrity >= MaxRepairPercent)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreAlreadyRepaired", MaxRepairPercent.ToString("F0")),
                    5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            return true;
        }

        // Repair the reactor to max Repair percent
        public void DoReactorRepair()
        {
            this.CoreIntegrity = MaxRepairPercent;
            ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_RepairSuccess", MaxRepairPercent.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        }

        // Check the current EVA engineer's level
        private bool CheckEVAEngineerLevel(int level)
        {
            ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.GetVesselCrew()[0];
            if (kerbal.experienceTrait.TypeName == Localizer.Format("#autoLOC_500103") && kerbal.experienceLevel >= level)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float GetCoreTemperature()
        {
            return (float)core.CoreTemperature;
        }

        // ####################################
        // Refuelling
        // ####################################

        // Finds time remaining at specified fuel burn rates
        public string FindTimeRemaining(double amount, double rate)
        {
            if (rate < 0.0000001)
            {
                return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_VeryLong");
            }
            double remaining = amount / rate;
            //TimeSpan t = TimeSpan.FromSeconds(remaining);

            if (remaining >= 0)
            {
                return Utils.FormatTimeString(remaining);
            }
            {
                return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Exhausted");
            }
        }
    }
}
