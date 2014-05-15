/// FissionGeneratorAnimator
/// ---------------------------------------------------
/// Handles animation for reactor heat

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace NearFutureElectrical
{
    public class FissionGeneratorAnimator: PartModule
    {

        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        private AnimationState[] heatStates;

        public override void OnStart(PartModule.StartState state)
        {
            heatStates = Utils.SetUpAnimation(HeatAnimation, part);


        }

        // set heat level from zero to one (1 = max heat)
        public void SetHeatLevel(float val)
        {
            foreach (AnimationState heatState in heatStates)
            {
                heatState.normalizedTime = val;
            }
        }
    }
}
