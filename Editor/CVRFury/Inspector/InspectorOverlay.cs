// #if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.inspector
{
  [InitializeOnLoad]
  public static class InspectorOverlay
  {
    private static Dictionary<string, VisualElement> headerMapping = new Dictionary<string, VisualElement>();
    private static System.Type inspectorWindowType;
    private static HashSet<EditorWindow> knownInspectors = new HashSet<EditorWindow>();
    private static bool isInitialized = false;
    private static HashSet<string> activeComponents = new HashSet<string>();
    private static float updateInterval = 0.05f; // Increase update frequency to 50ms
    private static double lastUpdateTime;
    private static int pendingUpdateFrames = 0;
    private static readonly int totalUpdateFrames = 5; // Check for 5 frames after selection change
    private static StringBuilder logCollector = new StringBuilder();

    private class ComponentConfig
    {
      public string EditorName { get; set; }
      public string ComponentIdentifier { get; set; }
      public string HeaderText { get; set; }
      public string DebugModeIdentifier { get; set; }
      public string Prefix { get; set; }
      public Color PrefixColour { get; set; }
      public Color HeaderBackgroundColour { get; set; }

      public ComponentConfig(
        string editorName,
        string componentIdentifier,
        string headerText,
        string prefix,
        Color prefixColour,
        Color headerBackgroundColour
      )
      {
        EditorName = editorName;
        ComponentIdentifier = componentIdentifier;
        HeaderText = headerText;
        DebugModeIdentifier = $"GenericInspector_{componentIdentifier}_";
        Prefix = prefix;
        PrefixColour = prefixColour;
        HeaderBackgroundColour = headerBackgroundColour;
      }
    }

    private static readonly ComponentConfig[] ComponentConfigs = new[]
    {
      new ComponentConfig(
        "Component Dev Mode Enabler",
        "CVRFuryDevModeEnablerEditor",
        "Dev Mode Enabler",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR
      ),
      new ComponentConfig(
        "Data Storage Unit",
        "CVRFuryDataStorageUnitBase",
        "DSU",
        "CVR Fury",
        Constants.CVRFURY_HEADER_PREFIX_COLOUR,
        Constants.CVRFURY_HEADER_BACKGROUND_COLOUR
      ),
      // VRCFury
      new ComponentConfig(
        "VRC Fury (Script)",
        "VRCFuryStubBase",
        "VRC Fury Component",
        "VRC Fury",
        Constants.VRCFURY_HEADER_PREFIX_COLOUR,
        Constants.VRCFURY_HEADER_BACKGROUND_COLOUR
      ),
      // Add more components here as needed
    };

    static InspectorOverlay()
    {
      CoreLogDebug("CVRFury: Inspector Overlay Initializing");
      inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");

      EditorApplication.delayCall += () =>
      {
        InitializeOverlay();
        isInitialized = true;
      };

      // Ensure we stay initialized
      EditorApplication.update += EnsureInitialization;
    }

    private static void EnsureInitialization()
    {
      if (!isInitialized || EditorApplication.update == null)
      {
        InitializeOverlay();
        isInitialized = true;
      }
    }

    private static void InitializeOverlay()
    {
      CoreLogDebug("CVRFury: Initializing Overlay");

      // Remove any existing callbacks to prevent duplicates
      Selection.selectionChanged -= OnSelectionChanged;
      EditorApplication.update -= Update;
      EditorApplication.update -= CheckForNewInspectors;

      // Add our callbacks
      Selection.selectionChanged += OnSelectionChanged;
      EditorApplication.update += Update;
      EditorApplication.update += CheckForNewInspectors;

      // Subscribe to hierarchy window changes
      EditorApplication.hierarchyChanged += OnHierarchyChanged;

      // Force immediate first update
      UpdateHeaders();
      RepaintInspectors();
    }

    private static void OnHierarchyChanged()
    {
      UpdateHeaders();
    }

    private static void OnAfterAssemblyReload()
    {
      CoreLogDebug("CVRFury: After Assembly Reload");
      InitializeOverlay();
    }

    private static void RepaintInspectors()
    {
      if (inspectorWindowType == null)
        return;

      var inspectors = Resources.FindObjectsOfTypeAll(inspectorWindowType).Cast<EditorWindow>();
      foreach (var inspector in inspectors)
      {
        inspector.Repaint();
      }
    }

    private static void CheckForNewInspectors()
    {
      if (inspectorWindowType == null)
        return;

      var currentInspectors = Resources.FindObjectsOfTypeAll(inspectorWindowType).Cast<EditorWindow>();
      bool foundNew = false;

      foreach (var inspector in currentInspectors)
      {
        if (!knownInspectors.Contains(inspector))
        {
          foundNew = true;
          knownInspectors.Add(inspector);

          // Register for both geometry and panel changes
          inspector.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnInspectorGeometryChanged);
          inspector.rootVisualElement.RegisterCallback<AttachToPanelEvent>(evt => OnInspectorPanelChanged(inspector));
          inspector.rootVisualElement.RegisterCallback<DetachFromPanelEvent>(evt => OnInspectorPanelChanged(inspector));
        }
      }

      if (foundNew)
      {
        UpdateHeaders();
      }
    }

    private static void OnInspectorPanelChanged(EditorWindow inspector)
    {
      EditorApplication.delayCall += () =>
      {
        UpdateHeaders();
        inspector.Repaint();
      };
    }

    private static void OnInspectorGeometryChanged(GeometryChangedEvent evt)
    {
      EditorApplication.delayCall += UpdateHeaders;
    }

    private static void OnSelectionChanged()
    {
      CoreLogDebug("CVRFury: Selection Changed - Scheduling Updates");
      pendingUpdateFrames = totalUpdateFrames;

      // Add multiple delayed updates to catch the inspector rebuild
      EditorApplication.delayCall += () =>
      {
        EditorApplication.delayCall += () =>
        {
          UpdateHeaders();
          RepaintInspectors();
        };
      };
    }

    private static void UpdateHeaders()
    {
      if (inspectorWindowType == null)
      {
        return;
      }

      if (EditorPrefs.GetBool(Constants.INSPECTOR_OVERLAY_DEBUG_PREF, false))
      {
        CoreLogDebug("CVRFury: Updating Headers");
      }

      // Clean up any invalid entries
      var invalidKeys = headerMapping.Keys.Where(key => headerMapping[key]?.panel == null).ToList();
      foreach (var key in invalidKeys)
      {
        headerMapping.Remove(key);
      }

      Update();
      RepaintInspectors();
    }

    private static string GetEditorComponentId(VisualElement editor)
    {
      var editorType = editor.GetType();
      var componentIdField = editorType.GetProperty(
        "componentInstanceID",
        System.Reflection.BindingFlags.Instance
          | System.Reflection.BindingFlags.NonPublic
          | System.Reflection.BindingFlags.Public
      );
      if (componentIdField != null)
      {
        return componentIdField.GetValue(editor)?.ToString();
      }

      var targetIdField = editorType.GetProperty(
        "targetInstanceID",
        System.Reflection.BindingFlags.Instance
          | System.Reflection.BindingFlags.NonPublic
          | System.Reflection.BindingFlags.Public
      );
      if (targetIdField != null)
      {
        return targetIdField.GetValue(editor)?.ToString();
      }

      return null;
    }

    private static void Update()
    {
      if (!isInitialized || inspectorWindowType == null)
        return;

      // Clear any previous logs
      logCollector.Clear();

      // Handle pending updates from selection changes
      if (pendingUpdateFrames > 0)
      {
        pendingUpdateFrames--;
        CollectLog($"Pending update frames: {pendingUpdateFrames}");
        UpdateHeaders();
        FlushLogs();
        return;
      }

      // Remove throttling to ensure we catch all changes
      lastUpdateTime = EditorApplication.timeSinceStartup;

      var inspectorWindows = Resources.FindObjectsOfTypeAll(inspectorWindowType).Cast<EditorWindow>();
      CollectLog($"Found {inspectorWindows.Count()} inspector windows");
      activeComponents.Clear();

      foreach (var window in inspectorWindows)
      {
        if (window == null || window.rootVisualElement == null)
          continue;

        CollectLog("Starting detailed inspector analysis");

        foreach (var config in ComponentConfigs)
        {
          // Look for EditorElements that contain our component - handle both naming patterns
          var editorElements = window.rootVisualElement
            .Query<VisualElement>()
            .Where(e => e.name == config.EditorName || (e.name?.StartsWith(config.DebugModeIdentifier) == true))
            .ToList();

          CollectLog($"Found {editorElements.Count} {config.EditorName} elements");

          foreach (var editorElement in editorElements)
          {
            // Find the header container - fixed query
            var headerContainer = editorElement
              .Query<IMGUIContainer>()
              .Where(e => e.name != null && (e.name == $"{config.EditorName}Header" || e.name.EndsWith("Header")))
              .ToList()
              .FirstOrDefault();

            if (headerContainer != null)
            {
              var componentId = $"{editorElement.GetHashCode()}_{GetElementPath(editorElement)}";
              activeComponents.Add(componentId);

              // Use the parent of the header container
              var targetContainer = headerContainer.parent;
              if (!headerMapping.ContainsKey(componentId) || !targetContainer.Contains(headerMapping[componentId]))
              {
                CollectLog($"Creating/Updating header for {componentId} in editor element");
                CreateOrUpdateHeader(
                  targetContainer,
                  componentId,
                  headerContainer,
                  config.HeaderText,
                  config.Prefix,
                  config.PrefixColour,
                  config.HeaderBackgroundColour
                );
              }
            }
          }
        }
      }

      // Clean up headers for components that no longer exist
      var componentsToRemove = headerMapping.Keys.Where(id => !activeComponents.Contains(id)).ToList();
      foreach (var componentId in componentsToRemove)
      {
        if (headerMapping[componentId] != null)
        {
          headerMapping[componentId].RemoveFromHierarchy();
        }
        headerMapping.Remove(componentId);
      }

      // Ensure headers stay visible
      foreach (var window in knownInspectors)
      {
        if (window != null)
        {
          window.Repaint();
        }
      }

      // Flush all collected logs at the end
      FlushLogs();
    }

    private static void CollectLog(string message)
    {
      logCollector.AppendLine(message);
    }

    private static void FlushLogs()
    {
      // Check if the debug preference is enabled
      bool isDebugEnabled = EditorPrefs.GetBool(Constants.INSPECTOR_OVERLAY_DEBUG_PREF, false);

      if (logCollector.Length > 0)
      {
        if (isDebugEnabled)
        {
          // Output the logs if debug is enabled
          CoreLogDebug($"CVRFury Inspector Analysis Report:\n{logCollector}");
        }
        // Clear the log collector regardless of the debug state
        logCollector.Clear();
      }
    }

    private static string GetElementPath(VisualElement element)
    {
      var path = new List<string>();
      var current = element;
      while (current != null)
      {
        path.Add($"{current.GetType().Name}_{current.name}");
        current = current.parent;
      }
      path.Reverse();
      return string.Join("/", path);
    }

    private static void CreateOrUpdateHeader(
      VisualElement targetContainer,
      string componentId,
      VisualElement headerReference,
      string headerText,
      string prefix,
      Color prefixColour,
      Color backgroundColour
    )
    {
      CollectLog($"Creating header for {componentId}");
      if (headerMapping.ContainsKey(componentId))
      {
        headerMapping[componentId].RemoveFromHierarchy();
        headerMapping.Remove(componentId);
      }

      var customHeader = new VisualElement
      {
        name = $"cvr-fury-custom-header-overlay-{componentId}",
        pickingMode = PickingMode.Ignore,
        style =
        {
          position = Position.Absolute,
          top = headerReference.worldBound.y - headerReference.parent.worldBound.y,
          left = 21,
          right = 74,
          height = 21,
          backgroundColor = backgroundColour,
          paddingLeft = 10,
          paddingTop = 0,
          unityTextAlign = TextAnchor.MiddleLeft,
          color = Color.white,
          opacity = 1,
          display = DisplayStyle.Flex,
          flexDirection = FlexDirection.Row,
          borderBottomRightRadius = 10,
          borderTopRightRadius = 10
        }
      };

      customHeader.AddToClassList("cvr-fury-custom-header-overlay");

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
      customHeader.Add(prefixLabel);

      var label = new Label(headerText) { style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } };
      customHeader.Add(label);

      // Insert the custom header right after the original header
      targetContainer.Insert(targetContainer.IndexOf(headerReference) + 1, customHeader);
      headerMapping[componentId] = customHeader;

      var checkCount = 0;
      EditorApplication.update += () =>
      {
        if (checkCount++ < 100 && targetContainer != null && targetContainer.panel != null)
        {
          if (!targetContainer.Contains(customHeader))
          {
            CollectLog($"Restoring lost header for {componentId}");
            targetContainer.Insert(targetContainer.IndexOf(headerReference) + 1, customHeader);
          }
        }
      };
    }

    private static void DebugVisualElementHierarchy(VisualElement element, int depth = 0)
    {
      if (element == null || depth > 20)
        return; // Increased depth limit

      var indent = new string(' ', depth * 2);
      var classes = string.Join(", ", element.GetClasses());
      CollectLog($"{indent}Element: {element.name} | Type: {element.GetType().Name} | Classes: {classes}");

      foreach (var child in element.Children())
      {
        DebugVisualElementHierarchy(child, depth + 1);
      }
    }

    private static int GetElementDepth(VisualElement element)
    {
      int depth = 0;
      var parent = element.parent;
      while (parent != null)
      {
        depth++;
        parent = parent.parent;
      }
      return depth;
    }
  }

  public static class VisualElementExtensions
  {
    private static readonly string CustomStyleKey = "CVRFuryCustomHeader";

    public static bool HasCustomStyle(this VisualElement element)
    {
      return element.userData != null && element.userData.ToString() == CustomStyleKey;
    }

    public static void MarkAsCustomStyled(this VisualElement element)
    {
      element.userData = CustomStyleKey;
    }
  }

  public class InspectorOverlayMenu
  {
    private const string MENU_PATH =
      "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Debug/Development/Overlays/Inspector Overlay Debug";
    private const string EDITOR_PREFS_KEY = Constants.INSPECTOR_OVERLAY_DEBUG_PREF;

    [MenuItem(MENU_PATH)]
    private static void ToggleInspectorOverlayDebug()
    {
      // Toggle the value
      bool currentValue = EditorPrefs.GetBool(EDITOR_PREFS_KEY, false);
      EditorPrefs.SetBool(EDITOR_PREFS_KEY, !currentValue);
    }

    [MenuItem(MENU_PATH, true)]
    private static bool ToggleInspectorOverlayDebugValidation()
    {
      // Toggle the checked state
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(EDITOR_PREFS_KEY, false));
      return true;
    }
  }
}
//#endif
