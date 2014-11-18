// Fission Reprocessor
/// ---------------------------------------------------
/// A part that slowly recycles fission fuel in a fission Container

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    class FissionReprocessor: PartModule
    {

        [KSPField(isPersistant = false)]
        public string fuelName = "EnrichedUranium";
        [KSPField(isPersistant = false)]
        public string depletedName = "DepletedUranium";

        // Is processor online?
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Energy cost per second
        [KSPField(isPersistant = false)]
        public float EnergyCost = 50f;

        // Refining rate per second
        [KSPField(isPersistant = false)]
        public float ReprocessRate = 0.0001f;

        // How much waste we recycle
        [KSPField(isPersistant = false)]
        public float RecycleEfficiency = 0.5f;

        // Animation
        [KSPField(isPersistant = false)]
        public string RefineAnimation;

        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        // Start Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Start Reprocessing", active =true)]
        public void StartReprocessing()
        {
            Enabled = true;
            foreach (AnimationState workState in workStates)
            {
                workState.speed = 1.0f;
                workState.wrapMode = WrapMode.Loop;
            }
            foreach (AnimationState heatState in heatStates)
            {
                heatState.speed = 0.25f;
                heatState.normalizedTime = 0.0f;
                heatState.wrapMode = WrapMode.Loop;
            }

        }
        // Stop Reprocessing Fuel
        [KSPEvent(guiActive = true, guiName = "Stop Reprocessing", active = false)]
        public void StopReprocessing()
        {
            Enabled = false;
            Status = "Shutdown";
            foreach (AnimationState workState in workStates)
            {
                workState.speed = 0.0f;
                workState.wrapMode = WrapMode.Loop;
            }
            foreach (AnimationState heatState in heatStates)
            {
                heatState.speed = 0.25f;
                heatState.wrapMode = WrapMode.Once;
            }
            
        }
        // Toggle Reprocessing
        [KSPEvent(guiActive = true, guiName = "Toggle Reprocessing", active = true)]
        public void ToggleReprocessing()
        {
            if (Enabled)
                StopReprocessing();
            else
                StartReprocessing();

        }

        // Actions
        [KSPAction("Start Reprocessing")]
        public void StartReprocessingAction(KSPActionParam param)
        {
            StartReprocessing();
        }

        [KSPAction("Stop Reprocessing")]
        public void StopReprocessingAction(KSPActionParam param)
        {
            StopReprocessing();
        }

        [KSPAction("Toggle Reprocessing")]
        public void ToggleReprocessingAction(KSPActionParam param)
        {
            ToggleReprocessing();
        }


        // STATUS STRINGS
        ///--------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string Status;
        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Power Required: {0:F1} Ec/s", EnergyCost) + "\n" +
                String.Format("Depleted Fuel Processing Rate: {0:F4} U/s", ReprocessRate) + "\n" +
                String.Format("Efficiency: {0:F0} %", RecycleEfficiency*100f);
        }

        private FissionContainer workContainer;
        private AnimationState[] workStates;
        private AnimationState[] heatStates;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            Status = "Shutdown";
            workStates = Utils.SetUpAnimation(RefineAnimation, part);
            heatStates = Utils.SetUpAnimation(HeatAnimation, part);
        }

        public override void OnUpdate()
        {
            // Update events
            if ((Enabled && Events["StartReprocessing"].active) || (!Enabled && Events["StopReprocessing"].active))
            {
                Events["StartReprocessing"].active = !Enabled;
                Events["StopReprocessing"].active = Enabled;
            }
            if (Enabled && workContainer != null)
            {
                foreach (AnimationState heatState in heatStates)
                {
                   
                }
            }
        }

        public override void OnFixedUpdate()
        {
            if (Enabled)
            {
                
                if (workContainer == null)
                {
                    
                    // try to get a container, turn us off if we can't find any
                    workContainer = FindValidFissionContainer();
                   
                    if (workContainer == null)
                    {
                        
                        StopReprocessing();
                        Status = "No waste found";
                       
                        return;
                    }
                }
                else
                {
                   
                    // consume power
                    double power = this.part.RequestResource("ElectricCharge", EnergyCost * TimeWarp.fixedDeltaTime);

                    if (power <= EnergyCost*Time.fixedDeltaTime)
                    {
                       
                        Status = "Not enough Electric Charge!";
                        StopReprocessing();
                    }
                    else
                    {
                        Status = String.Format("Processing at: {0:F2} DU/s", ReprocessRate);
                        double wasteRefined = workContainer.part.RequestResource(depletedName, ReprocessRate * TimeWarp.fixedDeltaTime);
                        if (wasteRefined >= 0d)
                        {
                            workContainer.part.RequestResource(fuelName, -wasteRefined * RecycleEfficiency);
                            if (workContainer.Expended && (workContainer.part.Resources.Get( PartResourceLibrary.Instance.GetDefinition(fuelName).id).maxAmount - 
                                workContainer.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount) > 0.0d)
                            {
                                workContainer.Expended = false;
                            }
                        }
                        else
                        {
                            StopReprocessing();
                            workContainer = null;
                           
                        }
                    }
                }
            }
        }


        // Finds a container to refuel
        FissionContainer FindValidFissionContainer()
        {
            List<FissionContainer> candidates = FindFissionContainers();
            foreach (FissionContainer cont in candidates)
            {
                // check for fuel space
                if (cont.CheckContainsWaste())
                {
                    Debug.Log("NFPP: Found valid container.");
                    return cont;
                }
            }
            ScreenMessages.PostScreenMessage(new ScreenMessage("No fuel containers with any Depleted Uranium Found!", 4f, ScreenMessageStyle.UPPER_CENTER));
            return null;
        }

        // Finds a list of all fission containers
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

    }
}
