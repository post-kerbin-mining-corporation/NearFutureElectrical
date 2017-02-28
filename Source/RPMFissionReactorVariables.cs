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
            // Supported variables
            // CORETEMP: Current core temp
            // NOMINALTEMP: Nominal core temp
            // CRITICALTEMP: Critical core temp
            // MELTDOWNTEMP
            // AUTOSHUTDOWNTEMP
            // STATE: On/Off as an integer, 0/1
            // THROTTLE: Reactor throttle, from 0-100
            // REALTHROTTLE: Reactor 'real' throttle, from 0-100
            // COUNT: Current number of reactors on the ship
            // CURRENT: Current reactor ID as selected

            if (currentReactor == -1)
              return null;

            switch (variableName)
            {
              case  "CORETEMP":
                return GetCoreTemperature();
              case "NOMINALTEMP":
                return GetNominalTemperature();
              case "MELTDOWNTEMP":
                return GetMeltdownTemperature();
              case "CRITICALTEMP":
                return GetCriticalTemperature();
              case "AUTOSHUTDOWNTEMP":
                return GetShutdownTemperature();
              case "STATE":
                return  GetReactorState();
              case "THROTTLE")
                return GetReactorThrottle();
              case "REALTHROTTLE")
                  return GetReactorActualThrottle();
              case "COUNT":
                return reactorList.Count;
              case "CURRENT":
                return currentReactor;
            }
            return null;
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

        float GetReactorThrottle()
        {
          return reactors[currentReactor].ActualPowerPercent;
        }
    }



}
