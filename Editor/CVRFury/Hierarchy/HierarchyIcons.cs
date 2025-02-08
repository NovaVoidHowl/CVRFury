#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VF.Model;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.hierarchy
{
  [InitializeOnLoad]
  public class HierarchyIcons
  {
    private static Texture warningIcon;
    private static GUIStyle arrowStyle;
    private static bool stylesInitialized = false;
    private const string EDITOR_PREFS_KEY = Constants.HIERARCHY_ICONS_STATE_PREF;

    static HierarchyIcons()
    {
      EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void InitializeStyles()
    {
      if (stylesInitialized)
        return;

      // Try to load the warning icon with fallbacks
      warningIcon = EditorGUIUtility.Load("Icons/console.warnicon.sml") as Texture2D;
      if (warningIcon == null)
        warningIcon = EditorGUIUtility.IconContent("console.warnicon").image;
      if (warningIcon == null)
        warningIcon = EditorGUIUtility.IconContent("Warning").image;

      // Create arrow style only when EditorStyles is ready
      if (EditorStyles.label != null)
      {
        arrowStyle = new GUIStyle(EditorStyles.label)
        {
          fontSize = 13,
          alignment = TextAnchor.MiddleLeft,
          fontStyle = FontStyle.Bold
        };
        arrowStyle.normal.textColor = new Color(1f, 0.7f, 0f); // Orange color for arrow
        stylesInitialized = true;
      }
    }

    private static bool CheckForIssuesInChildren(GameObject gameObject)
    {
      // Check all children recursively
      foreach (Transform child in gameObject.transform)
      {
        // Check if the child has any VRCFury components with issues
        var childVrcFuryComponents = child.GetComponents<VRCFury>();
        foreach (var vrcFury in childVrcFuryComponents)
        {
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

        // Recursively check children
        if (CheckForIssuesInChildren(child.gameObject))
          return true;
      }

      return false;
    }

    private static bool IsExpanded(int instanceID)
    {
      try
      {
        var sceneHierarchy = EditorWindow.GetWindow(
          typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow")
        );
        if (sceneHierarchy == null)
          return false;

        var treeViewStateField = sceneHierarchy
          .GetType()
          .GetField("m_TreeViewState", BindingFlags.Instance | BindingFlags.NonPublic);
        if (treeViewStateField == null)
          return false;

        var treeViewState = treeViewStateField.GetValue(sceneHierarchy);
        if (treeViewState == null)
          return false;

        var expandedIDsField = treeViewState
          .GetType()
          .GetField("expandedIDs", BindingFlags.Instance | BindingFlags.Public);
        if (expandedIDsField == null)
          return false;

        var expandedIds = expandedIDsField.GetValue(treeViewState) as System.Collections.Generic.List<int>;
        return expandedIds != null && expandedIds.Contains(instanceID);
      }
      catch
      {
        return false;
      }
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
      // Early exit if icons are disabled
      if (!EditorPrefs.GetBool(EDITOR_PREFS_KEY, true))
        return;

      if (!stylesInitialized)
      {
        InitializeStyles();
        if (!stylesInitialized)
          return;
      }

      GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
      if (gameObject == null)
        return;

      bool hasDirectIssues = false;
      bool hasChildIssues = false;

      // Check direct components first
      var vrcFuryComponents = gameObject.GetComponents<VRCFury>();
      if (vrcFuryComponents != null && vrcFuryComponents.Length > 0)
      {
        foreach (var vrcFury in vrcFuryComponents)
        {
          SerializedObject serializedObject = new SerializedObject(vrcFury);
          int version = serializedObject.FindProperty("version").intValue;

          if (version > Constants.MAX_VRCFURY_VERSION_DATA)
          {
            hasDirectIssues = true;
            break;
          }
          else if (version == 3)
          {
            var contentProperty = serializedObject.FindProperty("content");
            if (contentProperty == null || string.IsNullOrEmpty(contentProperty.managedReferenceFullTypename))
            {
              hasDirectIssues = true;
              break;
            }
            else
            {
              string contentClassName = contentProperty.managedReferenceFullTypename.Split('.').Last();
              if (Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName))
              {
                hasDirectIssues = true;
                break;
              }
            }
          }
        }
      }

      // Calculate icon positions
      float iconSize = 16f;
      float arrowWidth = 20f;
      Rect iconRect = new Rect(selectionRect.xMax - iconSize - 2, selectionRect.y, iconSize, iconSize);
      Rect arrowRect = new Rect(selectionRect.xMax - iconSize - arrowWidth - 2, selectionRect.y, arrowWidth, iconSize);

      // For objects with children, check if we need to show child issue indicators
      if (gameObject.transform.childCount > 0)
      {
        bool isExpanded = IsExpanded(instanceID);
        if (!isExpanded)
        {
          hasChildIssues = CheckForIssuesInChildren(gameObject);

          // Draw child issue indicators only when collapsed
          if (hasChildIssues && !hasDirectIssues)
          {
            if (warningIcon != null)
            {
              GUI.DrawTexture(iconRect, warningIcon);
              if (arrowStyle != null)
              {
                Color originalColor = GUI.color;
                GUI.color = new Color(1f, 0.7f, 0f, 1f);
                GUI.Label(arrowRect, "→", arrowStyle);
                GUI.color = originalColor;
              }
            }
          }
        }
      }

      // Always show direct issues
      if (hasDirectIssues && warningIcon != null)
      {
        GUI.DrawTexture(iconRect, warningIcon);
      }

      // Handle repaint without blocking window switching
      if (Event.current != null && Event.current.type == EventType.Layout)
      {
        EditorApplication.delayCall += () => EditorApplication.RepaintHierarchyWindow();
      }
    }
  }

  public class HierarchyIconsOptionMenu
  {
    private const string MENU_PATH = "NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Options/Hierarchy Icons Enable";
    private const string EDITOR_PREFS_KEY = Constants.HIERARCHY_ICONS_STATE_PREF;

    [MenuItem(MENU_PATH, false, -100)]
    private static void ToggleAvatarInfoOverlay()
    {
      bool currentValue = EditorPrefs.GetBool(EDITOR_PREFS_KEY, true);
      EditorPrefs.SetBool(EDITOR_PREFS_KEY, !currentValue);

      // Force hierarchy window to repaint when preference changes
      EditorApplication.RepaintHierarchyWindow();
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
