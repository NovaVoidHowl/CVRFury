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
    private static HashSet<int> dirtyObjects = new HashSet<int>();
    private static GUIContent iconContent;
    private static System.DateTime lastRepaintTime;

    static HierarchyIcons()
    {
      // Ensure we're not in play mode or compiling
      if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
      {
        Debug.Log("[CVRFury] HierarchyIcons constructor called");
        EditorApplication.delayCall += () =>
        {
          if (EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true))
          {
            Initialize();
            EditorApplication.RepaintHierarchyWindow();
          }
        };
      }
    }

    public static void Initialize()
    {
      if (isInitialized)
        return;

      CoreLogDebug("Initializing Hierarchy Icons");

      iconContent = EditorGUIUtility.IconContent("console.warnicon");
      if (iconContent != null)
      {
        iconContent.tooltip = "VRCFury Issue";
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

      gameObjectIssueCache.Clear();
      iconContent = null;
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

      dirtyObjects.Add(gameObject.GetInstanceID());

      Transform parent = gameObject.transform.parent;
      while (parent != null)
      {
        dirtyObjects.Add(parent.gameObject.GetInstanceID());
        parent = parent.parent;
      }

      if ((System.DateTime.Now - lastRepaintTime).TotalMilliseconds > 250)
      {
        lastRepaintTime = System.DateTime.Now;
        EditorApplication.delayCall += () => EditorApplication.RepaintHierarchyWindow();
      }
    }

    private static void DrawHierarchyItem(int instanceID, Rect selectionRect)
    {
      try
      {
        if (
          !EditorPrefs.GetBool(HierarchyIconsConstants.EDITOR_PREFS_KEY, true)
          || EditorApplication.isPlaying
          || iconContent == null
        )
          return;

        if (Event.current.type != EventType.Repaint)
          return;

        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
          return;

        bool hasIssues = false;
        if (!gameObjectIssueCache.TryGetValue(instanceID, out hasIssues))
        {
          hasIssues = CheckForIssues(gameObject);
          gameObjectIssueCache[instanceID] = hasIssues;
        }

        if (hasIssues)
        {
          var iconRect = new Rect(selectionRect.xMax - 16f, selectionRect.y, 16f, 16f);
          GUI.Label(iconRect, iconContent);
        }
      }
      catch (System.Exception e)
      {
        Debug.LogError($"[CVRFury][Error] DrawHierarchyItem: {e.Message}");
      }
    }

    private static bool CheckForIssues(GameObject gameObject)
    {
      if (gameObject == null)
        return false;

      var vrcFuryComponents = gameObject.GetComponents<VRCFury>();
      if (vrcFuryComponents == null || vrcFuryComponents.Length == 0)
        return false;

      foreach (var vrcFury in vrcFuryComponents)
      {
        if (vrcFury == null)
          continue;

        SerializedObject serializedObject = new SerializedObject(vrcFury);
        int version = serializedObject.FindProperty("version").intValue;

        if (version > Constants.MAX_VRCFURY_VERSION_DATA)
          return true;

        if (version == 3)
        {
          var contentProperty = serializedObject.FindProperty("content");
          if (contentProperty == null || string.IsNullOrEmpty(contentProperty.managedReferenceFullTypename))
            return true;

          string contentClassName = contentProperty.managedReferenceFullTypename.Split('.').Last();
          if (Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName))
            return true;
        }
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
