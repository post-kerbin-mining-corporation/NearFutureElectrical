using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    class FissionFlowRadiator:ModuleActiveRadiator
    {
        [KSPField(isPersistant = false)]
        public float passiveCooling = 0f;
        
        public void ChangeRadiatorTransfer(float scale)
        {
            this.maxEnergyTransfer = scale + passiveCooling*50f;
        }
    }
}
