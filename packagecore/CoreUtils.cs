using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// dynamic using statements
#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
    // this bit needs editor to work

    public static void DisplayProgressBarAndSleep(string title, string message, float progress, int sleepTime)
    {
      EditorUtility.DisplayProgressBar(title, message, progress);
      System.Threading.Thread.Sleep(sleepTime);
    }

    public static async Task DisplayProgressBarAndSleepAsync(string title, string info, float progress, int sleepTime)
    {
      // Display the progress bar
      EditorUtility.DisplayProgressBar(title, info, progress);

      // Wait for the specified amount of time
      await Task.Delay(sleepTime);
    }
#endif
  }
}
