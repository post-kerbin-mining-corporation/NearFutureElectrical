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
    public class FissionEngine : FissionConsumer
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

        // Engine to work with
        [KSPField(isPersistant = false)]
        public string EngineID = "";


        private List<EngineBaseData> engineData = new List<EngineBaseData>();

        private FissionFlowRadiator flowRadiator;
        private FissionReactor reactor;
        private ModuleCoreHeat core;

        public override string GetInfo()
        {
            return "";
        }
        public bool EngineActive()
        {
            for (int i = 0; i < engineData.Count; i++)
            {
                if (engineData[i].engineFX.EngineIgnited)
                {
                    return true;
                }
            }
            return false;
        }
        public float GetThrustLimiterFraction()
        {
            for (int i = 0; i < engineData.Count; i++)
            {
                if (engineData[i].engineFX.isActiveAndEnabled)
                    return engineData[i].engineFX.thrustPercentage / 100f;
            }
            return 1.0f;
        }

        public void Start()
        {
            SetupEngines();
            SetupReactor();

        }

        private void SetupEngines()
        {
            // Locate the engine modules
            List<ModuleEnginesFX> engines = this.GetComponents<ModuleEnginesFX>().ToList();
            // Get their Isps
            for (int i = 0; i < engines.Count; i++)
            {
                if (EngineID == "" || engines[i].engineID == EngineID)
                    engineData.Add(new EngineBaseData(engines[i], engines[i].atmosphereCurve, engines[i].maxThrust));
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
                    for (int i = 0; i < engineData.Count; i++)
                    {
                        // If the engine is off, it will have the maximum Isp available
                        if (!engineData[i].engineFX.isActiveAndEnabled || (engineData[i].engineFX.isActiveAndEnabled && engineData[i].engineFX.throttleSetting <= 0f))
                        {
                            engineData[i].engineFX.atmosphereCurve = engineData[i].ispCurve;
                            maxFlowScalar = Mathf.Max(maxFlowScalar, 0.0f);
                        }
                        else
                        {
                            float CoreTemperatureRatio = TempIspScale.Evaluate((float)core.CoreTemperature);
                            float reactorRatio = reactor.ActualPowerPercent / 100f * reactor.CoreIntegrity / 100f;
                            if (!reactor.ModuleIsActive())
                                reactorRatio = 0f;

                            float ispRatio = CoreTemperatureRatio * reactorRatio;

                            engineData[i].engineFX.atmosphereCurve = new FloatCurve();
                            engineData[i].engineFX.atmosphereCurve.Add(0f, engineData[i].ispCurve.Evaluate(0f) * ispRatio);
                            engineData[i].engineFX.atmosphereCurve.Add(1f, engineData[i].ispCurve.Evaluate(1f) * ispRatio);
                            engineData[i].engineFX.atmosphereCurve.Add(4f, engineData[i].ispCurve.Evaluate(4f) * ispRatio);

                            //Utils.Log(String.Format("{0} ui {1} max {2} reqested", engineData[i].engineFX.fuelFlowGui, engineData[i].engineFX.maxFuelFlow, engineData[i].engineFX.requestedMassFlow));
                            //Utils.Log(String.Format("{0} t_setting {1} normTr {2} normout", engineData[i].engineFX.throttleSetting, engineData[i].engineFX.normalizedThrustOutput, engineData[i].engineFX.normalizedOutput));
                            maxFlowScalar = Mathf.Max(maxFlowScalar, (engineData[i].engineFX.throttleSetting));

                        }

                    }
                    float heat = reactor.ActualPowerPercent / 100f * reactor.HeatGeneration / 50f * reactor.CoreIntegrity / 100f;
                    //flowRadiator.ChangeRadiatorTransfer(Mathf.Max(base.CurrentHeatUsed, heat) * maxFlowScalar);

                    float exhaustCooling = flowRadiator.ChangeRadiatorTransfer(maxFlowScalar);
                    core.MaxCoolant = Mathf.Clamp(heat - exhaustCooling, 0.0, heat);
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
