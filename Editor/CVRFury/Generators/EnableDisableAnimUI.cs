#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class EnableDisableAnimUI : EditorWindow
  {
    private const string LAST_PATH_PREF_KEY = "EnableDisableAnim_LastPath";

    private ObjectField targetGameObjectField;
    private ObjectField rootGameObjectField;
    private TextField animationNameField;
    private TextField animationPathField;
    private Button generateButton;

    private GameObject targetGameObject;
    private GameObject rootGameObject;
    private string animationName;
    private string animationPath;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Generation Tools/Animation Clip/GameObject Enable Disable")]
    public static void ShowWindow()
    {
      ShowWindowWithPath("Assets");
    }

    [MenuItem("Assets/Create/CVRFury/Animation Clips/Enable-Disable Animation", false, 2)]
    private static void ShowWindowFromContext()
    {
      string path = "Assets";
      Object selected = Selection.activeObject;
      if (selected != null)
      {
        path = AssetDatabase.GetAssetPath(selected);
        if (!Directory.Exists(path))
        {
          path = Path.GetDirectoryName(path);
        }
        Debug.Log($"Selected path: {path}");
      }
      ShowWindowWithPath(path);
    }

    private static void ShowWindowWithPath(string path)
    {
      var window = GetWindow<EnableDisableAnimUI>();
      if (window == null)
      {
        Debug.LogError("Failed to create window");
        return;
      }
      window.titleContent = new GUIContent("Enable-Disable Animation");
      window.minSize = new Vector2(400, 200);

      window.animationPath = path;
      EditorPrefs.SetString(LAST_PATH_PREF_KEY, path);

      if (window.animationPathField != null)
      {
        window.animationPathField.value = path;
      }
    }

    private void CreateGUI()
    {
      // Load UXML
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityUXML/CVRFuryEnableDisableAnim"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return;
      }

      // Load and apply stylesheet
      var styleSheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityStyleSheets/CVRFuryEnableDisableAnim"
      );
      if (styleSheet != null)
        rootVisualElement.styleSheets.Add(styleSheet);

      // Instantiate UXML
      visualTree.CloneTree(rootVisualElement);

      // Get references to UI elements
      targetGameObjectField = rootVisualElement.Q<ObjectField>("targetGameObjectField");
      rootGameObjectField = rootVisualElement.Q<ObjectField>("rootGameObjectField");
      animationNameField = rootVisualElement.Q<TextField>("animationNameField");
      animationPathField = rootVisualElement.Q<TextField>("animationPathField");
      generateButton = rootVisualElement.Q<Button>("generateButton");

      // Setup ObjectFields
      targetGameObjectField.objectType = typeof(GameObject);
      rootGameObjectField.objectType = typeof(GameObject);

      // Register callbacks
      targetGameObjectField.RegisterValueChangedCallback(evt =>
      {
        targetGameObject = evt.newValue as GameObject;
        if (targetGameObject != null)
        {
          animationNameField.value = targetGameObject.name;
          animationName = targetGameObject.name;
        }
        UpdateControlStates();
      });

      rootGameObjectField.RegisterValueChangedCallback(evt =>
      {
        rootGameObject = evt.newValue as GameObject;
        UpdateControlStates();
      });

      animationNameField.RegisterValueChangedCallback(evt =>
      {
        animationName = evt.newValue;
        UpdateControlStates();
      });

      animationPathField.RegisterCallback<MouseDownEvent>(evt =>
      {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
          BrowsePath();
        }
      });

      generateButton.clicked += ValidateAndGenerate;

      // Load saved path or use the one set by ShowWindowWithPath
      if (string.IsNullOrEmpty(animationPath))
      {
        animationPath = EditorPrefs.GetString(LAST_PATH_PREF_KEY, "Assets");
      }
      animationPathField.value = animationPath;

      UpdateControlStates();
    }

    private void BrowsePath()
    {
      string startPath = string.IsNullOrEmpty(animationPath) ? "Assets" : animationPath;
      string absolutePath = EditorUtility.OpenFolderPanel("Select Animation Folder", startPath, "");

      if (!string.IsNullOrEmpty(absolutePath))
      {
        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.Length - 6);

        if (absolutePath.StartsWith(projectPath))
        {
          string relativePath = absolutePath.Substring(projectPath.Length);
          animationPath = relativePath;
          animationPathField.value = relativePath;
          EditorPrefs.SetString(LAST_PATH_PREF_KEY, relativePath);
        }
        else
        {
          EditorUtility.DisplayDialog("Invalid Path", "Please select a folder under the Assets folder", "OK");
        }
      }
    }

    private void UpdateControlStates()
    {
      generateButton.SetEnabled(
        targetGameObject != null
          && rootGameObject != null
          && !string.IsNullOrEmpty(animationName)
          && !string.IsNullOrEmpty(animationPath)
      );
    }

    private void ValidateAndGenerate()
    {
      if (targetGameObject == null || rootGameObject == null)
      {
        EditorUtility.DisplayDialog("Error", "Both Target and Root GameObjects must be set", "OK");
        return;
      }

      Generate();
    }

    private void Generate()
    {
      string errorMessage;
      bool success = EnableDisableAnimCreator.CreateAnimations(
        targetGameObject,
        rootGameObject,
        animationName,
        animationPath,
        out errorMessage
      );

      if (success)
      {
        EditorUtility.DisplayDialog("Success", "Animation clips generated successfully", "OK");
      }
      else
      {
        EditorUtility.DisplayDialog("Error", errorMessage, "OK");
      }
    }
  }
}
#endif
