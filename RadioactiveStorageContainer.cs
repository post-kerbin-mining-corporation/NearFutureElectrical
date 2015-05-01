/// RadioactiveStorageContainer.cs
/// ---------------------------
/// A container of fission fuel 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace NearFutureElectrical
{
    public class RadioactiveStorageContainer: PartModule
    {
        // Fuel that is dangerous to transfer
        [KSPField(isPersistant = false)]
        public string DangerousFuel = "DepletedFuel";

        // Fuel that is safe to transfer
        [KSPField(isPersistant = false)]
        public string SafeFuel = "EnrichedUranium";

        // Level of engie needed for transferring safe fuel
        [KSPField(isPersistant = false)]
        public int EngineerLevelForSafe = 1;

        // Level of engie needed for transferring dangerous fuel
        [KSPField(isPersistant = false)]
        public int EngineerLevelForDangerous = 3;

        // Maximum heat level at which a transfer can be made
        [KSPField(isPersistant = false)]
        public float MaxTempForTransfer = 300;

        // Heat Flux for waste
        [KSPField(isPersistant = false)]
        public float HeatFluxPerWasteUnit = 5;

        // Transfer the dangerous fuel
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Transfer Waste")]
        public void TransferWaste()
        {
            // See if we fail any of the start transfer conditions
            if (!PartCanTransferResource(DangerousFuel))
                return;
            if (CheckEngineerLevel(EngineerLevelForDangerous))
            {
                // Check for presence of L1/2engineer
                curTransferType = DangerousFuel;
                StartTransfer();
            }
        }

        // Transfer the safe fuel
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Transfer Fuel")]
        public void TransferFuel()
        {
            // See if we fail any of the start transfer conditions
            if (!PartCanTransferResource(SafeFuel))
                return;
            if (CheckEngineerLevel(EngineerLevelForSafe))
            {
                curTransferType = SafeFuel;
                StartTransfer();
            }
        }

        // starts and stops transfer mode
        private void StartTransfer()
        {
            transferring = true;
            transferMessage = new ScreenMessage("Left click a part to transfer nuclear fuel", 999f, ScreenMessageStyle.UPPER_CENTER);
            ScreenMessages.PostScreenMessage(transferMessage);
        }
        private void EndTransfer()
        {
            transferring = false;
            ScreenMessages.RemoveMessage(transferMessage);
            curTransferType = "";
        }

        // checks engineer requirements for transfer
        private bool CheckEngineerLevel(int lvl)
        {
            List<ProtoCrewMember> crew = part.vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewman in crew)
            {
                if (crewman.experienceTrait.Title == "Engineer")
                {
                    if (crewman.experienceLevel >= lvl)
                    {
                        return true;
                    }
                }
            }
            Utils.Log("No Engineer with level " + lvl.ToString() + " or higher found on board!");
            ScreenMessages.PostScreenMessage(new ScreenMessage("This transfer requires a Level " + lvl.ToString() + "Engineer on board!",5.0f,ScreenMessageStyle.UPPER_CENTER));
            return false;
        }

        public bool PartCanTransferResource(string nm)
        {
            
            // Some modules need to be off.
            ModuleResourceConverter converter = GetComponent<ModuleResourceConverter>(); 
            FissionReactor reactor = GetComponent<FissionReactor>();
            if (converter != null && converter.ModuleIsActive())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot transfer from a running converter!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (reactor !=null && reactor.ModuleIsActive())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot transfer from a running reactor! Seriously a bad idea!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }

            // Fail if the part is too hot
            if (part.temperature > MaxTempForTransfer)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("This part must be below " + MaxTempForTransfer.ToString() + " K to transfer!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            // Fail if that part can't contain this resource
            if ((GetResourceAmount(nm, true) <= 0d))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("This part has no " + nm + " to transfer!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            // Fail if this part has no resource
            if (GetResourceAmount(nm) <= 0d)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("This part has no " +nm + " to transfer!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }

            return true;
        }

        // Helpbers for getting a resource amount
        public double GetResourceAmount(string nm)
        {
            return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
        }
        public double GetResourceAmount(string nm,bool max)
        {
            if (max)
                return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount;
            else
                return GetResourceAmount(nm);
        }

        // Privacy
        private bool transferring = false;
        private string curTransferType = "";
        private ScreenMessage transferMessage;

        private void FixedUpdate()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                // Spent fuel needs cooling!
                double wasteAmount = GetResourceAmount(DangerousFuel);
                part.AddThermalFlux(HeatFluxPerWasteUnit * (float)wasteAmount);
            }
        }

        private void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (transferring && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    AttemptTransfer();
                    GetPartClicked();
                }
            }
        }

        private void AttemptTransfer()
        {
            Part targetPart = GetPartClicked();
            // No part was clicked
            if (targetPart == null)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("No part selected, exiting transfer mode...", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                EndTransfer();
            }
            else
            {
                // part cannot be on another vessel
                if (targetPart.vessel != part.vessel)
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot transfer to an unconnected vessel, exiting transfer mode...", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    EndTransfer();
                    return;
                } 

                RadioactiveStorageContainer container = targetPart.GetComponent<RadioactiveStorageContainer>();
                if (container == null)
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage("Selected part can't handle radioactive storage, exiting transfer mode...", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    EndTransfer();
                    
                }
                else
                {
                    ModuleResourceConverter converter = container.GetComponent<ModuleResourceConverter>();
                    FissionReactor reactor = container.GetComponent<FissionReactor>();
                    if (part.temperature > container.MaxTempForTransfer)
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage("Selected part must be below " + container.MaxTempForTransfer.ToString() + " K to transfer!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    }
                    
                    else if (converter != null && converter.ModuleIsActive())
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot transfer into a running converter!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                        
                    }
                    else if (reactor!= null && reactor.ModuleIsActive())
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage("Cannot transfer into a running reactor! Seriously a bad idea!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                        
                    }
                    else
                    {
                        // get available space in the target container
                        double availableSpace = container.GetResourceAmount(curTransferType, true) - container.GetResourceAmount(curTransferType);
                        double availableResource = this.GetResourceAmount(curTransferType);

                        // transfer as much as possible
                        double amount = this.part.RequestResource(curTransferType, availableSpace);
                        container.part.RequestResource(curTransferType, -amount);

                        ScreenMessages.PostScreenMessage(new ScreenMessage("Transferred " + amount.ToString() + " " + curTransferType + " to container!", 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    }
                }
            }
            
        }

        private Part GetPartClicked()
        {
            Camera flightCam = Camera.main;
            Ray clickRay = flightCam.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(clickRay, out hitInfo, 2500f))
            {
                Part hitPart = hitInfo.rigidbody.gameObject.GetComponent<Part>();
                //ScreenMessages.PostScreenMessage(new ScreenMessage("Hit! hit was " + hitInfo.rigidbody.gameObject.name, 5.0f, ScreenMessageStyle.UPPER_CENTER));
                if (hitPart != null)
                {
                    return hitPart;
                }
                else
                {
                    return null;
                }
            } 


            return null;
            
        }
    }    
}
