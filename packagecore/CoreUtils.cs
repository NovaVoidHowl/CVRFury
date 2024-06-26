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
    #if UNITY_EDITOR
    public static void CoreLog(object message)
    {
      Debug.Log($"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] {message.ToString()}");
    }

    public static void CoreLogError(object message)
    {
      Debug.LogError(
        $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_ERROR}>[ERROR]</color> {message.ToString()}"
      );
    }

    public static void CoreLogCritical(object message)
    {
      Debug.LogError(
        $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_CRIT}>[CRITICAL ERROR]</color> {message.ToString()}"
      );
    }

    public static void CoreLogDebug(object message)
    {
      if(EditorPrefs.GetBool(Constants.DEBUG_PRINT_EDITOR_PREF, true))
      {
        Debug.Log(
          $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_DBG}>[DEBUG]</color> {message.ToString()}"
        );
      }
    }
    #endif

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

    public static string TranslateMenuNameToParameterName(string menuName)
    {
      return menuName.Replace(" ", "").Replace("\\", "");
    }
    public static string TranslateMenuNameToParameterName(string menuName, bool forceMachineName)
    {
      if (forceMachineName)
      {
        return menuName;
      }
      else
      {
        return menuName.Replace(" ", "").Replace("\\", "");
      }
    }

    public static List<GameObject> GetAllChildGameObjects(this GameObject parent)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in parent.transform)
        {
            result.Add(child.gameObject);
            result.AddRange(child.gameObject.GetAllChildGameObjects());
        }
        return result;
    }

#if UNITY_EDITOR
    // this bit needs editor to work

    //function to add a menu item to the Unity Editor


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

    public static string GetHierarchyPath(GameObject start, GameObject end)
    {
      if (start == null || end == null)
      {
        return "";
      }

      List<string> path = new List<string>();
      Transform current = end.transform;

      while (current != null)
      {
        path.Add(current.name);
        if (current == start.transform)
        {
          break;
        }
        current = current.parent;
      }

      path.Reverse();
      return string.Join("/", path);
    }
#endif
  }
#if UNITY_EDITOR
  public class CoreDebugPrintMenu
  {
    private const string MENU_PATH =
      "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Debug/Console Debug Print Enable";
    private const string EDITOR_PREFS_KEY = Constants.DEBUG_PRINT_EDITOR_PREF;
    [MenuItem(MENU_PATH)]
    private static void ToggleConsoleDebugPrint()
    {
      // Toggle the value
      bool currentValue = EditorPrefs.GetBool(EDITOR_PREFS_KEY, false);
      EditorPrefs.SetBool(EDITOR_PREFS_KEY, !currentValue);
    }

    [MenuItem(MENU_PATH, true)]
    private static bool ToggleConsoleDebugPrintValidation()
    {
      // Toggle the checked state
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(EDITOR_PREFS_KEY, false));
      return true;
    }

  }
  #endif
}
