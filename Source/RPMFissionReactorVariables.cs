using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace NearFutureElectrical
{

    public class NFElectricalReactorVariables;PartModule
    {

      List<FissionReactor> reactors = new List<FissionReactor>();
      int currentReactor = -1;

        public void Awake()
        {
           if (HighLogic.LoadedSceneIsEditor)
             return;
           FindReactors();
        }

        public void FindReactors()
        {
              //Debug.Log("NFE: Capacitor Manager: Finding Capcitors");
            List<FissionReactor> unsortedReactorList = new List<FissionReactor>();
            // Get all parts
            List<Part> allParts = vessel.parts;
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                FissionReactor toAdd = allParts[i].GetComponent<FissionReactor>();
                if (toAdd != null)
                {
                        unsortedReactorList.Add(toAdd);
                }
            }

            //sort
            reactors= unsortedReactorList.OrderByDescending(x => x.HeatGeneration).ToList();
            reactors = unsortedReactorList;
        }

        public void NextReactor()
        {

          currentReactor = currentReactor +1;
          if (currentReactor >= reactors.Count)
          {
            currentReactor = 0;
          }
        }

        public void PrevReactor()
        {
          currentReactor = currentReactor -1;
          if (currentReactor < 0)
          {
            currentReactor = reactors.Count -1;
          }
        }

        public void CurrentReactor()
        {
          return reactors[currentReactor];
        }


        public object ProcessVariable(string variableName)
        {
            // Supported varibles
            // CORETEMP: Current core temp
            // NOMINALTEMP: Nominal core temp
            // CRITICALTEMP: Critical core temp
            // MELTDOWNTEMP
            // AUTOSHUTDOWNTEMP
            // STATE: On/Off 0-1
            // THROTTLE: Reactor throttle 0-100
            // COUNT: Current number of reactors
            // CURRENT: Current reactor ID

            if (currentReactor == -1)
              return;

            if (variableName == "CORETEMP")
              return GetCoreTemperature();
            if (variableName == "NOMINALTEMP")
              return GetNominalTemperature();
            if (variableName == "MELTDOWNTEMP")
              return GetMeltdownTemperature();
            if (variableName == "CRITICALTEMP")
              return GetCriticalTemperature();
            if (variableName == "AUTOSHUTDOWNTEMP")
              return GetShutdownTemperature();
            if (variableName == "STATE")
              return  GetReactorState();
            if (variableName == "THROTTLE")
              return GetReactorThrottle();

            if (variableName == "COUNT")
              return reactorList.Count;
            if (variableName == "CURRENT")
              return currentReactor;
        }

        float GetCoreTemperature()
        {
          return reactors[currentReactor].GetCoreTemperature();
        }

        float GetNominalTemperature()
        {
          return reactors[currentReactor].NominalTemperature;
        }

        float GetCriticalTemperature()
        {
          return reactors[currentReactor].CriticalTemperature;
        }

        float GetMeltdownTemperature()
        {
            return reactors[currentReactor].MaximumTemperature;
        }

        float GetShutdownTemperature()
        {
          return reactors[currentReactor].CurrentSafetyOverride;
        }

        float GetReactorState()
        {
          if (reactors[currentReactor].ModuleIsActive())
              return 1;
          return 0;
        }

        float GetReactorThrottle()
        {
          return reactors[currentReactor].CurrentPowerPercent;
        }
    }



}
