using UnityEngine;
using UnityEditor;
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

    public static string GetGameObjectPath(GameObject obj)
    {
      string path = "/" + obj.name;
      while (obj.transform.parent != null)
      {
        obj = obj.transform.parent.gameObject;
        path = "/" + obj.name + path;
      }
      return path;
    }

    public static void DisplayProgressBarAndSleep(string title, string message, float progress, int sleepTime)
    {
      EditorUtility.DisplayProgressBar(title, message, progress);
      System.Threading.Thread.Sleep(sleepTime);
    }
  }
}
