using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    class FissionFlowRadiator: ModuleActiveRadiator
    {
        [KSPField(isPersistant = false)]
        public float passiveCooling = 0f;

        int ticker = 0;
        void Update()
        {
            // oh god.
            if (ticker > 60)
            {
                base.Activate();
                ticker = 0;
            }
            else
            {
                ticker = ticker + 1;
            }
            
             
            if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.EDITOR)   
            {
                foreach (BaseField fld in base.Fields)
                {
                    if (fld.name == "Cooling")
                    {
                        fld.guiActive = false;
                        fld.guiActiveEditor = false;
                    }

                }

                if (Events["Activate"].active == true)
                    Events["Activate"].active = false;
                if (Events["Shutdown"].active == true)
                    Events["Shutdown"].active = false;
            }
            
            
        }
        


        public void ChangeRadiatorTransfer(float scale)
        {
            base.maxEnergyTransfer = (scale + passiveCooling) * 50d;
        }
    }
}
