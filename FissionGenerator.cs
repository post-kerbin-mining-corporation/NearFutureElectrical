/// FissionGenerator
/// ---------------------------------------------------
/// Fission Generator that uses EnrichedUranium to generate fuel
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionGenerator: PartModule
    {

        /// CONFIGURABLE FIELDS
        // ----------------------
        /// Fuel Resource name
        [KSPField(isPersistant = false)]
        public string fuelName = "EnrichedUranium";
        
        // Waste Resource Name
        [KSPField(isPersistant = false)]
        public string depletedName = "DepletedUranium";

        // Produced resource name
        [KSPField(isPersistant = false)]
        public string generatedName = "ElectricCharge";
        
        // Heat resource name
        [KSPField(isPersistant = false)]
        public string heatName = "SystemHeat";
        
        // Use a staging icon or not
        [KSPField(isPersistant = false)]
        public bool UseStagingIcon = true;

        // Force activate on load or not
        [KSPField(isPersistant = false)]
        public bool UseForcedActivation = true;

        // Is reactor online?
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Last FuelUpdate time
        [KSPField(isPersistant = true)]
        public float LastFuelUpdate = 0f;

        // Power resource genreated/s at maximum power
         [KSPField(isPersistant = false)]
        public float PowerGenerationMaximum;
        
        // Fuel burnt per second at maximum power
        [KSPField(isPersistant = false)]
        public float BurnRate;

        // Minumum power the reactor can run at (0-1.0)
        [KSPField(isPersistant = true)]
        public float MinPowerPercent;

        // Current reactor power setting (0-1.0)
        [KSPField(isPersistant = true)]
        public float CurrentPowerPercent = 1.0f;

        // State of the reactor safety limit
        [KSPField(isPersistant = true)]
        public bool SafetyLimit = true;

        // Thermal power of the reactor
        [KSPField(isPersistant = false)]
        public float ThermalPower;
        
        // Rate of thermal power response
        [KSPField(isPersistant = false)]
        public float ThermalPowerResponseRate;

        // Maximum reactor core temperature
        [KSPField(isPersistant = false)]
        public float MaxCoreTemperature;
        
        // Rate of core temperature response
        [KSPField(isPersistant = false)]
        public float CoreTemperatureResponseRate;
        
        // Current core temperature
        [KSPField(isPersistant = true)]
        public float CurrentCoreTemperature = 0f;
        
        // Meltdown core temperature
        [KSPField(isPersistant = false)]
        public float MeltdownCoreTemperature;
        
        // Current thermal power
        [KSPField(isPersistant = true)]
        public float currentThermalPower = 0f;

        // Overheating going on
        [KSPField(isPersistant = true)]
        public float overheatAmount = 0f;
        // Amount of damage to the core (1.0 = 100%)
        [KSPField(isPersistant = true)]
        public float CoreDamagePercent = 0f;

        /// Heat Dissipation in atmosphere
        // Amount of power dissipated w/ pressure in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve PressureCurve= new FloatCurve();
        
        // Amount of power dissipated w/ velocity in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve VelocityCurve = new FloatCurve();

       
        /// PRIVATE VARIABLES
        /// ----------------------
        // Current resource generation
        private double currentGeneration =0d;
        
        // Ratio of cur power to max power
        private double thermalPowerRatio = 0f;

        // Ratio of cur temp to max temp
        private double coreTemperatureRatio = 0d;
        
        // the info staging box
        private VInfoBox infoBox;

        // The wind curve
        private FloatCurve WindCurve = new FloatCurve();
        private SystemHeat.ModuleSystemHeat heatModule;


        /// ACTIONS
        // -------
        [KSPEvent(guiActive = true, guiName = "Startup Reactor", active = true)]
        public void StartReactor()
        {
            CurrentPowerPercent = 1.0f;
            Enabled = true;
        }
        [KSPEvent(guiActive = true, guiName = "Shutdown Reactor", active = false)]
        public void ShutdownReactor()
        {
            Enabled = false;
        }


        [KSPAction("Start Reactor")]
        public void StartReactorAction(KSPActionParam param)
        {
            StartReactor();
        }

        [KSPAction("Shutdown Reactor")]
        public void ShutdownReactorAction(KSPActionParam param)
        {
            ShutdownReactor();
        }

      
        
        /// UI BUTTONS
        /// --------------------
        /// Toggle control panel
        [KSPEvent(guiActive = true, guiName = "Toggle Reactor Control", active = true)]
        public void ShowReactorControl()
        {
            FissionGeneratorUI.ToggleWindow();
           
        }
        // try to refuel the reactor
        [KSPEvent(guiName = "Refuel Reactor", externalToEVAOnly = true, unfocusedRange = 2f, guiActiveUnfocused = true)]
        public void RefuelReactor()
        {
           this.TryRefuel();
        }

        /// UI FIELDS
        /// --------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Life")]
        public string FuelStatus;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Power Generated")]
        public string GeneratorStatus;

        // Core Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Integrity")]
        public string CoreStatus;

        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Maximum Power: {0:F2} Ec/s", PowerGenerationMaximum) + "\n" +
                String.Format("Heat Generated: {0:F2} kW", ThermalPower) + "\n" +
                "Estimated Core Life: " + FindTimeRemaining(BurnRate);
        }

       

        public double CoreTemperatureRatio
        {
            get { return coreTemperatureRatio; }
        }   
        public bool isHighlighted = false;
        public Color oldHighlight;

        public void ToggleHighlight()
        {
            if (isHighlighted)
            {
                isHighlighted = false;
                part.SetHighlightColor(oldHighlight);
                part.SetHighlightType(Part.HighlightType.OnMouseOver);
            }
            else
            {
                oldHighlight = part.highlightColor;
                isHighlighted = true;
                part.SetHighlightType(Part.HighlightType.AlwaysOn);
                part.SetHighlightColor(XKCDColors.RedOrange);
            }
            
            part.SetHighlightColor(XKCDColors.RedOrange);


        }


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }


        public override void OnStart(PartModule.StartState state)
        {
            if (UseStagingIcon)
                this.part.stagingIcon = "FUEL_TANK";
            else
                Utils.LogWarn("Fission Reactor: Staging Icon Disabled!");

            if (state != StartState.Editor)
            {
                // Get heat module
                heatModule = GetComponent<SystemHeat.ModuleSystemHeat>();

                // Set up staging icon heat bar
                if (UseStagingIcon)
                {
                    infoBox = this.part.stackIcon.DisplayInfo();
                    infoBox.SetMsgBgColor(XKCDColors.RedOrange);
                    infoBox.SetMsgTextColor(XKCDColors.Orange);
                    infoBox.SetLength(1.0f);
                    infoBox.SetMessage("CoreDamage");
                    infoBox.SetProgressBarBgColor(XKCDColors.RedOrange);
                    infoBox.SetProgressBarColor(XKCDColors.Orange);
                }
               

                if (UseForcedActivation)
                    this.part.force_activate();
                    
                // Perform a FuelUpdate
                FuelUpdate();
                oldHighlight = part.highlightColor;
            }

            // LogItAll();

        }


        private void FuelUpdate()
        {
            if (LastFuelUpdate == 0f)
            {
                Utils.Log("Fission Reactor: checking nonfocused use, no time elapsed.");
                return;
            }
            double timeElapsed = vessel.missionTime - (double)LastFuelUpdate;
            
            float fuelUsage = (float)(((CurrentCoreTemperature / MaxCoreTemperature)) * BurnRate * timeElapsed);
            double fuelAmt = this.part.RequestResource(fuelName, fuelUsage);
            this.part.RequestResource(depletedName, -fuelAmt);
            Utils.Log("Fission Reactor: checking nonfocused use, time unfocused is " + timeElapsed.ToString() + "s, removing " + fuelAmt.ToString() + " fuel"); 
        }


       

        // Do animation, UI
        public override void OnUpdate()
        {
            if ((Enabled && Events["StartReactor"].active) || (!Enabled && Events["ShutdownReactor"].active))
            {
                Events["StartReactor"].active = !Enabled;
                Events["ShutdownReactor"].active = Enabled;
            }
            // Update GUI 
            GeneratorStatus = String.Format("{0:F2} Ec/s", currentGeneration);
        }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                // First, add heat to ship
                float overheat = (float)heatModule.GenerateHeat((double)(currentThermalPower * TimeWarp.fixedDeltaTime));
                Utils.Log(overheat.ToString());
                float goalTemperature = 0f;
                float thermalGoal = 0f;
                double fuelUsage = 0d;
                // if the reactor is online
                if (Enabled)
                {
                    thermalGoal = ThermalPower * CurrentPowerPercent;
                    //Don't let thermal wattage go too high
                    if (SafetyLimit)
                    {
                        float curDelta = heatModule.VesselHeatBalance;
                        // If heat delta is positive, heat is accumulating
                        if (curDelta >= 0f)
                        {
                            // Reduce goal to the lower of thermalGoal and currentpower - the current delta
                            thermalGoal = Mathf.Clamp(
                                Mathf.Min(thermalGoal, currentThermalPower - curDelta*0.25f),
                                0f, ThermalPower);
                        }
                        else
                        {
                            thermalGoal = ThermalPower * CurrentPowerPercent;
                        }
                        //wattsGoal = Mathf.Min(ThermalPower * CurrentPowerPercent, wattsRadiated + wattsConvected);
                    }
                    // Allow thermal power to go to maximum
                    
                    //Split addition of resources into several calls, improves stability of high rates
                    this.part.RequestResource(generatedName, -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);
                    this.part.RequestResource(generatedName, -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);
                    this.part.RequestResource(generatedName, -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);
                    this.part.RequestResource(generatedName, -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);

                }
                else
                {
                    thermalGoal = 0f;
                }


                currentThermalPower = Mathf.MoveTowards(currentThermalPower, thermalGoal, TimeWarp.fixedDeltaTime * ThermalPowerResponseRate);
                thermalPowerRatio = (double)(currentThermalPower / ThermalPower);


                if (overheat > 0f)
                {
                    // increase the overheat amount
                    overheatAmount += TimeWarp.fixedDeltaTime * (overheat / (this.part.mass * 1000f));
                }
                else
                {
                    overheatAmount = Mathf.MoveTowards(overheatAmount, 0f, TimeWarp.fixedDeltaTime * Mathf.Max(((float)overheat / (this.part.mass * 1000f)), 1f));
                }
                //Debug.Log("Overheat total: " + overheatAmount.ToString());
                goalTemperature = (float)thermalPowerRatio * MaxCoreTemperature + Mathf.Clamp(overheatAmount, 0f, 5000f);

                CurrentCoreTemperature = Mathf.MoveTowards(CurrentCoreTemperature, goalTemperature, TimeWarp.fixedDeltaTime * CoreTemperatureResponseRate);
                coreTemperatureRatio = (double)(CurrentCoreTemperature / MaxCoreTemperature);

                // Reactor meltdown!
                if (CurrentCoreTemperature >= MeltdownCoreTemperature)
                {
                    CoreDamagePercent = 1f;
                    CoreStatus = "Complete Meltdown!";
                    if (Enabled)
                    {
                        ShutdownReactor();
                    }
                }
                else if (CurrentCoreTemperature >= MaxCoreTemperature)
                {
                    CoreDamagePercent = Mathf.Max(CoreDamagePercent, (CurrentCoreTemperature - MaxCoreTemperature * 1.15f) / (MeltdownCoreTemperature - MaxCoreTemperature * 1.15f));
                    if (CoreDamagePercent < 1f)
                        CoreStatus = String.Format("{0:F0} %", Mathf.Clamp01(1f - CoreDamagePercent) * 100f);
                }
                else
                {
                    if (CoreDamagePercent < 1f)
                        CoreStatus = String.Format("{0:F0} %", Mathf.Clamp01(1f - CoreDamagePercent) * 100f);
                }


                currentGeneration = thermalPowerRatio * PowerGenerationMaximum * (1f - CoreDamagePercent);

                if (UseStagingIcon)
                    infoBox.SetValue(CoreDamagePercent);


                FuelStatus = FindTimeRemaining(BurnRate * coreTemperatureRatio);

                // Compute and subtract the fuel usage
                fuelUsage = BurnRate * coreTemperatureRatio * TimeWarp.fixedDeltaTime;
                double fuelAmt = this.part.RequestResource(fuelName, fuelUsage);
                this.part.RequestResource(depletedName, -fuelAmt);

                if (fuelAmt <= 0d && fuelUsage > 0d)
                {
                    FuelStatus = "No fuel remaining";
                    ShutdownReactor();
                }

                // Set the last fuel update time
                LastFuelUpdate = (float)vessel.missionTime;

            }

        }


        // ####################################
        // Refuelling
        // ####################################


        // Tries to refeul the reactor
        void TryRefuel()
        {
            if (Enabled || CurrentCoreTemperature > 0f)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot refuel while reactor is running or hot!", 4f, ScreenMessageStyle.UPPER_CENTER));
                return;
            }
            else
            {
                Utils.Log("Fission Reactor: Searching for valid containers...");
                FissionContainer from = FindValidFissionContainer();
                if (from != null)
                {
                    Utils.Log("Fission Reactor: Refuelling valid container...");
                    from.RefuelReactorFromContainer(this, this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount);

                }
            }
        }

        // Finds a valid container
        FissionContainer FindValidFissionContainer()
        {

            List<FissionContainer> candidates = FindFissionContainers();

            foreach (FissionContainer cont in candidates)
            {
                // check for fuel space
                if (cont.CheckFuelSpace(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount))
                {
                    Utils.Log("Fission Reactor: Found valid FissionContainer.");
                    return cont;
                }
            }
            ScreenMessages.PostScreenMessage(new ScreenMessage("No fuel containers with enough space found!", 4f, ScreenMessageStyle.UPPER_CENTER));
            return null;
        }

        // Finds fission containers
        List<FissionContainer> FindFissionContainers()
        {
            List<FissionContainer> fissionContainers = new List<FissionContainer>();
            List<Part> allParts = this.vessel.parts;
            foreach (Part pt in allParts)
            {

                PartModuleList pml = pt.Modules;
                for (int i = 0; i < pml.Count; i++)
                {
                    PartModule curModule = pml.GetModule(i);
                    FissionContainer candidate = curModule.GetComponent<FissionContainer>();

                    if (candidate != null)
                        fissionContainers.Add(candidate);
                }

            }
            if (fissionContainers.Count == 0)
                ScreenMessages.PostScreenMessage(new ScreenMessage("No nuclear fuel containers attached to this ship.", 4f, ScreenMessageStyle.UPPER_CENTER));
            return fissionContainers;
        }
        // Finds time remaining at current fuel burn rates
        public string FindTimeRemaining(double rate)
        {
            if (rate <= 0.000000001d)
            {
                rate = BurnRate;
            }
            double remaining = (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount) / rate;

            

            TimeSpan t = TimeSpan.FromSeconds(remaining);
            
            if (remaining >= 0)
            {
                return Utils.FormatTimeString(remaining);
            }
            {
                return "No fuel remaining";
            }
        }

        public double GetCurrentPower()
        {
            return currentGeneration;
        }
    }
}
