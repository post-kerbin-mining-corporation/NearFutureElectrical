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


        /// KSPFIELD
        /// 
        [KSPField(isPersistant = false)]
        public string fuelName = "EnrichedUranium";
        [KSPField(isPersistant = false)]
        public string depletedName = "DepletedUranium";

        // Use a staging icon
        [KSPField(isPersistant = false)]
        public bool UseStagingIcon = true;
        // Force activate on load
        [KSPField(isPersistant = false)]
        public bool UseForcedActivation = true;

        // Is reactor online?
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // LastFuelUpdate
        [KSPField(isPersistant = true)]
        public float LastFuelUpdate = 0f;

        // Maximum power generation
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

        // Is the safety limit on?
        [KSPField(isPersistant = true)]
        public bool SafetyLimit = true;

        // Amount of power radiation required
        [KSPField(isPersistant = false)]
        public float ThermalPower;
        // Rate of thermal power response
        [KSPField(isPersistant = false)]
        public float ThermalPowerResponseRate;

        // Maximum core temperature
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

        [KSPField(isPersistant = true)]
        public float CoreDamagePercent = 0f;

        // Amount of power dissipated w/ pressure in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve PressureCurve= new FloatCurve();
        // Amount of power dissipated w/ velocity in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve VelocityCurve = new FloatCurve();

        // Fairings
        // Editor Toggle
        [KSPEvent(guiName = "Toggle Reactor Fairing", guiActive = false, guiActiveEditor = false)]
        public void ToggleFairing () 
        {
        }

        // Whether the fairing is present
        [KSPField(isPersistant = true)]
        public bool hasFairing = false;
        


        // private

        // Current Ec/s generation
        private double currentGeneration =0d;
        

        // Ratio of cur power to max power
        private double thermalPowerRatio = 0f;

        // Ratio of cur temp to max temp
        private double coreTemperatureRatio = 0d;
        
        // the info staging box
        private VInfoBox infoBox;
        private List<FissionRadiator> radiators;
        private FissionGeneratorAnimator generatorAnimation;

        private FloatCurve WindCurve = new FloatCurve();

        // ACTIONS
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

        /// UI ACTIONS
        /// --------------------
        /// Toggle control panel
        [KSPEvent(guiActive = true, guiName = "Toggle Reactor Control", active = true)]
        public void ShowReactorControl()
        {
            FissionGeneratorUI.ToggleWindow();
            
        }

        // Deploy all radiators attached to this reactor
        [KSPEvent(guiActive = true, guiName = "Deploy Attached Radiators", active = false)]
        public void DeployRadiators()
        {

            foreach (FissionRadiator radiator in radiators)
            {
                //radiator.Extend();
            }
        }
        // Retract all radiators attached to this reactor
        [KSPEvent(guiActive = true, guiName = "Retract Attached Radiators", active = false)]
        public void RetractRadiators()
        {
            
            foreach (FissionRadiator radiator in radiators)
            {
               // radiator.Retract();
            }
        }
        // Toggle all radiators attached to this reactor
        [KSPEvent(guiActive = true, guiName = "Toggle Attached Radiators", active = true)]
        public void ToggleRadiators()
        {

            foreach (FissionRadiator radiator in radiators)
            {
                radiator.Toggle();
               //radiator.ExtendPanelsAction(new KSPActionParam(KSPActionGroup.None,KSPActionType.Activate));
            }
        }
        // try to refuel the reactor
        [KSPEvent(guiName = "Refuel Reactor", externalToEVAOnly = true, unfocusedRange = 2f, guiActiveUnfocused = true)]
        public void RefuelReactor()
        {
           this.TryRefuel();
        }


        // Actions
        [KSPAction("Deploy Radiators")]
        public void DeployRadiatorsAction(KSPActionParam param)
        {
            DeployRadiators();
        }

        [KSPAction("Retract Radiators")]
        public void RetractRadiatorsAction(KSPActionParam param)
        {
            RetractRadiators();
        }

        [KSPAction("Toggle Radiators")]
        public void ToggleRadiatorsAction(KSPActionParam param)
        {
            ToggleRadiators();
        }

        // STATUS STRINGS
        ///--------------------
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
            StringBuilder str = new StringBuilder();
            
            return String.Format("Maximum Power: {0:F2} Ec/s", PowerGenerationMaximum) + "\n" +
                String.Format("Required Radiator Power: {0:F2} kW", ThermalPower) + "\n" +
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
            this.moduleName = "Fission Reactor";
        }


        public override void OnStart(PartModule.StartState state)
        {
            if (UseStagingIcon)
                this.part.stagingIcon = "FUEL_TANK";
            else 
                Debug.Log("NFT: Fission Reactor: Staging Icon Disabled!");

            if (state != StartState.Editor)
            {
                WindCurve = new FloatCurve();
                WindCurve.Add(0f, 0f);
                WindCurve.Add(1000f, 20f);
                WindCurve.Add(200000f, 0f);

                if (UseStagingIcon)
                {
                    infoBox = this.part.stackIcon.DisplayInfo();

                    infoBox.SetMsgBgColor(XKCDColors.RedOrange);
                    infoBox.SetMsgTextColor(XKCDColors.Orange);
                    infoBox.SetLength(1.0f);
                    infoBox.SetMessage("CoreHeat");
                    infoBox.SetProgressBarBgColor(XKCDColors.RedOrange);
                    infoBox.SetProgressBarColor(XKCDColors.Orange);
                }
                generatorAnimation = part.Modules.OfType<FissionGeneratorAnimator>().First();
                SetupRadiators();

                if (UseForcedActivation)
                    this.part.force_activate();
   
                FuelUpdate();
                oldHighlight = part.highlightColor;
            }

            // LogItAll();

        }


        private void FuelUpdate()
        {

            if (LastFuelUpdate == 0f)
            {
                Debug.Log("NFT: Fission Reactor: checking nonfocused use, no time elapsed.");
                return;
            }
            double timeElapsed = vessel.missionTime - (double)LastFuelUpdate;
            
            float fuelUsage = (float)(((CurrentCoreTemperature / MaxCoreTemperature)) * BurnRate * timeElapsed);
            double fuelAmt = this.part.RequestResource(fuelName, fuelUsage);
            this.part.RequestResource(depletedName, -fuelAmt);
            Debug.Log("NFT: Fission Reactor: checking nonfocused use, time unfocused is " + timeElapsed.ToString() + "s, removing " + fuelAmt.ToString() + " fuel"); 
        }


        // Gets all attached radiators
        private void SetupRadiators()
        {
            Debug.Log("NFT: Fission Reactor: begin radiator check....");
            radiators = new List<FissionRadiator>();
            // Get attached radiators
            Part[] children = this.part.FindChildParts<Part>();
            // Debug.Log("NFPP: Reactor has " + children.Length.ToString()+" children");
            foreach (Part pt in children)
            {
                PartModuleList modules = pt.Modules;
                for (int i = 0; i < modules.Count; i++)
                {
                    PartModule curModule = modules.GetModule(i);
                    FissionRadiator candidate = curModule.GetComponent<FissionRadiator>();
                    if (candidate != null)
                    {
                        candidate.SetupRadiator(this);
                        radiators.Add(candidate);
                    }
                }

            }
            Debug.Log("NFT: Fission Reactor: Completed radiator check, found " + radiators.Count() + " radiators" );
        }
        public void RemoveRadiator(FissionRadiator rad)
        {
            if (rad != null)
            {
                radiators.Remove(rad);
            }
        }

        public float RadiatorEfficiency()
        {
            float wattsDissip = 0f;

            foreach (FissionRadiator rad in radiators)
            {
                wattsDissip += rad.HeatRejection((float)currentThermalPower / (float)radiators.Count);
            }

            return wattsDissip;
        }
        
        private void LogItAll()
        {
            Debug.Log(
                 "Enabled: " + Enabled.ToString() + "\n" +
                 "Radiator list: " + radiators + "\n" +
                    "Generator anim: " + generatorAnimation
                );
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


        public override void OnFixedUpdate()
        {
            float wattsRadiated = RadiatorEfficiency();
            float wattsConvected = 0f;
            float wattsGoal = 0f;
            float wattsError = 0f;

            float goalTemperature = 0f;

            double fuelUsage = 0d;

            // calculate atmoshperic dissipation
            if (Utils.VesselInAtmosphere(vessel))
            {
                //Debug.Log("NFPP: Debugging Pressure Curves... " + PressureCurve.maxTime.ToString() + " and " + PressureCurve.minTime.ToString() );
                float pressure = (float)FlightGlobals.getStaticPressure(vessel.transform.position);
                double velocity = vessel.srfSpeed;
                // v*const*
                wattsConvected = PressureCurve.Evaluate(pressure) * VelocityCurve.Evaluate((float)velocity+WindCurve.Evaluate((float)vessel.terrainAltitude))*(part.mass);
            }
            if (Enabled)
            {
                // Don't let thermal wattage go over radiation+convection
                if (SafetyLimit)
                {
                    wattsGoal = Mathf.Min(ThermalPower * CurrentPowerPercent, wattsRadiated + wattsConvected);
                }
                // Allow thermal power to go to maximum
                else
                {
                    wattsGoal = ThermalPower * CurrentPowerPercent;
                }
            }
            else
            {
                wattsGoal = 0f;
            }
            
            currentThermalPower = Mathf.MoveTowards(currentThermalPower, wattsGoal, TimeWarp.fixedDeltaTime * ThermalPowerResponseRate);
            thermalPowerRatio = (double)(currentThermalPower / ThermalPower);

            // what the wattage difference between cooling and power is
            wattsError = currentThermalPower - wattsRadiated - wattsConvected;
            //Debug.Log("Watt error: " + wattsError.ToString());

            if (wattsError > 0f)
            {
                // increase the overheat amount
                overheatAmount += TimeWarp.fixedDeltaTime * (wattsError / (this.part.mass * 500f));
                if (UseStagingIcon)
                    infoBox.SetValue(Mathf.Clamp01((CurrentCoreTemperature - MaxCoreTemperature) / (MeltdownCoreTemperature - MaxCoreTemperature)));
            }
            else
            {
                //overheatAmount += TimeWarp.fixedDeltaTime * (wattsError / (this.part.mass * 500f));
                overheatAmount = Mathf.MoveTowards(overheatAmount, 0f, TimeWarp.fixedDeltaTime * Mathf.Max((wattsError / (this.part.mass * 500f)), 1f));
                if (UseStagingIcon)
                    infoBox.SetValue(Mathf.Clamp01((CurrentCoreTemperature - MaxCoreTemperature) / (MeltdownCoreTemperature - MaxCoreTemperature)));
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




            // heat animation
            if (CoreDamagePercent >= 1f)
                generatorAnimation.SetHeatLevel(1f);
            else
                generatorAnimation.SetHeatLevel(Mathf.Clamp01((CurrentCoreTemperature - MaxCoreTemperature) / MeltdownCoreTemperature));

            currentGeneration = thermalPowerRatio * PowerGenerationMaximum * (1f - CoreDamagePercent);
            FuelStatus = FindTimeRemaining(BurnRate * coreTemperatureRatio);

            // compute the fuel usage
            fuelUsage = BurnRate * coreTemperatureRatio * TimeWarp.fixedDeltaTime;
            double fuelAmt = this.part.RequestResource(fuelName, fuelUsage);
            this.part.RequestResource(depletedName, -fuelAmt);

            if (fuelAmt <= 0d && fuelUsage > 0d)
            {
                FuelStatus = "No fuel remaining";

                ShutdownReactor();
            }

            LastFuelUpdate = (float)vessel.missionTime;
           
            // Split addition of resources into several calls, improves stability of high rates
            this.part.RequestResource("ElectricCharge", -0.25f*currentGeneration*TimeWarp.fixedDeltaTime);
            this.part.RequestResource("ElectricCharge", -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);
            this.part.RequestResource("ElectricCharge", -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);
            this.part.RequestResource("ElectricCharge", -0.25f * currentGeneration * TimeWarp.fixedDeltaTime);

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
                Debug.Log("NFT: Fission Reactor: Searching for valid containers...");
                FissionContainer from = FindValidFissionContainer();
                if (from != null)
                {
                    Debug.Log("NFT: Fission Reactor: Refuelling valid container...");
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
                    Debug.Log("NFT: Fission Reactor: Found valid FissionContainer.");
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
