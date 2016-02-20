/// NearFutureElectrical.cs
/// -------------------------
/// Core class for NearFutureElectrica. 
/// Currently does nothing more than store GUI data
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    
    ///  Should only be needed in flight
    [KSPAddon(KSPAddon.Startup.Flight, false)] 
    public class NearFutureElectrical : MonoBehaviour
    {

        public void Awake()
        {
            // Load resources up
            Resources.LoadResources();
        }

        
    }


}
