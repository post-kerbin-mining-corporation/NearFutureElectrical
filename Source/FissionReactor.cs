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

namespace NearFutureElectrical
{
    public class FissionReactor: ModuleResourceConverter
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

        // Whether to use a staging icon or not
        [KSPField(isPersistant = false)]
        public bool UseStagingIcon = true;

        // Force activate on load or not
        [KSPField(isPersistant = false)]
        public bool UseForcedActivation = true;

        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float HeatGeneration;

        // Nominal reactor temperature (where the reactor should live)
        [KSPField(isPersistant = false)]
        public float NominalTemperature = 900f;

        // Critical reactor temperature (core damage after this)
        [KSPField(isPersistant = false)]
        public float CriticalTemperature = 1400f;

        // Current reactor power setting (0-100, tweakable)
        [KSPField(isPersistant = true, guiActive = true, guiName = "Power Setting"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float CurrentPowerPercent = 50f;

        // Curve relating available power to temperature. Generally should be of the form
        // AmbientTemp  0
        // NominalTemp RatedReactorOutput
        // MaxTemp BonusReactorOutput
        [KSPField(isPersistant = false)]
        public FloatCurve PowerCurve = new FloatCurve();

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

        // Try to fix the reactor
        [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Repair Reactor")]
        public void RepairReactor()
        {
            if (TryRepairReactor())
            {
              DoReactorRepair();
            }
        }

        /// PRIVATE VARIABLES
        /// ----------------------
        // the info staging box
        private VInfoBox infoBox;

        private ModuleCoreHeat core;

        private AnimationState[] overheatStates;

        // base paramters
        private List<ResourceBaseRatio> inputs;
        private List<ResourceBaseRatio> outputs;

        /// UI FIELDS
        /// --------------------


        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Output")]
        public string ReactorOutput;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Heat Transf")]
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

        public override string GetInfo()
        {
            double baseRate = 0d;
            foreach (ResourceRatio input in inputList)
            {
                if (input.ResourceName == FuelName)
                    baseRate = input.Ratio;
            }
            return base.GetInfo()
                + String.Format("Optimal Temperature: {0:F0} K", NominalTemperature) + "\n"
                + String.Format("Critical Temperature: {0:F0} K", CriticalTemperature) + "\n"
                + "Estimated Core Life: " +
                FindTimeRemaining(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,baseRate) ;
        }

        private void SetupResourceRatios()
        {

            inputs = new List<ResourceBaseRatio>();
            outputs = new List<ResourceBaseRatio>();

            foreach (ResourceRatio input in inputList)
            {
                inputs.Add(new ResourceBaseRatio(input.ResourceName, input.Ratio));
            }
            foreach (ResourceRatio output in outputList)
            {
                outputs.Add(new ResourceBaseRatio(output.ResourceName, output.Ratio));
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            
            if (UseStagingIcon)
                this.part.stagingIcon = "FUEL_TANK";
            else
                Utils.LogWarn("Fission Reactor: Staging Icon Disabled!");

            if (state != StartState.Editor)
            {
                
                core = this.GetComponent<ModuleCoreHeat>();
                if (core == null)
                    Utils.LogError("Fission Reactor: Could not find core heat module!");

               
                

                SetupResourceRatios();
                // Set up staging icon heat bar
                if (UseStagingIcon)
                {
                    infoBox = this.part.stackIcon.DisplayInfo();
                    infoBox.SetMsgBgColor(XKCDColors.RedOrange);
                    infoBox.SetMsgTextColor(XKCDColors.Orange);
                    infoBox.SetLength(1.0f);
                    infoBox.SetValue(0.0f);
                    infoBox.SetMessage("Meltdwn");
                    infoBox.SetProgressBarBgColor(XKCDColors.RedOrange);
                    infoBox.SetProgressBarColor(XKCDColors.Orange);
                }

                if (OverheatAnimation != "")
                {
                    overheatStates = Utils.SetUpAnimation(OverheatAnimation, this.part);
                }
                if (UseForcedActivation)
                    this.part.force_activate();
            }
            base.OnStart(state);
        }



        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {

                // Update reactor core integrity readout
                if (CoreIntegrity > 0)
                    CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
                else
                    CoreStatus = "Complete Meltdown";


                // Handle core damage tracking and effects
                HandleCoreDamage();
                // Heat consumption occurs if reactor is on or off
                DoHeatConsumption();

                // IF REACTOR ON
                // =============
                if (base.ModuleIsActive())
                {
                  DoFuelConsumption();
                  DoHeatGeneration();

                }
                // IF REACTOR OFF
                // =============
                else
                {
                    // Update UI
                    if (CoreIntegrity <= 0f)
                    {
                        FuelStatus = "Core Destroyed";
                        ReactorOutput = "Core Destroyed";
                    }
                    else
                    {
                        FuelStatus = "Reactor Offline";
                        ReactorOutput = "Reactor Offline";

                    }
                }

            }
        }

        private void DoFuelConsumption()
        {
          // Get current resource consumption
          double rate = 0d;
          foreach (ResourceRatio input in inputList)
          {
            if (input.ResourceName == FuelName)
                rate = input.Ratio;
          }
          // Recalculate fuel use Ratio
          // Fuel use is proportional to power setting
          RecalculateRatios(CurrentPowerPercent / 100f );

          // Find the time remaining at current rate
          FuelStatus = FindTimeRemaining(
            this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,
            rate);
        }

        // Creates heat from the reaction
        private void DoHeatGeneration()
        {
            // Generate heat from the reaction and apply it
            SetHeatGeneration((CurrentPowerPercent / 100f * HeatGeneration)* CoreIntegrity/100f);

            if (CoreIntegrity <= 0f)
            {
                FuelStatus = "Core Destroyed";
                ReactorOutput = "Core Destroyed";
            }
            else
            {
                ReactorOutput = String.Format("{0:F0} kW", CurrentPowerPercent / 100f * HeatGeneration / 50f * CoreIntegrity / 100f);
            }
        }

        List<float> availablePowerList = new List<float>();
        float reactorFudgeFactor = 0f;

        private void DoHeatConsumption()
        {
            // determine the maximum radiator cooling
            // At temperature 0, no cooling is possible
            // at nominal temperature, full cooling is possible
            float partTemperatureDiff = (float)core.CoreTemperature-(float)part.temperature;
           
            float temperatureRatio = Mathf.Clamp(partTemperatureDiff/(NominalTemperature - (float)part.temperature),0f,1.0f);
            float maxRadiatorCooling = Mathf.Clamp(
                 temperatureRatio * HeatGeneration/50f,
                0f,
                HeatGeneration / 50f);


            //Utils.Log("MaxCool: " + maxRadiatorCooling.ToString());

            // Determine power available to transfer to components
            float frameAvailablePower = 0f;
            if (Single.TryParse(core.D_CoolAmt, out frameAvailablePower))
            {
                availablePowerList.Add(frameAvailablePower/ Mathf.Clamp(TimeWarp.CurrentRate,0f,(float)core.MaxCalculationWarp));
                if (availablePowerList.Count > smoothingInterval)
                {
                    availablePowerList.RemoveAt(0);
                }
            }
            float smoothedPower = ListMean(availablePowerList);


            // The reactor fudge factor is a number by which we increase the reactor power to pretend radiators are
            // transferring less at low temperatures
            reactorFudgeFactor =  Mathf.Clamp(smoothedPower - maxRadiatorCooling,0f,(float)core.MaxCoolant);


            AvailablePower = Mathf.Clamp(smoothedPower,0f, maxRadiatorCooling);
            //Utils.Log("MeanPower: " + ListMean(availablePowerList));

            ThermalTransfer = String.Format("{0:F2} kW", AvailablePower);
            CoreTemp = String.Format("{0:F1}/{1:F1} K", (float)core.CoreTemperature, NominalTemperature);

            core.CoreTempGoalAdjustment = -core.CoreTempGoal;

           // Debug.Log(core.D_CoolAmt + core.D_CoolPercent+core.D_coreXfer+core.D_CTE+ core.D_EDiff+
            //    core.D_Excess+ core.D_GE+ core.D_partXfer+ core.D_POT+ core.D_PTE+ core.D_RadCap+ core.D_RadSat+ core.D_RCA+ core.D_TRU+ core.D_XTP);
            // Get the list of all modules on the part that can consume heat
            List<FissionConsumer> consumers = GetOrderedConsumers();

            //Utils.Log("FissionReactor: START CYCLE: has " + AvailablePower.ToString() +" kW to distribute");

            float remainingPower = AvailablePower;
            // Iterate through all consumers and allocate available thermal power
            foreach (FissionConsumer consumer in consumers)
            {
                if (consumer.Status)
                {
                    remainingPower = consumer.ConsumeHeat(remainingPower);
                    //totalWaste = totalWaste + consumer.GetWaste();

                    if (remainingPower <= 0f)
                        remainingPower = 0f;
                    //Utils.Log ("FissionReactor: Consumer left "+ remainingPower.ToString()+ " kW");
                }
            }
            //Utils.Log ("FissionReactor: END CYCLE with "+totalWaste.ToString() + " waste, and " + AvailablePower.ToString() +" spare");
        }

        // Get the mean of a list
        private float ListMean(List<float> theList)
        {
            float sum = 0f;
            foreach (float i in theList)
            {
                sum += i;
            }
            return sum / (float)theList.Count;
        }

        private List<FissionConsumer> GetOrderedConsumers()
        {
          List<FissionConsumer> consumers = this.GetComponents<FissionConsumer>().ToList();
          return consumers.OrderBy(o=>o.Priority).ToList();
        }

        private void SetHeatGeneration(float heat)
        {
            //Utils.Log("Fudge Factor currently " + reactorFudgeFactor.ToString());
            if (float.IsNaN(reactorFudgeFactor))
            {
                reactorFudgeFactor = 0f;
            }
          TemperatureModifier = new FloatCurve();
           
          TemperatureModifier.Add(0f, heat + reactorFudgeFactor * 50f);
        }





        // track and set core damage
        private void HandleCoreDamage()
        {
          // Update reactor damage
          float critExceedance = (float)core.CoreTemperature - CriticalTemperature;

          // If overheated too much, damage the core
          if (critExceedance > 0f)
          {
              // core is damaged by Rate * temp exceedance * time
              CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
          }

          // Calculate percent exceedance of nominal temp
          float tempNetScale = 1f - Mathf.Clamp01((float)((core.CoreTemperature - NominalTemperature) / (part.maxTemp - NominalTemperature)));

          // update staging bar if in use
          if (UseStagingIcon)
              infoBox.SetValue(1f-tempNetScale);

          if (OverheatAnimation != "")
          {
              foreach (AnimationState cState in overheatStates)
              {
                  cState.normalizedTime = 1f - tempNetScale;
              }
          }
        }

        // Set ModuleResourceConverter ratios based on an input scale
        private void RecalculateRatios(float fuelInputScale)
        {
            foreach (ResourceRatio input in inputList)
            {
                foreach (ResourceBaseRatio baseInput in inputs)
                {
                    if (baseInput.ResourceName == input.ResourceName)
                    {
                        input.Ratio = baseInput.ResourceRatio * fuelInputScale;
                    }
                }
            }
            foreach (ResourceRatio output in outputList)
            {
                foreach (ResourceBaseRatio baseOutput in outputs)
                {
                    if (baseOutput.ResourceName == output.ResourceName)
                    {
                          output.Ratio = baseOutput.ResourceRatio * fuelInputScale;
                    }
                }
            }
        }


        // ####################################
        // Repairing
        // ####################################

        public bool TryRepairReactor()
        {
          if (CoreIntegrity <= MinRepairPercent)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage("Reactor core is too damaged to repair.", 5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (!CheckEVAEngineerLevel(EngineerLevelForRepair))
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Reactor core repair requires a Level {0:F0} Engineer."), 5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (base.ModuleIsActive())
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot repair reactor core while running! Seriously!",
                  5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (core.CoreTemperature > MaxTempForRepair)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("The reactor must be below {0:F0} K to initiate repair!", MaxTempForRepair), 5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (CoreIntegrity >= MaxRepairPercent)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Reactor core is already at maximum field repairable integrity ({0:F0})", MaxRepairPercent),
                  5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          return true;
        }

        // Repair the reactor to max Repair percent
        public void DoReactorRepair()
        {
            this.CoreIntegrity = MaxRepairPercent;
            ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Reactor repaired to {0:F0}%!", MaxRepairPercent), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        }

        // Check the current EVA engineer's level
        private bool CheckEVAEngineerLevel(int level)
        {
            ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (kerbal.experienceTrait.Title == "Engineer" && kerbal.experienceLevel >= level)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ####################################
        // Refuelling
        // ####################################

        // Finds time remaining at specified fuel burn rates
        public string FindTimeRemaining(double amount, double rate)
        {
            if (rate < 0.0000001)
            {
                return "A long time!";
            }
            double remaining = amount / rate;
            //TimeSpan t = TimeSpan.FromSeconds(remaining);

            if (remaining >= 0)
            {
                return Utils.FormatTimeString(remaining);
            }
            {
                return "No fuel remaining";
            }
        }
    }
}
