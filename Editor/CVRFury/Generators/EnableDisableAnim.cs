#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class EnableDisableAnim : EditorWindow
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

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Generation Tools/Enable-Disable Animation")]
    public static void ShowWindow()
    {
      var window = GetWindow<EnableDisableAnim>();
      window.titleContent = new GUIContent("Enable-Disable Animation");
      window.minSize = new Vector2(400, 200);
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

      // Load saved path
      animationPath = EditorPrefs.GetString(LAST_PATH_PREF_KEY, "Assets");
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
      // Check and create directory if needed
      if (!Directory.Exists(animationPath))
      {
        try
        {
          Directory.CreateDirectory(animationPath);
        }
        catch (System.Exception ex)
        {
          EditorUtility.DisplayDialog("Error", $"Failed to create directory: {ex.Message}", "OK");
          return;
        }
      }

      // Create the animation clips
      AnimationClip enableClip = new AnimationClip();
      AnimationClip disableClip = new AnimationClip();

      enableClip.name = $"{animationName}_enable";
      disableClip.name = $"{animationName}_disable";

      // Get relative path between root and target
      string targetPath = GetGameObjectPath(targetGameObject);
      string rootPath = GetGameObjectPath(rootGameObject);

      // Get the path segments
      string[] targetSegments = targetPath.Split('/');
      string[] rootSegments = rootPath.Split('/');

      // Build relative path starting after root object
      List<string> relativeSegments = new List<string>();
      bool foundRoot = false;

      foreach (string segment in targetSegments)
      {
        if (foundRoot)
        {
          relativeSegments.Add(segment);
        }
        else if (segment == rootSegments[rootSegments.Length - 1])
        {
          foundRoot = true;
        }
      }

      string relativePath = string.Join("/", relativeSegments);

      // Create curve bindings
      EditorCurveBinding curveBinding = new EditorCurveBinding
      {
        path = relativePath,
        propertyName = "m_IsActive",
        type = typeof(GameObject)
      };

      // Create keyframes
      AnimationCurve enableCurve = new AnimationCurve(new Keyframe(0, 1));
      AnimationCurve disableCurve = new AnimationCurve(new Keyframe(0, 0));

      AnimationUtility.SetEditorCurve(enableClip, curveBinding, enableCurve);
      AnimationUtility.SetEditorCurve(disableClip, curveBinding, disableCurve);

      // Save the clips
      string enablePath = Path.Combine(animationPath, $"{enableClip.name}.anim").Replace("\\", "/");
      string disablePath = Path.Combine(animationPath, $"{disableClip.name}.anim").Replace("\\", "/");

      AssetDatabase.CreateAsset(enableClip, enablePath);
      AssetDatabase.CreateAsset(disableClip, disablePath);

      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Success", "Animation clips generated successfully", "OK");
    }
  }
}

#endif
