/// FissionEngine
/// ---------------------------------------------------
/// An engine module that consumes availableHeat from a FissionReactor
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFutureElectrical
{
    public class FissionEngine: FissionConsumer
    {
        public struct EngineBaseData
        {
            public ModuleEnginesFX engineFX;
            public float maxThrust;
            public FloatCurve ispCurve;

            public EngineBaseData(ModuleEnginesFX fx, FloatCurve isp, float thrust)
            {
                
                engineFX = fx;
                maxThrust = thrust;
                ispCurve = isp;
            }
        }

        // Relates core temp (K) to Isp scalar
        [KSPField(isPersistant = false)]
        public FloatCurve TempIspScale = new FloatCurve();

        private List <EngineBaseData> engineData = new List<EngineBaseData>();

        private FissionFlowRadiator flowRadiator;
        private FissionReactor reactor;
        private ModuleCoreHeat core;

        public override string GetInfo()
        {
            return "";
        }

        public override void OnStart(PartModule.StartState state)
        {
            SetupEngines();
            SetupReactor();

        }

        private void SetupEngines()
        {
          // Locate the engine modules
          List<ModuleEnginesFX> engines = this.GetComponents<ModuleEnginesFX>().ToList();
          // Get their Isps
          foreach (ModuleEnginesFX engine in engines) {
              engineData.Add(new EngineBaseData(engine,engine.atmosphereCurve,engine.maxThrust));
          }
         
        }
        private void SetupReactor()
        {
            reactor = this.GetComponent<FissionReactor>();
            flowRadiator = this.GetComponent<FissionFlowRadiator>();
            core = this.GetComponent<ModuleCoreHeat>();
        }

        public void FixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {
            if (reactor != null)
            {
                float maxFlowScalar = 0f;
                foreach (EngineBaseData eData in engineData)
                {
                    // If the engine is off, it will have the maximum Isp available
                    if (!eData.engineFX.isActiveAndEnabled || (eData.engineFX.isActiveAndEnabled && eData.engineFX.throttleSetting <= 0f))
                    {
                        eData.engineFX.atmosphereCurve = eData.ispCurve;
                        maxFlowScalar = Mathf.Max(maxFlowScalar, 0.0f);
                    }
                    else
                    {
                        float CoreTemperatureRatio = TempIspScale.Evaluate((float)core.CoreTemperature);
                        float reactorRatio = reactor.CurrentPowerPercent / 100f;
                        if (!reactor.ModuleIsActive())
                            reactorRatio = 0f;

                        float ispRatio = CoreTemperatureRatio * reactorRatio;

                        eData.engineFX.atmosphereCurve = new FloatCurve();
                        eData.engineFX.atmosphereCurve.Add(0f, eData.ispCurve.Evaluate(0f) * ispRatio);
                        eData.engineFX.atmosphereCurve.Add(1f, eData.ispCurve.Evaluate(1f) * ispRatio);
                        eData.engineFX.atmosphereCurve.Add(4f, eData.ispCurve.Evaluate(4f) * ispRatio);

                        //Utils.Log(String.Format("{0} ui {1} max {2} reqested",eData.engineFX.fuelFlowGui,eData.engineFX.maxFuelFlow,eData.engineFX.requestedMassFlow));
                        maxFlowScalar = Mathf.Max(maxFlowScalar, (eData.engineFX.requestedMassFlow/eData.engineFX.maxFuelFlow));
                        
                    }
                   
                }
                float heat = reactor.CurrentPowerPercent / 100f * reactor.HeatGeneration / 50f * reactor.CoreIntegrity / 100f;
                flowRadiator.ChangeRadiatorTransfer(Mathf.Max(base.CurrentHeatUsed, heat) * maxFlowScalar);
            }

          }
        }
        private float FindFlowRate(float thrust, float isp, Propellant fuelPropellant)
        {
            double fuelDensity = PartResourceLibrary.Instance.GetDefinition(fuelPropellant.name).density;
            double fuelRate = ((thrust * 1000f) / (isp * Utils.GRAVITY)) / (fuelDensity * 1000f);
            return (float)fuelRate;
        }

        private float FindIsp(float thrust, float flowRate, Propellant fuelPropellant)
        {
            double fuelDensity = PartResourceLibrary.Instance.GetDefinition(fuelPropellant.name).density;
            double isp = (((thrust * 1000f) / (Utils.GRAVITY)) / flowRate) / (fuelDensity * 1000f);
            return (float)isp;
        }

        private float FindThrust(float isp, float flowRate, Propellant fuelPropellant)
        {
            
            double fuelDensity = PartResourceLibrary.Instance.GetDefinition(fuelPropellant.name).density;
            double thrust = Utils.GRAVITY * isp * flowRate;
            //double isp = (((thrust * 1000f) / (Utils.GRAVITY)) / flowRate) / (fuelDensity * 1000f);
            return (float)isp;
        }

    }
}
