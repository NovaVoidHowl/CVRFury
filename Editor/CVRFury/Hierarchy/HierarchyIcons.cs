#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Compilation;
using System.Collections.Generic;
using System.Linq;
using VF.Model;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.hierarchy
{
  public static class HierarchyIconsConstants
  {
    public const string EDITOR_PREFS_KEY = Constants.HIERARCHY_ICONS_STATE_PREF;
  }

  public enum IssueType
  {
    None,
    Warning,
    Error
  }

  [InitializeOnLoad]
  public static class HierarchyIcons
  {
    private static Dictionary<int, IssueType> gameObjectIssueCache = new Dictionary<int, IssueType>();
    private static Dictionary<int, IssueType> childIssueCache = new Dictionary<int, IssueType>();
    private static bool isInitialized = false;
    private static GUIContent warningIconContent;
    private static GUIContent errorIconContent;
    private static GUIContent arrowIconContent;
    private static float iconSize = 16f;
    private static float arrowSize = 12f;
    private static Dictionary<VRCFury, SerializedObject> serializedObjectCache =
      new Dictionary<VRCFury, SerializedObject>();
    private static readonly int MaxCacheSize = 100; // Prevent unbounded growth

    static HierarchyIcons()
    {
      EditorApplication.delayCall += () =>
      {
        if (
          !EditorApplication.isPlayingOrWillChangePlaymode
          && !EditorApplication.isCompiling
          && EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true)
        )
        {
          Initialize();
          EditorApplication.RepaintHierarchyWindow();
        }
      };
    }

    public static void Initialize()
    {
      if (isInitialized)
        return;

      CoreLogDebug("Initializing Hierarchy Icons");

      warningIconContent = EditorGUIUtility.IconContent("console.warnicon");
      if (warningIconContent != null)
      {
        warningIconContent.tooltip = "CVRFury/VRCFury Warning";
      }

      errorIconContent = EditorGUIUtility.IconContent("console.erroricon");
      if (errorIconContent != null)
      {
        errorIconContent.tooltip = "CVRFury/VRCFury Error - VRC Stub Component Detected";
      }

      // Create arrow icon content using a built-in Unity icon
      arrowIconContent = EditorGUIUtility.IconContent("tab_next@2x");
      if (arrowIconContent == null)
      {
        arrowIconContent = EditorGUIUtility.IconContent("forward@2x");
      }
      if (arrowIconContent == null)
      {
        arrowIconContent = EditorGUIUtility.IconContent("d_forward");
      }
      if (arrowIconContent != null)
      {
        arrowIconContent.tooltip = "Issue found in child objects";
      }

      EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
      EditorSceneManager.sceneOpened += OnSceneOpened;
      EditorApplication.hierarchyChanged += OnHierarchyChanged;
      Undo.postprocessModifications += OnComponentModified;
      Selection.selectionChanged += OnSelectionChanged;
      CompilationPipeline.compilationStarted += OnCompilationStarted;
      CompilationPipeline.compilationFinished += OnCompilationFinished;

      isInitialized = true;
      EditorApplication.RepaintHierarchyWindow();
      CoreLogDebug("Hierarchy Icons Initialized");
    }

    public static void Cleanup()
    {
      if (!isInitialized)
        return;

      EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
      EditorSceneManager.sceneOpened -= OnSceneOpened;
      CompilationPipeline.compilationStarted -= OnCompilationStarted;
      CompilationPipeline.compilationFinished -= OnCompilationFinished;
      EditorApplication.hierarchyChanged -= OnHierarchyChanged;
      Undo.postprocessModifications -= OnComponentModified;
      Selection.selectionChanged -= OnSelectionChanged;

      // Clear the SerializedObject cache
      foreach (var serializedObj in serializedObjectCache.Values)
      {
        if (serializedObj != null)
        {
          serializedObj.Dispose();
        }
      }
      serializedObjectCache.Clear();

      gameObjectIssueCache.Clear();
      childIssueCache.Clear();
      warningIconContent = null;
      errorIconContent = null;
      arrowIconContent = null;
      isInitialized = false;
      CoreLogDebug("Hierarchy Icons Cleaned Up");
    }

    private static void OnCompilationStarted(object obj) => Cleanup();

    private static void OnCompilationFinished(object obj)
    {
      if (EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true))
      {
        Initialize();
      }
    }

    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
      EditorApplication.delayCall += () =>
      {
        gameObjectIssueCache.Clear();
        childIssueCache.Clear();
        EditorApplication.RepaintHierarchyWindow();
      };
    }

    private static void OnHierarchyChanged()
    {
      if (Selection.activeGameObject != null)
      {
        MarkObjectAndHierarchyDirty(Selection.activeGameObject);
      }
    }

    private static UndoPropertyModification[] OnComponentModified(UndoPropertyModification[] modifications)
    {
      foreach (var mod in modifications)
      {
        var component = mod.currentValue?.target as Component;
        if (component != null)
        {
          var gameObject = component.gameObject;
          MarkObjectAndHierarchyDirty(gameObject);
        }
      }
      return modifications;
    }

    private static void OnSelectionChanged()
    {
      if (Selection.activeGameObject != null)
      {
        MarkObjectAndHierarchyDirty(Selection.activeGameObject);
      }
    }

    private static void MarkObjectAndHierarchyDirty(GameObject gameObject)
    {
      if (gameObject == null)
        return;

      // Clear cache entries for this object and its parents
      var instanceID = gameObject.GetInstanceID();
      gameObjectIssueCache.Remove(instanceID);
      childIssueCache.Remove(instanceID);

      var parent = gameObject.transform.parent;
      while (parent != null)
      {
        gameObjectIssueCache.Remove(parent.gameObject.GetInstanceID());
        childIssueCache.Remove(parent.gameObject.GetInstanceID());
        parent = parent.parent;
      }

      EditorApplication.RepaintHierarchyWindow();
    }

    private static void DrawHierarchyItem(int instanceID, Rect selectionRect)
    {
      try
      {
        if (
          !isInitialized
          || EditorApplication.isPlaying
          || !EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true)
          || Event.current.type != EventType.Repaint
        )
          return;

        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
          return;

        // Check for direct issues on this GameObject
        if (!gameObjectIssueCache.TryGetValue(instanceID, out IssueType directIssueType))
        {
          directIssueType = CheckForIssues(gameObject);
          gameObjectIssueCache[instanceID] = directIssueType;
        }

        // Check for issues in child objects
        if (!childIssueCache.TryGetValue(instanceID, out IssueType childIssueType))
        {
          childIssueType = CheckForChildIssues(gameObject);
          childIssueCache[instanceID] = childIssueType;
        }

        // Draw the direct issue icon if there is one
        if (directIssueType != IssueType.None)
        {
          GUIContent iconContent = directIssueType == IssueType.Error ? errorIconContent : warningIconContent;
          GUI.Label(new Rect(selectionRect.xMax - iconSize, selectionRect.y, iconSize, iconSize), iconContent);
        }
        // Draw child issue icon with arrow if there are child issues but no direct issues
        else if (childIssueType != IssueType.None)
        {
          // Draw the issue icon for child problems
          GUIContent iconContent = childIssueType == IssueType.Error ? errorIconContent : warningIconContent;
          // Create a copy of the icon content with a custom tooltip for child issues
          GUIContent childIconContent = new GUIContent(
            iconContent.image,
            childIssueType == IssueType.Error
              ? "CVRFury/VRCFury Error found in child objects"
              : "CVRFury/VRCFury Warning found in child objects"
          );
          GUI.Label(
            new Rect(selectionRect.xMax - iconSize - arrowSize, selectionRect.y, iconSize, iconSize),
            childIconContent
          );

          // Draw the arrow indicator
          if (arrowIconContent != null)
          {
            GUI.Label(
              new Rect(selectionRect.xMax - arrowSize, selectionRect.y + 2, arrowSize, arrowSize),
              arrowIconContent
            );
          }
        }
      }
      catch (System.Exception e)
      {
        Debug.LogError($"[CVRFury][Error] DrawHierarchyItem: {e.Message}");
      }
    }

    private static IssueType CheckForIssues(GameObject gameObject)
    {
      // Check for VRC stub components first - these are errors
      var issueType = CheckForVRCStubComponents(gameObject);
      if (issueType == IssueType.Error)
        return IssueType.Error;

      // Check for CVRAvatar body mesh issues
      if (HasCVRAvatarBodyMeshIssues(gameObject))
        return IssueType.Warning;

      var vrcFuryComponents = gameObject.GetComponents<VRCFury>();
      if (vrcFuryComponents == null || vrcFuryComponents.Length == 0)
        return IssueType.None;

      foreach (var vrcFury in vrcFuryComponents)
      {
        if (vrcFury == null)
          continue;

        SerializedObject serializedObject;
        if (!serializedObjectCache.TryGetValue(vrcFury, out serializedObject))
        {
          // Manage cache size
          if (serializedObjectCache.Count >= MaxCacheSize)
          {
            var oldestEntry = serializedObjectCache.First();
            oldestEntry.Value.Dispose();
            serializedObjectCache.Remove(oldestEntry.Key);
          }

          serializedObject = new SerializedObject(vrcFury);
          serializedObjectCache[vrcFury] = serializedObject;
        }
        else
        {
          serializedObject.Update(); // Refresh the serialized object
        }

        var version = serializedObject.FindProperty("version").intValue;
        if (version > Constants.MAX_VRCFURY_VERSION_DATA)
          return IssueType.Warning;

        if (version == 3)
        {
          var contentProperty = serializedObject.FindProperty("content");
          if (contentProperty == null || string.IsNullOrEmpty(contentProperty.managedReferenceFullTypename))
            return IssueType.Warning;

          var contentClassName = contentProperty.managedReferenceFullTypename.Split('.').Last();
          if (Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName))
            return IssueType.Warning;

          if (Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassName))
            return IssueType.Warning;
        }
      }
      return IssueType.None;
    }

    private static IssueType CheckForChildIssues(GameObject gameObject)
    {
      if (gameObject == null || gameObject.transform.childCount == 0)
        return IssueType.None;

      IssueType highestIssue = IssueType.None;

      // Recursively check all children (limited depth to prevent performance issues)
      return CheckForChildIssuesRecursive(gameObject, 0, 10); // Max depth of 10 levels
    }

    private static IssueType CheckForChildIssuesRecursive(GameObject gameObject, int currentDepth, int maxDepth)
    {
      if (gameObject == null || currentDepth >= maxDepth)
        return IssueType.None;

      IssueType highestIssue = IssueType.None;

      // Check all direct children
      for (int i = 0; i < gameObject.transform.childCount; i++)
      {
        GameObject child = gameObject.transform.GetChild(i).gameObject;
        if (child == null)
          continue;

        // Check for direct issues on this child
        IssueType childDirectIssue = CheckForIssues(child);
        if (childDirectIssue == IssueType.Error)
          return IssueType.Error; // Return early if we find an error

        if (childDirectIssue == IssueType.Warning && highestIssue == IssueType.None)
          highestIssue = IssueType.Warning;

        // Recursively check this child's children
        IssueType childNestedIssue = CheckForChildIssuesRecursive(child, currentDepth + 1, maxDepth);
        if (childNestedIssue == IssueType.Error)
          return IssueType.Error; // Return early if we find an error

        if (childNestedIssue == IssueType.Warning && highestIssue == IssueType.None)
          highestIssue = IssueType.Warning;
      }

      return highestIssue;
    }

    private static IssueType CheckForVRCStubComponents(GameObject gameObject)
    {
      var components = gameObject.GetComponents<Component>();
      foreach (var component in components)
      {
        if (component == null)
          continue;

        string typeName = component.GetType().FullName;
        if (Constants.VRCSTUB_COMPONENTS_TO_REMOVE.Contains(typeName))
        {
          return IssueType.Error;
        }
      }
      return IssueType.None;
    }

    private static bool HasCVRAvatarBodyMeshIssues(GameObject gameObject)
    {
      // Check if this object has a CVRAvatar component
      var avatar = gameObject.GetComponent("CVRAvatar");
      if (avatar == null)
        return false;

      try
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

        if (bodyMeshField == null)
        {
          // CCK compatibility issue
          return true;
        }

        var bodyMeshValue = bodyMeshField.GetValue(avatar);

        // Check for null or Unity's "null" object reference
        if (bodyMeshValue == null || (bodyMeshValue is UnityEngine.Object unityObj && unityObj == null))
        {
          // Body mesh is not set - this is a warning
          return true;
        }
        else if (bodyMeshValue is UnityEngine.Object meshObj && meshObj != null)
        {
          // Now check if it's a SkinnedMeshRenderer
          if (meshObj is SkinnedMeshRenderer skinnedMeshRenderer)
          {
            GameObject meshGameObject = skinnedMeshRenderer.gameObject;
            if (!meshGameObject.transform.IsChildOf(gameObject.transform))
            {
              // Body mesh is not on a child gameObject of the Avatar - WARNING (changed from ERROR)
              return true;
            }
          }
          else
          {
            // Body mesh is not a valid SkinnedMeshRenderer reference - WARNING (changed from ERROR)
            return true;
          }
        }
        else
        {
          // bodyMeshValue is some other unexpected type - WARNING
          return true;
        }
      }
      catch (System.Exception)
      {
        // Error checking body mesh
        return true;
      }

      // Check Animator component and avatar
      var animatorComponent = gameObject.GetComponent<Animator>();
      if (animatorComponent != null)
      {
        if (animatorComponent.avatar == null)
        {
          // Animator Avatar is not set - this is a warning
          return true;
        }
      }
      else
      {
        // Animator component is missing - this is a WARNING (changed from ERROR)
        return true;
      }

      return false;
    }

    #region Menu Items
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Options/Hierarchy Icons Enable";

    [MenuItem(MENU_PATH, false, -100)]
    private static void ToggleHierarchyIcons()
    {
      bool currentValue = EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true);
      EditorPrefs.SetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, !currentValue);

      if (currentValue)
      {
        CoreLogDebug("Disabling Hierarchy Icons");
        Cleanup();
      }
      else
      {
        CoreLogDebug("Enabling Hierarchy Icons");
        Initialize();
      }

      EditorApplication.RepaintHierarchyWindow();
    }

    [MenuItem(MENU_PATH, true, -100)]
    private static bool ToggleHierarchyIconsValidation()
    {
      Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true));
      return true;
    }
    #endregion
  }
}
#endif
