#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
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
    private VisualElement overlayContainer;
    private Label objectNameLabel;
    private Label vrcFuryCountLabel;
    private Label dsuCountLabel;
    private VisualElement componentCounterBox;
    private Label errorListLabel;
    private VisualElement avatarError;
    private Label warningListLabel;
    private VisualElement avatarWarning;
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
          errorListLabel = rootVisualElement.Q<Label>("error-list");
          avatarError = rootVisualElement.Q<VisualElement>("avatar-error");
          warningListLabel = rootVisualElement.Q<Label>("warning-list");
          avatarWarning = rootVisualElement.Q<VisualElement>("avatar-warning");

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
              overlayContainer.RemoveFromClassList("border-avatar-not-selected");
              overlayContainer.RemoveFromClassList("border-avatar-error-condition");
              overlayContainer.AddToClassList("border-avatar-ok");
              componentCounterBox.style.display = DisplayStyle.Flex;
            }
            else
            {
              objectNameLabel.text = "Selected: Not an Avatar";
              overlayContainer.RemoveFromClassList("border-avatar-ok");
              overlayContainer.RemoveFromClassList("border-avatar-error-condition");
              overlayContainer.AddToClassList("border-avatar-not-selected");
              componentCounterBox.style.display = DisplayStyle.None;
            }
          }
          else
          {
            objectNameLabel.text = "Selected Avatar: None";
            overlayContainer.RemoveFromClassList("border-avatar-ok");
            overlayContainer.RemoveFromClassList("border-avatar-error-condition");
            overlayContainer.AddToClassList("border-avatar-not-selected");
            componentCounterBox.style.display = DisplayStyle.None;
          }
        }

        if (errorListLabel != null)
        {
          errorListLabel.style.display = DisplayStyle.None;
          avatarError.style.display = DisplayStyle.None;
        }

        if (warningListLabel != null)
        {
          warningListLabel.style.display = DisplayStyle.None;
          avatarWarning.style.display = DisplayStyle.None;
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

          // Errors and warnings reporting section
          bool hasErrors = false;
          bool hasWarnings = false;
          string errorMessage = "";
          string warningMessage = "";

          // Body/Face mesh check
          // check if the avatar has a bodyMesh set
          if (avatarRoot.GetComponent("CVRAvatar") != null)
          {
            var avatar = avatarRoot.GetComponent("CVRAvatar");
            if (avatar != null)
            {
              // Access bodyMesh as a field
              var bodyMeshField = avatar
                .GetType()
                .GetField(
                  "bodyMesh",
                  System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance
                );

              if (bodyMeshField != null)
              {
                try
                {
                  var bodyMeshValue = bodyMeshField.GetValue(avatar);

                  // Check for null or Unity's "null" object reference
                  if (bodyMeshValue == null || (bodyMeshValue is UnityEngine.Object unityObj && unityObj == null))
                  {
                    hasWarnings = true;
                    warningMessage += "\nBody mesh is not set.";
                  }
                  else if (bodyMeshValue is UnityEngine.Object meshObj && meshObj != null)
                  {
                    // Now check if it's a SkinnedMeshRenderer
                    if (meshObj is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                      GameObject meshGameObject = skinnedMeshRenderer.gameObject;
                      if (meshGameObject.transform.IsChildOf(avatarRoot.transform))
                      {
                        // is valid mesh and is a child of the avatar - all good
                      }
                      else
                      {
                        hasErrors = true;
                        errorMessage += "\nBody mesh is not on a child gameObject of the Avatar.";
                      }
                    }
                    else
                    {
                      hasErrors = true;
                      errorMessage += "\nBody mesh is not a valid SkinnedMeshRenderer. [ref.id=sr-01]";
                    }
                  }
                  else
                  {
                    // bodyMeshValue is some other type that we don't expect
                    hasWarnings = true;
                    warningMessage += "\nBody mesh reference is invalid or corrupted.";
                  }
                }
                catch (System.Exception e)
                {
                  hasErrors = true;
                  errorMessage += "\nError checking body mesh: " + e.Message;
                }
              }
              else
              {
                hasErrors = true;
                errorMessage +=
                  "\nCCK Link ERROR: Could not evaluate body mesh field\nPlease check the CCK version and CVR Fury version compatibility.";
              }
            }
          }

          // Animator Avatar check
          var animatorComponent = avatarRoot.GetComponent<Animator>();
          if (animatorComponent != null)
          {
            if (animatorComponent.avatar == null)
            {
              hasWarnings = true;
              warningMessage += "\nAnimator Avatar is not set.";
            }
          }
          else
          {
            hasErrors = true;
            errorMessage += "\nAnimator component is missing.";
          }

          // Error and warning output section
          // Handle error display
          if (errorListLabel != null)
          {
            if (hasErrors)
            {
              // for every \n in the error message, add a - directly after it
              errorMessage = errorMessage.Replace("\n", "\n- ");

              // remove the first \n from the error message
              if (errorMessage.StartsWith("\n"))
              {
                errorMessage = errorMessage.Substring(1);
              }

              errorListLabel.text = errorMessage;
              errorListLabel.style.display = DisplayStyle.Flex;
              avatarError.style.display = DisplayStyle.Flex;
            }
            else
            {
              errorListLabel.style.display = DisplayStyle.None;
              avatarError.style.display = DisplayStyle.None;
            }
          }

          // Handle warning display
          if (warningListLabel != null)
          {
            if (hasWarnings)
            {
              // for every \n in the warning message, add a - directly after it
              warningMessage = warningMessage.Replace("\n", "\n- ");

              // remove the first \n from the warning message
              if (warningMessage.StartsWith("\n"))
              {
                warningMessage = warningMessage.Substring(1);
              }

              warningListLabel.text = warningMessage;
              warningListLabel.style.display = DisplayStyle.Flex;
              avatarWarning.style.display = DisplayStyle.Flex;
            }
            else
            {
              warningListLabel.style.display = DisplayStyle.None;
              avatarWarning.style.display = DisplayStyle.None;
            }
          }

          // Set border styling based on the most severe condition
          if (hasErrors)
          {
            // Apply animated border for error condition
            overlayContainer.RemoveFromClassList("border-avatar-not-selected");
            overlayContainer.RemoveFromClassList("border-avatar-ok");
            overlayContainer.RemoveFromClassList("border-avatar-warning-condition");
            overlayContainer.AddToClassList("border-avatar-error-condition");
          }
          else if (hasWarnings)
          {
            // Apply warning border
            overlayContainer.RemoveFromClassList("border-avatar-not-selected");
            overlayContainer.RemoveFromClassList("border-avatar-ok");
            overlayContainer.RemoveFromClassList("border-avatar-error-condition");
            overlayContainer.AddToClassList("border-avatar-warning-condition");
          }
          else
          {
            // Remove the animated border when no errors or warnings
            overlayContainer.RemoveFromClassList("border-avatar-error-condition");
            overlayContainer.RemoveFromClassList("border-avatar-warning-condition");

            // Make sure to properly apply the green border (OK state)
            overlayContainer.RemoveFromClassList("border-avatar-not-selected");
            overlayContainer.AddToClassList("border-avatar-ok");
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

  [InitializeOnLoad]
  public class UIOverlayOptionMenu
  {
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Options/Avatar Info Overlay Enable";
    private const string EDITOR_PREFS_KEY = Constants.AVATAR_OVERLAY_STATE_PREF;

    [MenuItem(MENU_PATH, false, -100)]
    static void ToggleAvatarInfoOverlay()
    {
      SceneUIOverlay.HandleOverlayToggle();
    }

    [MenuItem(MENU_PATH, true, -100)]
    static bool ToggleAvatarInfoOverlayValidation()
    {
      // Toggle the checked state, using true as the default
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(EDITOR_PREFS_KEY, true));
      return true;
    }
  }
}
#endif
