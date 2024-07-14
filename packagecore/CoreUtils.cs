using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.IO;

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
      if (EditorPrefs.GetBool(Constants.DEBUG_PRINT_EDITOR_PREF, true))
      {
        Debug.Log(
          $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_DBG}>[DEBUG]</color> {message.ToString()}"
        );
      }
    }

    public static void CoreLogDebugPrintList(IEnumerable<string> list, string preMessage)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string item in list)
      {
        stringBuilder.Append(item + "\n");
      }
      CoreLogDebug(preMessage + "\n" + stringBuilder.ToString());
    }

    public static void CoreLogDebugPrintDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string preMessage)
    {
      List<string> entriesList = new List<string>();
      foreach (var kvp in dictionary)
      {
        entriesList.Add($"{kvp.Key}: {kvp.Value}");
      }
      CoreLogDebugPrintList(entriesList, preMessage);
    }
#endif

    public static string GenerateDebugCopyFilePath(string fullFilePath, string debugSuffix)
    {
      // get part of the path after last .
      string extension = Path.GetExtension(fullFilePath);
      // get part of the path before last .
      string pathWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);
      // combine the path without extension, .debug and extension
      string debugFilePath = Path.Combine(
        Path.GetDirectoryName(fullFilePath),
        pathWithoutExtension + debugSuffix + extension
      );
      return debugFilePath;
    }

    public static void ForceRefreshAssetDatabase()
    {
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      EditorApplication.QueuePlayerLoopUpdate();
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

    public static string GetFullTransformPath(Transform current)
    {
      string path = current.name;
      while (current.parent != null)
      {
        current = current.parent;
        path = current.name + "/" + path;
      }
      return path;
    }

    public static string GetCommonPath(List<string> paths)
    {
      if (paths == null || paths.Count == 0)
      {
        return string.Empty;
      }

      // Split each path into parts
      var splitPaths = paths.Select(path => path.Split('/')).ToList();
      var commonParts = new List<string>();

      // Assume the first path is the shortest; adjust if not
      int shortestPathLength = splitPaths.Min(sp => sp.Length);

      for (int i = 0; i < shortestPathLength; i++)
      {
        // Take the ith part of the first path as reference
        string currentPart = splitPaths[0][i];

        // Check if all paths have the same part at this position
        if (splitPaths.All(sp => sp[i] == currentPart))
        {
          commonParts.Add(currentPart);
        }
        else
        {
          // As soon as a difference is found, stop looking further
          break;
        }
      }

      // Join the common parts to form the common path
      string commonPath = string.Join("/", commonParts);
      return commonPath;
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

    public static bool CheckIfChildrenHaveSkinnedMeshRenderers(GameObject parentGameObject)
    {
      SkinnedMeshRenderer[] skinnedMeshRenderers = parentGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      if (skinnedMeshRenderers.Length > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

#endif
  }

#if UNITY_EDITOR
  public class CoreDebugPrintMenu
  {
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Debug/Console Debug Print Enable";
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
