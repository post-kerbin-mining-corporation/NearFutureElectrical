/// ReactorConsumer
/// ---------------------------------------------------
/// Base thing to consume output from a fission reactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class ModuleUpdateOverride: PartModule
    {

        private FissionReactor[] reactors;

        void Start()
        {
          reactors = this.GetComponents<FissionReactor>();
          for (int i = 0; i< reactors.Length; i++)
          {
              reactors[i].OverriddenStart();
          }
        }
         void FixedUpdate()
         {
           if (HighLogic.LoadedSceneIsFlight)
           {
            for (int i = 0; i< reactors.Length; i++)
            {
              reactors[i].OverriddenFixedUpdate();
            }
          }
         }

         void Update()
         {
           if (HighLogic.LoadedSceneIsFlight)
           {
             for (int i = 0; i< reactors.Length; i++)
             {
               reactors[i].OverriddenUpdate();
             }
            }
         }
    }
}
