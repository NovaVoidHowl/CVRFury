using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static class CoreUtils
  {
    public static void CoreLog(object message)
    {
      Debug.Log($"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] {message.ToString()}");
    }

    public static void CoreLogCritical(object message)
    {
      Debug.LogError(
        $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_CRIT}>[CRITICAL ERROR]</color> {message.ToString()}"
      );
    }
  }
}
