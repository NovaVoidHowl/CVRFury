// This needs reviewing as its not managing to find the correct elements to add the overlay to
// after the selection changes. It may be that the elements are not being found in the hierarchy
// as expected, could be some sort of race condition or timing issue.


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

    static InspectorOverlay()
    {
      Debug.Log("CVRFury: Inspector Overlay Initializing");
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
      Debug.Log("CVRFury: Initializing Overlay");

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
      Debug.Log("CVRFury: After Assembly Reload");
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
      Debug.Log("CVRFury: Selection Changed - Scheduling Updates");
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
        return;

      Debug.Log("CVRFury: Updating Headers");

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

        // Look for EditorElements that contain our component - handle both naming patterns
        var editorElements = window.rootVisualElement
          .Query<VisualElement>()
          .Where(
            e =>
              e.name == "Component Dev Mode Enabler"
              || (e.name?.StartsWith("GenericInspector_CVRFuryDevModeEnabler_") == true)
          )
          .ToList();

        CollectLog($"Found {editorElements.Count} Component Dev Mode Enabler elements");

        foreach (var editorElement in editorElements)
        {
          // Find the header container - fixed query
          var headerContainer = editorElement
            .Query<IMGUIContainer>()
            .Where(e => e.name != null && (e.name == "Component Dev Mode EnablerHeader" || e.name.EndsWith("Header")))
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
              CreateOrUpdateHeader(targetContainer, componentId, headerContainer);
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
      if (logCollector.Length > 0)
      {
        Debug.Log($"CVRFury Inspector Analysis Report:\n{logCollector}");
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
      VisualElement headerReference
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
          right = 60,
          height = 21,
          backgroundColor = new Color(0.125f, 0.125f, 0.125f),
          paddingLeft = 28,
          paddingTop = 4,
          unityTextAlign = TextAnchor.MiddleLeft,
          color = Color.white,
          opacity = 1,
          display = DisplayStyle.Flex
        }
      };

      customHeader.AddToClassList("cvr-fury-custom-header-overlay");

      var label = new Label("CVRFury Dev Mode Enabler")
      {
        style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold }
      };

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

    private static void FindEditorsInHierarchy(VisualElement element, List<VisualElement> results)
    {
      if (element == null)
        return;

      // Check if this element is our target
      if (element.name?.Contains("CVRFuryDevModeEnablerEditor") == true)
      {
        results.Add(element);
        CollectLog($"Found editor element at depth {GetElementDepth(element)}: {element.name}");
      }

      // Recursively check all children
      foreach (var child in element.Children())
      {
        FindEditorsInHierarchy(child, results);
      }
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
}
//#endif
