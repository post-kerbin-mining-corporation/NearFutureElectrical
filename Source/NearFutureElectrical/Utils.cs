// Utils
// ---------------------------------
// Static functions that are useful for the NearFuture pack

using System;
using UnityEngine;
using System.Collections.Generic;

namespace NearFutureElectrical
{
  internal static class Utils
  {

    public const double GRAVITY = 9.80665;
    
    /// <summary>
    /// Finds all AnimationStates on a part and configures them.
    /// </summary>
    /// <param name="animationName"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    public static AnimationState[] SetUpAnimation(string animationName, Part part)
    {
      var states = new List<AnimationState>();
      foreach (var animation in part.FindModelAnimators(animationName))
      {
        var animationState = animation[animationName];
        animationState.speed = 0;
        animationState.enabled = true;
        // Clamp this or else weird things happen
        animationState.wrapMode = WrapMode.ClampForever;
        animation.Blend(animationName);
        states.Add(animationState);
      }
      // Convert 
      return states.ToArray();
    }



    /// <summary>
    /// Sets an object as active as well as all child objects
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="active"></param>
    public static void SetActiveRecursively(GameObject obj, bool active)
    {
      obj.SetActive(active);

      foreach (Transform child in obj.transform)
      {
        SetActiveRecursively(child.gameObject, active);
      }
    }
    
    /// <summary>
    /// Formats a time string from seconds
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string FormatTimeString(double seconds)
    {

      return KSPUtil.dateTimeFormatter.PrintDate(seconds, true, false);
      //  double dayLength;
      //  double yearLength;
      //  double rem;
      //  if (GameSettings.KERBIN_TIME)
      //  {
      //    dayLength = 6d;
      //    yearLength = 426d;
      //  }
      //  else
      //  {
      //    dayLength = 24d;
      //    yearLength = 365d;
      //  }

      //  int years = (int)(seconds / (3600.0d * dayLength * yearLength));
      //  rem = seconds % (3600.0d * dayLength * yearLength);
      //  int days = (int)(rem / (3600.0d * dayLength));
      //  rem = rem % (3600.0d * dayLength);
      //  int hours = (int)(rem / (3600.0d));
      //  rem = rem % (3600.0d);
      //  int minutes = (int)(rem / (60.0d));
      //  rem = rem % (60.0d);
      //  int secs = (int)rem;

      //  string result = "";

      //  // draw years + days
      //  if (years > 0)
      //  {
      //    result += years.ToString() + "y ";
      //    result += days.ToString() + "d ";
      //    result += hours.ToString() + "h ";
      //    result += minutes.ToString() + "m";
      //  }
      //  else if (days > 0)
      //  {
      //    result += days.ToString() + "d ";
      //    result += hours.ToString() + "h ";
      //    result += minutes.ToString() + "m ";
      //    result += secs.ToString() + "s";
      //  }
      //  else if (hours > 0)
      //  {
      //    result += hours.ToString() + "h ";
      //    result += minutes.ToString() + "m ";
      //    result += secs.ToString() + "s";
      //  }
      //  else if (minutes > 0)
      //  {
      //    result += minutes.ToString() + "m ";
      //    result += secs.ToString() + "s";
      //  }
      //  else if (seconds > 0)
      //  {
      //    result += secs.ToString() + "s";
      //  }
      //  else
      //  {
      //    result = "None";
      //  }


      //  return result;
      //}
    }
    /// <summary>
    /// Turns a value in seconds into decimal current homeworld years
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static double CalculateDecimalYears(double seconds)
    {
      return  seconds / KSPUtil.dateTimeFormatter.Year;
    }
    
    /// <summary>
    /// Converts Kerbin years into current homeworld years
    /// </summary>
    /// <param name="kerbinYears"></param>
    /// <returns></returns>
    public static double KerbinYearsToLocalYears(double kerbinYears)
    {
      double seconds = 3600.0 * 6.0 * 426.08 * kerbinYears;
      return seconds / KSPUtil.dateTimeFormatter.Year;
    }

    // LOGGING
    // -------
    public static void Log(string message)
    {
      Debug.Log($"[NearFutureElectrical]: {message}" );
    }

    public static void LogWarn(string message)
    {
      Debug.LogWarning($"[NearFutureElectrical]: {message}");
    }

    public static void LogError(string message)
    {
      Debug.LogError($"[NearFutureElectrical]: {message}");
    }
  }


}
