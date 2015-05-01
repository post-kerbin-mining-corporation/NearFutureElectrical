/// FissionReactor
/// ---------------------------------------------------
/// Fission Generator!
/// 

using System;
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
     
        // Use a staging icon or not
        [KSPField(isPersistant = false)]
        public bool UseStagingIcon = true;

        // Force activate on load or not
        [KSPField(isPersistant = false)]
        public bool UseForcedActivation = true;

        [KSPField(isPersistant = false)]
        public float HeatGeneration;

        // Current reactor power setting (0-1.0)
        [KSPField(isPersistant = true, guiActive = true, guiName = "Power Setting"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float CurrentPowerPercent = 50f;

        [KSPField(isPersistant = false)]
        public string FuelName = "EnrichedUranium";

        
       
        /// PRIVATE VARIABLES
        /// ----------------------
        // the info staging box
        private VInfoBox infoBox;

        // base paramters
        
        private List<ResourceBaseRatio> inputs;
        private List<ResourceBaseRatio> outputs;


        /// UI FIELDS
        /// --------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Life")]
        public string FuelStatus;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Output")]
        public string GeneratorStatus;

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
                SetupResourceRatios();
                // Set up staging icon heat bar
                if (UseStagingIcon)
                {
                    infoBox = this.part.stackIcon.DisplayInfo();
                    infoBox.SetMsgBgColor(XKCDColors.RedOrange);
                    infoBox.SetMsgTextColor(XKCDColors.Orange);
                    infoBox.SetLength(1.0f);
                    infoBox.SetMessage("Temp.");
                    infoBox.SetProgressBarBgColor(XKCDColors.RedOrange);
                    infoBox.SetProgressBarColor(XKCDColors.Orange);
                }
               
                if (UseForcedActivation)
                    this.part.force_activate();
            }
        }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (UseStagingIcon)
                    infoBox.SetValue((float)(part.temperature/part.maxTemp));

                if (base.ModuleIsActive())
                {
                    double rate = 0d;

                    foreach (ResourceRatio input in inputList)
                    {
                        if (input.ResourceName == FuelName)
                            rate = input.Ratio;
                    }

                    foreach (ResourceRatio output in outputList)
                    {
                        if (output.ResourceName == "ElectricCharge")
                            GeneratorStatus = String.Format("{0:F2}/s",output.Ratio);
                    }

                    FuelStatus = FindTimeRemaining(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,rate);
                    RecalculateRatios(CurrentPowerPercent/100f);
                    // Generate heat
                    this.part.AddThermalFlux(HeatGeneration* CurrentPowerPercent/100f);
                } else 
                {
                    FuelStatus = "Reactor Offline";
                    GeneratorStatus = "Reactor Offline";
                }

                
            }
        }

        private void RecalculateRatios(float inputScale)
        {
            
            foreach (ResourceRatio input in inputList)
            {
                foreach (ResourceBaseRatio baseInput in inputs)
                {
                    if (baseInput.ResourceName == input.ResourceName)
                        input.Ratio = baseInput.ResourceRatio * inputScale;
                }
            }
            foreach (ResourceRatio output in outputList)
            {
                foreach (ResourceBaseRatio baseOutput in outputs)
                {
                    if (baseOutput.ResourceName == output.ResourceName)
                        output.Ratio = baseOutput.ResourceRatio * inputScale;
                }
            }
        }


        // ####################################
        // Repairing
        // ####################################


        // Tries to repair the reactor
        public void TryRepair()
        {
        }
        
        private int KerbalEngineerLevel()
        {
            ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (kerbal.experienceTrait.Title == "Engineer")
            {
                return kerbal.experienceLevel;
            }
            else 
            {
                return -1;
            }
        }
        
        
        
        // ####################################
        // Refuelling
        // ####################################


      
        // Finds time remaining at current fuel burn rates
        public string FindTimeRemaining(double amount, double rate)
        {
            double remaining = amount / rate;
            TimeSpan t = TimeSpan.FromSeconds(remaining);

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
