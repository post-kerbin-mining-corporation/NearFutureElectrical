using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace NearFutureElectrical
{

    public class NFElectricalReactorButtons:InternalModule
    {
        RPMFissionReactorVariables reactorController;

        public void NextReactor()
        {
          GetController();
             if (reactorController != null)
              reactorController.NextReactor();
        }
        public void PrevReactor()
        {
          GetController();
          if (reactorController != null)
            reactorController.PrevReactor();
        }

        public void ShutdownReactor()
        {
          GetController();
          if (reactorController != null)
            reactorController.CurrentReactor().StopResourceConverter();
        }

        public void ToggleReactor(bool state)
        {
          GetController();
          if (reactorController != null)
            reactorController.CurrentReactor().ToggleResourceConverterAction( new KSPActionParam(0,KSPActionType.Activate) );
        }

        public void AdjustReactorPower(float percent)
        {
          GetController();
          if (reactorController != null)
            reactorController.CurrentReactor().CurrentPowerPercent = percent;
        }

        private void GetController()
        {
          if (reactorController == null)
            reactorController = part.GetComponent<RPMFissionReactorVariables>();
        }
    }


}
