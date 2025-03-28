using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.IO;

// dynamic using statements
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static class CoreUtils
  {
    public static Color hexadecimalToColour(string hex)
    {
      hex = hex.Replace("#", "");
      byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
      byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
      byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
      return new Color32(r, g, b, 255);
    }

#if UNITY_EDITOR

    public static void CoreLog(object message)
    {
      Debug.Log($"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] {message.ToString()}");
    }

    // alias CoreLogMessage to the above CoreLog
    public static void CoreLogMessage(object message)
    {
      CoreLog(message);
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

    public static void CoreLogWarning(object message)
    {
      Debug.LogWarning(
        $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_WARN}>[WARNING]</color> {message.ToString()}"
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

    public static void CoreLogDebugWarning(object message)
    {
      if (EditorPrefs.GetBool(Constants.DEBUG_PRINT_EDITOR_PREF, true))
      {
        Debug.LogWarning(
          $"[<color={Constants.APP_COLOUR}>{Constants.PROGRAM_DISPLAY_NAME}</color>] <color={Constants.APP_COLOUR_WARN}>[WARNING]</color> {message.ToString()}"
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

    public static void RemoveLayerByName(RuntimeAnimatorController controller, string layerName)
    {
      // Cast the RuntimeAnimatorController to AnimatorController
      AnimatorController animatorController = controller as AnimatorController;

      if (animatorController == null)
      {
        CoreLogError("The provided controller is not an AnimatorController.");
        return;
      }

      // Find the index of the layer with the specified name
      int layerIndex = -1;
      for (int i = 0; i < animatorController.layers.Length; i++)
      {
        if (animatorController.layers[i].name == layerName)
        {
          layerIndex = i;
          break;
        }
      }

      // If the layer was found, remove it
      if (layerIndex != -1)
      {
        Undo.RecordObject(animatorController, "Remove Layer");
        var layers = animatorController.layers.ToList();
        layers.RemoveAt(layerIndex);
        animatorController.layers = layers.ToArray();
        EditorUtility.SetDirty(animatorController);
        CoreLog($"Layer '{layerName}' removed from the AnimatorController.");
      }
      else
      {
        CoreLogWarning($"Layer '{layerName}' not found in the AnimatorController.");
      }
    }

    public static bool ContainsNestedPrefabs(GameObject prefab)
    {
      // First check if the root object is actually a prefab
      PrefabAssetType rootPrefabType = PrefabUtility.GetPrefabAssetType(prefab);
      if (rootPrefabType == PrefabAssetType.NotAPrefab)
      {
        return false;
      }

      // Get the root prefab instance handle
      GameObject rootPrefabInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(prefab);

      // Get all child transforms, including inactive ones
      Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);

      foreach (Transform child in allChildren)
      {
        // Skip the root object
        if (child == prefab.transform)
          continue;

        // Get the nearest prefab instance root for this child
        GameObject childPrefabInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(child.gameObject);

        // If this child has a different prefab instance root than the main prefab,
        // and it's a proper prefab (not a model prefab), then it's a nested prefab
        if (
          childPrefabInstance != null
          && childPrefabInstance != rootPrefabInstance
          && PrefabUtility.GetPrefabAssetType(childPrefabInstance) != PrefabAssetType.Model
        )
        {
          return true;
        }
      }

      return false;
    }
#endif

    public static List<GameObject> GetParentObjects(GameObject currentObject, GameObject targetParent)
    {
      List<GameObject> parentObjects = new List<GameObject>();

      Transform currentTransform = currentObject.transform;
      while (currentTransform != null && currentTransform.gameObject != targetParent)
      {
        currentTransform = currentTransform.parent;
        if (currentTransform != null)
        {
          parentObjects.Add(currentTransform.gameObject);
        }
      }

      return parentObjects;
    }

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

    // Top bar for the components in the inspector

    public static VisualElement CreateComponentTopBar(
      String prefix,
      String title,
      Color prefixColour,
      Color backgroundColour,
      Color hoverColour
    )
    {
      // Create a new VisualElement
      var topBar = new VisualElement();

      // Set the name of the topBar to allow styling
      topBar.name = "topBar";
      // allow clicking through
      topBar.pickingMode = PickingMode.Ignore;

      // set size of the topBar
      topBar.style.height = 21;
      // set left/right pos
      topBar.style.left = 0;
      topBar.style.right = 64;
      // set padding
      topBar.style.paddingLeft = 0;
      topBar.style.top = 0;
      // set text alignment
      topBar.style.unityTextAlign = TextAnchor.MiddleLeft;
      // set opacity
      topBar.style.opacity = 1;
      // set text overflow
      topBar.style.overflow = Overflow.Hidden;
      topBar.style.textOverflow = TextOverflow.Ellipsis;

      // set flex direction
      topBar.style.flexDirection = FlexDirection.Row;
      // set display
      topBar.style.display = DisplayStyle.Flex;

      // set font size
      topBar.style.fontSize = 12;
      // set font style
      topBar.style.unityFontStyleAndWeight = FontStyle.Bold;
      // set font colour
      topBar.style.color = Color.white;

      // Add a background color to the topBar
      topBar.style.backgroundColor = backgroundColour;

      // create a visual element for the topBar content
      var topBarContent = new VisualElement();

      // Set the name of the topBarContent to allow styling
      topBarContent.name = "topBarContent";
      // allow clicking through
      topBarContent.pickingMode = PickingMode.Ignore;

      // Add the topBarContent to the topBar
      topBar.Add(topBarContent);

      var prefixLabel = new Label(prefix)
      {
        style =
        {
          fontSize = 12,
          unityFontStyleAndWeight = FontStyle.Bold,
          color = prefixColour,
          marginRight = 4
        }
      };

      topBarContent.Add(prefixLabel);
      // Add a label to the topBar to show the component type
      var label = new Label(title);
      label.pickingMode = PickingMode.Ignore; // allow clicking through
      label.style.unityFontStyleAndWeight = FontStyle.Bold; // Make the label text bold
      topBarContent.Add(label);

      // Register hover events on the topBar and topBarContent
      topBar.RegisterCallback<MouseEnterEvent>(evt =>
      {
        topBar.style.backgroundColor = hoverColour;
      });
      topBar.RegisterCallback<MouseLeaveEvent>(evt =>
      {
        topBar.style.backgroundColor = backgroundColour;
      });

      topBarContent.RegisterCallback<MouseEnterEvent>(evt =>
      {
        topBar.style.backgroundColor = hoverColour;
      });
      topBarContent.RegisterCallback<MouseLeaveEvent>(evt =>
      {
        topBar.style.backgroundColor = backgroundColour;
      });

      return topBar;
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

  // Custom SwitchToggle class that creates a mobile-style toggle switch
  public class SwitchToggle : BindableElement, INotifyValueChanged<bool>
  {
    private VisualElement track;
    private VisualElement knob;
    private Label label;
    private bool _value;
    private SerializedProperty _boundProperty;

    public bool value
    {
      get { return _value; }
      set
      {
        if (_value != value)
        {
          bool previousValue = _value;
          _value = value;
          UpdateVisualState();
          using (ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previousValue, _value))
          {
            evt.target = this;
            SendEvent(evt);
          }
        }
      }
    }

    // Event that clients can subscribe to
    public event EventCallback<ChangeEvent<bool>> onValueChanged;

    public SwitchToggle(string labelText = null)
    {
      AddToClassList("switch-toggle");

      // Create container for the label and toggle
      var container = new VisualElement();
      container.AddToClassList("switch-toggle-container");
      hierarchy.Add(container);

      // Add label if provided
      if (!string.IsNullOrEmpty(labelText))
      {
        label = new Label(labelText);
        label.AddToClassList("switch-toggle-label");
        container.Add(label);
      }

      // Create toggle track
      track = new VisualElement();
      track.AddToClassList("switch-toggle-track");
      container.Add(track);

      // Create toggle knob
      knob = new VisualElement();
      knob.AddToClassList("switch-toggle-knob");
      track.Add(knob);

      // Register callbacks
      RegisterCallback<ClickEvent>(OnClick);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
      UpdateVisualState();
    }

    private void OnClick(ClickEvent evt)
    {
      value = !_value;
      evt.StopPropagation();
    }

    private void UpdateVisualState()
    {
      if (_value)
      {
        track.AddToClassList("switch-toggle-track-active");
        knob.AddToClassList("switch-toggle-knob-active");
      }
      else
      {
        track.RemoveFromClassList("switch-toggle-track-active");
        knob.RemoveFromClassList("switch-toggle-knob-active");
      }
    }

    public void RegisterValueChangedCallback(EventCallback<ChangeEvent<bool>> callback)
    {
      onValueChanged += callback;
      RegisterCallback<ChangeEvent<bool>>(callback);
    }

    public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<bool>> callback)
    {
      onValueChanged -= callback;
      UnregisterCallback<ChangeEvent<bool>>(callback);
    }

    // Implementation of INotifyValueChanged<bool>
    public void SetValueWithoutNotify(bool newValue)
    {
      if (_value != newValue)
      {
        _value = newValue;
        UpdateVisualState();
      }
    }

    // Add method to bind to a SerializedProperty
    public void BindProperty(SerializedProperty property)
    {
      _boundProperty = property;
      if (_boundProperty != null)
      {
        SetValueWithoutNotify(_boundProperty.boolValue);
      }
    }

    // Method to update value from bound property
    public void UpdateFromProperty()
    {
      if (_boundProperty != null && _boundProperty.serializedObject != null)
      {
        SetValueWithoutNotify(_boundProperty.boolValue);
      }
    }
  }
#endif
}
