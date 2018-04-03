/// RadioactiveStorageContainer.cs
/// ---------------------------
/// A container of fission fuel

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using KSP.Localization;

namespace NearFutureElectrical
{
    public class RadioactiveStorageContainer : PartModule
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

        [KSPField(isPersistant = false, guiActive = false, guiName = "Waste Xfer Status")]
        public string WasteTransferStatus;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Fuel XFer Status")]
        public string FuelTransferStatus;

        // Maximum heat level at which a transfer can be made
        [KSPField(isPersistant = false)]
        public float MaxTempForTransfer = 300;

        // Heat Flux for waste
        [KSPField(isPersistant = false)]
        public float HeatFluxPerWasteUnit = 5;

        public override string GetInfo()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_PartInfo");
        }
        public string GetModuleTitle()
        {
            return "Nuclear Fuel Storage";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_ModuleName");
        }

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
            transferMessage = new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_StartTransfer"), 999f, ScreenMessageStyle.UPPER_CENTER);
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
                if (crewman.experienceTrait.TypeName == "Engineer")
                {
                    if (crewman.experienceLevel >= lvl)
                    {
                        return true;
                    }
                }
            }
            Utils.Log("No Engineer with level " + lvl.ToString() + " or higher found on board!");
            ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortEngineerLevel", lvl.ToString()), 5.0f, ScreenMessageStyle.UPPER_CENTER));
            return false;
        }

        public bool PartCanTransferResource(string nm)
        {

            // Some modules need to be off.
            ModuleResourceConverter converter = GetComponent<ModuleResourceConverter>();
            FissionReactor reactor = GetComponent<FissionReactor>();
            if (converter != null && converter.ModuleIsActive())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortFromRunningConverter"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            if (reactor != null && reactor.ModuleIsActive())
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortFromRunningReactor"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }

            // Fail if the part is too hot
            if (part.temperature > MaxTempForTransfer)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortTooHot", MaxTempForTransfer.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            // Fail if that part can't contain this resource
            if ((GetResourceAmount(nm, true) <= 0d))
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortNoResource", nm), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }
            // Fail if this part has no resource
            if (GetResourceAmount(nm) <= 0d)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortNoResource", nm), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                return false;
            }

            return true;
        }

        // Helpbers for getting a resource amount
        public double GetResourceAmount(string nm)
        {
            if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
                return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
            else
                return 0.0;
        }
        public double GetResourceAmount(string nm, bool max)
        {
            if (max)
                if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
                    return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount;
                else
                    return 0.0;

            else
                return GetResourceAmount(nm);
        }

        // Privacy
        private bool crewWasteFlag = false;
        private bool crewFuelFlag = false;

        private bool transferring = false;
        private string curTransferType = "";
        private ScreenMessage transferMessage;

        public override void OnStart(PartModule.StartState state)
        {
            Events["TransferFuel"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Event_TransferFuel");
            Events["TransferWaste"].guiName = Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Event_TransferWaste");
        }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (GetResourceAmount(DangerousFuel) <= 0d)
                {
                    Events["TransferWaste"].guiActive = false;
                }
                else
                {
                    Events["TransferWaste"].guiActive = true;
                }
                if (GetResourceAmount(SafeFuel) <= 0d)
                {
                    Events["TransferFuel"].guiActive = false;
                }
                else
                {
                    Events["TransferFuel"].guiActive = true;
                }
                // Generate heat
                if (TimeWarp.CurrentRate <= 100f)
                {
                    // Spent fuel needs cooling!
                    double wasteAmount = GetResourceAmount(DangerousFuel);
                    part.AddThermalFlux(HeatFluxPerWasteUnit * (float)wasteAmount);
                }
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

        private bool VesselConnected(Part targetPart)
        {
            if (this.part.vessel.id == targetPart.vessel.id)
                return true;
            return false;
        }
        private void AttemptTransfer()
        {
            Part targetPart = GetPartClicked();
            // No part was clicked
            if (targetPart == null)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortNoPartSelected"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                EndTransfer();
            }
            else
            {
                // part cannot be on another vessel
                if (!VesselConnected(targetPart))
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortUnconnected"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    EndTransfer();
                    return;
                }

                RadioactiveStorageContainer container = targetPart.GetComponent<RadioactiveStorageContainer>();
                if (container == null)
                {
                    ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortNoRadStorage"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    EndTransfer();

                }
                else
                {
                    Debug.Log("A");
                    ModuleResourceConverter converter = container.GetComponent<ModuleResourceConverter>();
                    FissionReactor reactor = container.GetComponent<FissionReactor>();
                    Debug.Log("B");
                    if (part.temperature > container.MaxTempForTransfer)
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortTooHot", container.MaxTempForTransfer.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                    }

                    else if (converter != null && converter.ModuleIsActive())
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortToRunningConverter"), 5.0f, ScreenMessageStyle.UPPER_CENTER));

                    }
                    else if (reactor != null && reactor.ModuleIsActive())
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_AbortToRunningReactor"), 5.0f, ScreenMessageStyle.UPPER_CENTER));

                    }
                    else
                    {
                        Debug.Log("s");
                        // get available space in the target container
                        double availableSpace = container.GetResourceAmount(curTransferType, true) - container.GetResourceAmount(curTransferType);
                        double availableResource = this.GetResourceAmount(curTransferType);
                        Debug.Log("1");
                        // transfer as much as possible
                        double amount = this.part.RequestResource(curTransferType, availableSpace);
                        container.part.RequestResource(curTransferType, -amount);
                        Debug.Log("2");
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleRadioactiveStorageContainer_Message_Success", amount.ToString("F1"), curTransferType.ToString()), 5.0f, ScreenMessageStyle.UPPER_CENTER));
                        transferring = false;
                        curTransferType = "";
                    }
                }
            }

        }

        private Part GetPartClicked()
        {
            Camera flightCam = Camera.main;
            Ray clickRay = flightCam.ScreenPointToRay(Input.mousePosition);

            LayerMask mask;
            LayerMask maskA = 1 << LayerMask.NameToLayer("Default");
            LayerMask maskB = 1 << LayerMask.NameToLayer("TerrainColliders");
            LayerMask maskC = 1 << LayerMask.NameToLayer("Local Scenery");

            mask = maskA | maskB | maskC;

            RaycastHit hitInfo;
            if (Physics.Raycast(clickRay, out hitInfo, 2500f, mask))
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
