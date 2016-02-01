using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    class FissionFlowRadiator:ModuleActiveRadiator
    {

        
        public void ChangeRadiatorTransfer(float scale)
        {
            this.maxEnergyTransfer = scale;
        }
    }
}
