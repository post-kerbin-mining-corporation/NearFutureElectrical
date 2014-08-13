/// FissionGeneratorRadiator
/// ---------------------------------------------------
/// Handles animation and fx for radiators

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionRadiator : ModuleDeployableSolarPanel
    {

        // Temperature lost by radiators
        [KSPField(isPersistant = false)]
        public float HeatRadiated;

        // Temperature lost by radiators when closed
        [KSPField(isPersistant = false)]
        public float HeatRadiatedClosed;

        // Amount of power dissipated w/ pressure in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve PressureCurve = new FloatCurve();

        // Amount of power dissipated w/ velocity in ideal conditions
        [KSPField(isPersistant = false)]
        public FloatCurve VelocityCurve = new FloatCurve();

        // animation for radiator heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        [KSPField(isPersistant = false)]
        public string HeatTransformName;

        // Tweakable to allow or disallow sun tracking
        [KSPField(guiName = "Tracking", guiActiveEditor = true,isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool TrackSun = true;


        // STATUS STRINGS
        ///--------------------
        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Heat Rejection")]
        public string HeatRejectionGUI = "0 kW";

        public override string GetInfo()
        {
            return String.Format("Heat Rejection (Retracted): {0:F1} kW", HeatRadiatedClosed) + "\n" +
                String.Format("Heat Rejection (Deployed): {0:F1} kW", HeatRadiated);
        }

        // Toggle radiator
        public void Toggle()
        {
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                base.Retract();
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                base.Extend();
        }

        private FloatCurve WindCurve = new FloatCurve();

        // Heat animations
        private AnimationState[] heatStates;
        private Transform heatTransform;
        private FissionGenerator attachedReactor;

        private float requestedHeatRejection = 0f;
        private float availableHeatRejection = 0f;

        public void SetupRadiator(FissionGenerator reactor)
        {
            attachedReactor = reactor;
        }

        public float HeatRejection(float request)
        {
            requestedHeatRejection = request;
            return availableHeatRejection;
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            heatStates = Utils.SetUpAnimation(HeatAnimation, part);


            heatTransform = part.FindModelTransform(HeatTransformName);
            
            foreach (AnimationState heatState in heatStates)
            {
                heatState.AddMixingTransform(heatTransform);
                heatState.blendMode = AnimationBlendMode.Blend;
                heatState.layer = 15;
                heatState.weight = 1.0f;
                heatState.enabled = true;
            }

            WindCurve = new FloatCurve();
            WindCurve.Add(0f, 0.5f);
            WindCurve.Add(1000f, 20f);
            WindCurve.Add(200000f, 0f);

            
            if (!TrackSun)
                base.trackingSpeed = 0f;

            part.force_activate();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (BaseField fld in base.Fields)
            {
                if (fld.guiName == "Sun Exposure")
                    fld.guiActive = false;
                if (fld.guiName == "Energy Flow")
                    fld.guiActive = false;
                if (fld.guiName == "Status")
                    fld.guiActive = false;
               
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            // Tweakable bits
           

            // Gameplay bits

            // Heat rejection from convection
            if (Utils.VesselInAtmosphere(this.vessel) && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
            {
                double pressure = FlightGlobals.getStaticPressure(vessel.transform.position);
                double velocity = vessel.srfSpeed;
                availableHeatRejection = PressureCurve.Evaluate((float)pressure) * VelocityCurve.Evaluate((float)velocity + WindCurve.Evaluate((float)vessel.terrainAltitude)) * (part.mass * 5f); ;
            }
                
            else
            {
                availableHeatRejection = 0f;
            }
            // Heat rejection from panels
            if (base.panelState != ModuleDeployableSolarPanel.panelStates.EXTENDED && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
            {
               // Debug.Log("Closed! " + HeatRadiatedClosed.ToString());
               availableHeatRejection += HeatRadiatedClosed;
            }
            else if (base.panelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
            {
               // Debug.Log("Broken!! " + 0.ToString());
                availableHeatRejection = 0f;
            }
            else
            {
               // Debug.Log("Open! " + HeatRadiated.ToString());
                availableHeatRejection += HeatRadiated;
            }
            
            foreach (AnimationState state in heatStates)
            {
                
                state.normalizedTime = Mathf.MoveTowards(state.normalizedTime, Mathf.Clamp01(requestedHeatRejection / availableHeatRejection), 0.1f * TimeWarp.fixedDeltaTime);
            }
            HeatRejectionGUI = String.Format("{0:F1} kW", availableHeatRejection);
        }

    }
}
