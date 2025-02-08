#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.scene
{
  [InitializeOnLoad]
  public class SceneUIOverlay
  {
    private static int overlayWidth = 400; // Width of the overlay box in pixels (must match the uss file)
    private static SceneUIOverlay instance;
    private VisualElement rootVisualElement;
    private VisualElement overlayContainer; // Add this line
    private Label objectNameLabel;
    private Label vrcFuryCountLabel;
    private Label dsuCountLabel;
    private VisualElement componentCounterBox;
    private bool uiInitialized = false;
    private bool uiAttached = false;
    private double playModeExitTime;
    private const double UI_RESTORE_DELAY = 1.0; // 1 second delay

    static SceneUIOverlay()
    {
      EditorApplication.delayCall += () =>
      {
        if (instance == null && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
          instance = new SceneUIOverlay();
          SceneView.duringSceneGui += instance.OnSceneGUI;
          EditorApplication.quitting += OnEditorQuitting;
          SceneView.beforeSceneGui += instance.OnBeforeSceneGui;
          EditorApplication.update += OnEditorUpdate;
          EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
      };

      EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private static void OnEditorUpdate()
    {
      if (instance != null && !EditorApplication.isPlayingOrWillChangePlaymode)
      {
        if (EditorApplication.timeSinceStartup >= instance.playModeExitTime + UI_RESTORE_DELAY)
        {
          if (!instance.uiAttached)
          {
            instance.ForceUIRefresh();
          }
        }
      }
    }

    private static void OnHierarchyChanged()
    {
      if (!EditorApplication.isPlayingOrWillChangePlaymode && instance != null)
      {
        instance.ForceUIRefresh();
      }
    }

    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
      switch (state)
      {
        case PlayModeStateChange.ExitingEditMode:
          if (instance != null)
          {
            instance.SafeRemoveUI();
          }
          break;

        case PlayModeStateChange.EnteredEditMode:
          EditorApplication.delayCall += () =>
          {
            if (instance == null)
            {
              instance = new SceneUIOverlay();
              SceneView.duringSceneGui += instance.OnSceneGUI;
              SceneView.beforeSceneGui += instance.OnBeforeSceneGui;
            }
            instance.playModeExitTime = EditorApplication.timeSinceStartup;
            instance.ForceUIRefresh();
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
              sceneView.Repaint();
            }
          };
          break;
      }
    }

    private static void OnEditorQuitting()
    {
      EditorApplication.update -= OnEditorUpdate;
      EditorApplication.hierarchyChanged -= OnHierarchyChanged;
      EditorApplication.playModeStateChanged -= PlayModeStateChanged;
      SceneView.duringSceneGui -= instance.OnSceneGUI;
      SceneView.beforeSceneGui -= instance.OnBeforeSceneGui;
    }

    private SceneUIOverlay()
    {
      if (!EditorApplication.isPlayingOrWillChangePlaymode)
      {
        EditorApplication.delayCall += CreateUI;
      }
    }

    private void CreateUI()
    {
      if (uiInitialized || EditorApplication.isPlayingOrWillChangePlaymode)
      {
        return;
      }

      // Add safety check
      if (!EditorApplication.isPlayingOrWillChangePlaymode)
      {
        try
        {
          rootVisualElement = new VisualElement();
          rootVisualElement.name = "cvrfury-avatar-status-scene-ui-overlay";
          rootVisualElement.style.position = Position.Absolute;

          // Debug path construction
          string uxmlPath = Constants.PROGRAM_DISPLAY_NAME + "/Scene/UnityUXML/CVRFuryAvatarStatusSceneUIOverlay";
          string ussPath = Constants.PROGRAM_DISPLAY_NAME + "/Scene/UnityStyleSheets/CVRFuryAvatarStatusSceneUIOverlay";

          CoreLogDebug("Loading UXML from: " + uxmlPath);
          CoreLogDebug("Loading USS from: " + ussPath);

          var baseTree = Resources.Load<VisualTreeAsset>(uxmlPath);
          var stylesheet = Resources.Load<StyleSheet>(ussPath);

          if (baseTree == null)
          {
            CoreLogError($"Failed to load UXML at {uxmlPath}");
            return;
          }

          if (stylesheet == null)
          {
            CoreLogError($"Failed to load USS at {ussPath}");
            return;
          }

          var overlayInstance = baseTree.Instantiate();
          rootVisualElement.Add(overlayInstance);
          rootVisualElement.styleSheets.Add(stylesheet);

          // Get reference to the overlay container
          overlayContainer = rootVisualElement.Q<VisualElement>(className: "overlay-container");
          objectNameLabel = rootVisualElement.Q<Label>("object-name");
          vrcFuryCountLabel = rootVisualElement.Q<Label>("object-vrcfury-count");
          dsuCountLabel = rootVisualElement.Q<Label>("object-dsu-count");
          componentCounterBox = rootVisualElement.Q<VisualElement>("component-counter");

          uiInitialized = true;
          CoreLogDebug($"UI elements initialized and ready for attachment");
        }
        catch (System.Exception e)
        {
          CoreLogError($"Failed to create UI: {e.Message}");
          uiInitialized = false;
        }
      }
    }

    private bool HasComponentByNameInParent(GameObject obj, string componentName)
    {
      if (obj == null)
        return false;

      // Check if the current object has the component
      if (obj.GetComponent(componentName) != null)
        return true;

      // If we haven't reached the root, check the parent
      if (obj.transform.parent != null)
        return HasComponentByNameInParent(obj.transform.parent.gameObject, componentName);

      return false;
    }

    private GameObject GetAvatarRoot(GameObject obj)
    {
      if (obj == null)
        return null;

      // Check if this object has the CVRAvatar component
      if (obj.GetComponent("CVRAvatar") != null)
        return obj;

      // If not, check parent if it exists
      if (obj.transform.parent != null)
        return GetAvatarRoot(obj.transform.parent.gameObject);

      return null;
    }

    private void OnBeforeSceneGui(SceneView sceneView)
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode)
      {
        return;
      }

      // Reset attachment flag if UI is not in hierarchy
      if (rootVisualElement != null)
      {
        bool isAttached = false;
        if (sceneView != null && sceneView.rootVisualElement != null)
        {
          isAttached = sceneView.rootVisualElement.Contains(rootVisualElement);
        }

        if (!isAttached)
        {
          uiAttached = false;
          CoreLogDebug("UI detached - will reattach");
        }
      }
    }

    private int CountComponentsByNameInChildren(GameObject obj, string componentName)
    {
      if (obj == null)
        return 0;

      int count = 0;

      // Count components on this object
      var components = obj.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component != null && component.GetType().Name == componentName)
        {
          count++;
        }
      }

      // Count in children
      foreach (Transform child in obj.transform)
      {
        count += CountComponentsByNameInChildren(child.gameObject, componentName);
      }

      return count;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
      if (!uiInitialized || EditorApplication.isPlayingOrWillChangePlaymode)
      {
        return;
      }

      // Check if this scene view is focused and needs UI refresh
      if (EditorWindow.focusedWindow == sceneView && !uiAttached)
      {
        ForceUIRefresh();
      }

      // Check if overlay is enabled
      bool overlayEnabled = EditorPrefs.GetBool(Constants.AVATAR_OVERLAY_STATE_PREF, true); // true is the default value

      if (!overlayEnabled)
      {
        if (uiAttached)
        {
          rootVisualElement.RemoveFromHierarchy();
          uiAttached = false;
        }
        return;
      }

      // Check if we need to reattach
      if (rootVisualElement.parent == null)
      {
        uiAttached = false;
      }

      // Attach UI if not already attached
      if (!uiAttached && sceneView != null)
      {
        // Remove from any existing parent just in case
        rootVisualElement.RemoveFromHierarchy();

        sceneView.rootVisualElement.Add(rootVisualElement);
        sceneView.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnWindowResize);
        rootVisualElement.style.display = DisplayStyle.Flex;
        uiAttached = true;
        CoreLogDebug("UI attached to SceneView");
        UpdateOverlayPosition(sceneView);
      }

      // Update position every frame to ensure it stays centered
      if (uiAttached)
      {
        UpdateOverlayPosition(sceneView);

        GameObject avatarRoot = null;

        // Update labels and UI state
        if (objectNameLabel != null)
        {
          if (Selection.activeGameObject != null)
          {
            avatarRoot = GetAvatarRoot(Selection.activeGameObject);
            if (avatarRoot != null)
            {
              objectNameLabel.text = $"Selected Avatar: {avatarRoot.name}";
              overlayContainer.RemoveFromClassList("border-red"); // Use overlayContainer instead
              overlayContainer.AddToClassList("border-green"); // Use overlayContainer instead
              componentCounterBox.style.display = DisplayStyle.Flex;
            }
            else
            {
              objectNameLabel.text = "Selected: Not an Avatar";
              overlayContainer.RemoveFromClassList("border-green"); // Use overlayContainer instead
              overlayContainer.AddToClassList("border-red"); // Use overlayContainer instead
              componentCounterBox.style.display = DisplayStyle.None;
            }
          }
          else
          {
            objectNameLabel.text = "Selected Avatar: None";
            overlayContainer.RemoveFromClassList("border-green");
            overlayContainer.AddToClassList("border-red");
            componentCounterBox.style.display = DisplayStyle.None;
          }
        }

        // Only update component counts if we have a valid avatar
        if (avatarRoot != null)
        {
          if (vrcFuryCountLabel != null)
          {
            int furyCount = CountComponentsByNameInChildren(avatarRoot, "VRCFury");
            vrcFuryCountLabel.text = $"VRCFury Components: {furyCount}";
          }

          if (dsuCountLabel != null)
          {
            int dsuCount = CountComponentsByNameInChildren(avatarRoot, "CVRFuryDataStorageUnit");
            dsuCountLabel.text = $"CVRFury DSUs: {dsuCount}";
          }
        }
      }
    }

    private void UpdateOverlayPosition(SceneView sceneView)
    {
      if (rootVisualElement == null || sceneView == null)
        return;

      float windowWidth = sceneView.rootVisualElement.layout.width;
      float centerX = (windowWidth - overlayWidth) / 2;

      rootVisualElement.style.left = centerX;
      rootVisualElement.style.top = 0;
    }

    private void OnWindowResize(GeometryChangedEvent evt)
    {
      if (SceneView.lastActiveSceneView != null)
      {
        UpdateOverlayPosition(SceneView.lastActiveSceneView);
      }
    }

    private void ForceUIRefresh()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode)
      {
        return;
      }

      try
      {
        SafeRemoveUI();
        CreateUI();

        if (uiInitialized && rootVisualElement != null)
        {
          foreach (SceneView sceneView in SceneView.sceneViews)
          {
            if (sceneView != null)
            {
              ForceReattachUI(sceneView);
              sceneView.Repaint();
            }
          }
        }
      }
      catch (System.Exception e)
      {
        CoreLogError($"Error during UI refresh: {e.Message}");
      }
    }

    private void SafeRemoveUI()
    {
      try
      {
        if (rootVisualElement != null)
        {
          // Remove event handlers first
          if (rootVisualElement.parent != null)
          {
            rootVisualElement.parent.UnregisterCallback<GeometryChangedEvent>(OnWindowResize);
          }

          rootVisualElement.RemoveFromHierarchy();
          rootVisualElement = null;
        }
        uiAttached = false;
        uiInitialized = false;
      }
      catch (System.Exception e)
      {
        CoreLogError($"Error during UI removal: {e.Message}");
      }
    }

    private void ForceReattachUI(SceneView sceneView)
    {
      if (sceneView == null || EditorApplication.isPlayingOrWillChangePlaymode)
      {
        return;
      }

      try
      {
        if (!uiInitialized || rootVisualElement == null)
        {
          CreateUI();
        }

        if (!uiInitialized || rootVisualElement == null)
        {
          return;
        }

        // Clean up existing UI first
        SafeRemoveUI();

        // Create new UI elements
        CreateUI();

        if (uiInitialized && rootVisualElement != null)
        {
          bool overlayEnabled = EditorPrefs.GetBool(Constants.AVATAR_OVERLAY_STATE_PREF, true);
          if (overlayEnabled && sceneView.rootVisualElement != null)
          {
            sceneView.rootVisualElement.Add(rootVisualElement);
            sceneView.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnWindowResize);
            rootVisualElement.style.display = DisplayStyle.Flex;
            uiAttached = true;
            UpdateOverlayPosition(sceneView);
            sceneView.Repaint();
          }
        }
      }
      catch (System.Exception e)
      {
        CoreLogError($"Error during UI reattachment: {e.Message}");
        SafeRemoveUI();
      }
    }

    public static void HandleOverlayToggle()
    {
      try
      {
        bool currentValue = EditorPrefs.GetBool(Constants.AVATAR_OVERLAY_STATE_PREF, true);
        EditorPrefs.SetBool(Constants.AVATAR_OVERLAY_STATE_PREF, !currentValue);

        if (instance != null)
        {
          if (currentValue) // If we're turning it off
          {
            instance.SafeRemoveUI();
          }
          else // If we're turning it on
          {
            EditorApplication.delayCall += () =>
            {
              if (instance != null)
              {
                instance.CreateUI();
                instance.ForceUIRefresh();
                SceneView.RepaintAll();
              }
            };
          }
        }
      }
      catch (System.Exception e)
      {
        CoreLogError($"Error during overlay toggle: {e.Message}");
      }
    }
  }

  public class UIOverlayOptionMenu
  {
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Options/Avatar Info Overlay Enable";
    private const string EDITOR_PREFS_KEY = Constants.AVATAR_OVERLAY_STATE_PREF;

    [MenuItem(MENU_PATH, false, -100)]
    private static void ToggleAvatarInfoOverlay()
    {
      SceneUIOverlay.HandleOverlayToggle();
    }

    [MenuItem(MENU_PATH, true, -100)]
    private static bool ToggleAvatarInfoOverlayValidation()
    {
      // Toggle the checked state, using true as the default
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(EDITOR_PREFS_KEY, true));
      return true;
    }
  }
}
#endif
