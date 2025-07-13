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

  [InitializeOnLoad]
  public static class HierarchyIcons
  {
    private static Dictionary<int, bool> gameObjectIssueCache = new Dictionary<int, bool>();
    private static bool isInitialized = false;
    private static GUIContent overlayIconContent;
    private static float iconSize = 16f;
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

      overlayIconContent = EditorGUIUtility.IconContent("console.warnicon");
      if (overlayIconContent != null)
      {
        overlayIconContent.tooltip = "CVRFury/VRCFury Issue";
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
      overlayIconContent = null;
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

      var parent = gameObject.transform.parent;
      while (parent != null)
      {
        gameObjectIssueCache.Remove(parent.gameObject.GetInstanceID());
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

        if (!gameObjectIssueCache.TryGetValue(instanceID, out bool hasIssues))
        {
          hasIssues = CheckForIssues(gameObject);
          gameObjectIssueCache[instanceID] = hasIssues;
        }

        if (hasIssues)
        {
          GUI.Label(new Rect(selectionRect.xMax - iconSize, selectionRect.y, iconSize, iconSize), overlayIconContent);
        }
      }
      catch (System.Exception e)
      {
        Debug.LogError($"[CVRFury][Error] DrawHierarchyItem: {e.Message}");
      }
    }

    private static bool CheckForIssues(GameObject gameObject)
    {
      // Check for CVRAvatar body mesh issues first
      if (HasCVRAvatarBodyMeshIssues(gameObject))
        return true;

      var vrcFuryComponents = gameObject.GetComponents<VRCFury>();
      if (vrcFuryComponents == null || vrcFuryComponents.Length == 0)
        return false;

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
          return true;

        if (version == 3)
        {
          var contentProperty = serializedObject.FindProperty("content");
          if (contentProperty == null || string.IsNullOrEmpty(contentProperty.managedReferenceFullTypename))
            return true;

          var contentClassName = contentProperty.managedReferenceFullTypename.Split('.').Last();
          if (Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName))
            return true;

          if (Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassName))
            return true;
        }
      }
      return false;
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

        if (bodyMeshValue == null)
        {
          // Body mesh is not set
          return true;
        }

        // Check if bodyMesh is a valid SkinnedMeshRenderer and is a child of the avatar
        if (bodyMeshValue is SkinnedMeshRenderer skinnedMeshRenderer)
        {
          GameObject meshGameObject = skinnedMeshRenderer.gameObject;
          if (!meshGameObject.transform.IsChildOf(gameObject.transform))
          {
            // Body mesh is not on a child gameObject of the Avatar
            return true;
          }
        }
        else
        {
          // Body mesh is not a valid SkinnedMeshRenderer reference
          return true;
        }
      }
      catch (System.Exception)
      {
        // Error checking body mesh
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
