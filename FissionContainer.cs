/// FissionContainer
/// ---------------------------------------------------
/// A container of fission fuel used for refuelling

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace NearFutureElectrical
{
    public class FissionContainer: PartModule
    {
        [KSPField(isPersistant = false)]
        public string fuelName = "EnrichedUranium";
        [KSPField(isPersistant = false)]
        public string depletedName = "DepletedUranium";

        public bool Expended = false;


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        // Check to see if this module has both fuel and space for waste
        public bool CheckFuelSpace(double amt)
        {
            if (Expended)
            {
                return false;
            }

            double fuelAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            double wasteSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount;

            // waste is limiting
            if (fuelAvailable > 0.0d || wasteSpaceAvailable > 0.0d)
            {
                Utils.Log("FissionContainer has enough fuel");
                return true;
            }
            Utils.Log("FissionContainer has insufficient fuel");
            return false;
            
        }

        // Check to see if this module has any waste (for reprocessing)
        public bool CheckContainsWaste()
        {

            double wasteAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount;
            double fuelSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;

            if (wasteAvailable > 0d && fuelSpaceAvailable > 0)
            {
                Utils.Log("FissionContainer has waste & space");
                return true;
            }
            Utils.Log("FissionContainer has no waste and space");
            return false;

        }

        // Refuel from this module
        public void RefuelReactorFromContainer(FissionGenerator reactor, double amt)
        {
            // The reactor fuel max. Total contents of reactor should never exceed this number
            double reactorFuelMax = reactor.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).maxAmount;
            
            // Reactor resource counts
            double reactorFuelAvailable = reactor.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            double reactorWasteAvailable = reactor.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            
            // Container resource counts
            double fuelAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            double wasteSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount;

            //Transfer only the waste that will "fit"
            
            double wasteToTransfer = Mathf.Clamp((float)reactorWasteAvailable,0f,(float)wasteSpaceAvailable);
            
            // Remove the waste
            this.part.RequestResource(depletedName, -wasteToTransfer);
            reactor.part.RequestResource(depletedName, wasteToTransfer);
            
            // Space for fuel  after waste removed
            double fuelSpace = reactorFuelMax - reactor.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount
                - reactorWasteAvailable + reactor.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            
            // Add that amount of fuel if possible
            double thisAmt = this.part.RequestResource(fuelName, fuelSpace);
            reactor.part.RequestResource(fuelName, -thisAmt);
            
            
            if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount <= 0 ||
                      ((this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount) <= 0))
            {
                Expended = true;
                Utils.Log("FissionContainer is now expended");
            }
        }
    }    
}
