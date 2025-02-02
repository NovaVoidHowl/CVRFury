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

    static SceneUIOverlay()
    {
      EditorApplication.delayCall += () =>
      {
        instance = new SceneUIOverlay();
        SceneView.duringSceneGui += instance.OnSceneGUI;
        EditorApplication.quitting += OnEditorQuitting;
        SceneView.beforeSceneGui += instance.OnBeforeSceneGui;
      };
    }

    private static void OnEditorQuitting()
    {
      SceneView.duringSceneGui -= instance.OnSceneGUI;
      SceneView.beforeSceneGui -= instance.OnBeforeSceneGui;
    }

    private SceneUIOverlay()
    {
      // Ensure we create UI on the main thread
      if (EditorApplication.isPlaying)
      {
        CreateUI();
      }
      else
      {
        EditorApplication.delayCall += CreateUI;
      }
    }

    private void CreateUI()
    {
      if (uiInitialized)
      {
        CoreLogDebug("UI already initialized");
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
      if (!uiInitialized)
      {
        CoreLogDebugWarning("UI not ready");
        return;
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
  }

  public class UIOverlayOptionMenu
  {
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Options/Avatar Info Overlay Enable";
    private const string EDITOR_PREFS_KEY = Constants.AVATAR_OVERLAY_STATE_PREF;

    [MenuItem(MENU_PATH, false, -100)]
    private static void ToggleAvatarInfoOverlay()
    {
      // Toggle the value, using true as the default
      bool currentValue = EditorPrefs.GetBool(EDITOR_PREFS_KEY, true);
      EditorPrefs.SetBool(EDITOR_PREFS_KEY, !currentValue);
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
