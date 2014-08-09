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
            this.moduleName = "Fission Fuel Container";
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
                Debug.Log("NFPP: Container has enough fuel");
                return true;
            }
            Debug.Log("NFPP: Container has insufficient fuel");
            return false;
            
        }

        // Check to see if this module has any waste (for reprocessing)
        public bool CheckContainsWaste()
        {

            double wasteAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount;
            double fuelSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;

            if (wasteAvailable > 0d && fuelSpaceAvailable > 0)
            {
                Debug.Log("NFPP: Container has waste & space");
                return true;
            }
            Debug.Log("NFPP: Container has no waste and space");
            return false;

        }

        // Refuel from this module
        public void RefuelReactorFromContainer(FissionGenerator reactor, double amt)
        {
            double fuelAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount;
            double wasteSpaceAvailable = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount;

            double adjAmt = amt;
            if (wasteSpaceAvailable < fuelAvailable)
            {
                // waste limited
                adjAmt = (double)Mathf.Clamp((float)amt, 0f, (float)wasteSpaceAvailable);
            }
            else
            {
                // fuel limited
                adjAmt = (double)Mathf.Clamp((float)amt, 0f, (float)fuelAvailable);
            }

            //Debug.Log("NFPP: FissionContainer has enough fuel and waste space");
            this.part.RequestResource(fuelName, adjAmt);
            this.part.RequestResource(depletedName, -adjAmt);

            reactor.part.RequestResource(fuelName, -adjAmt);
            reactor.part.RequestResource(depletedName, adjAmt);

            if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(fuelName).id).amount <= 0 ||
                      ((this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).maxAmount - this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(depletedName).id).amount) <= 0))
            {
                Expended = true;
                Debug.Log("NFPP: FissionContainer is now expended");
            }
        }
    }    
}
