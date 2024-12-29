#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class MeshRendererParamAnim : EditorWindow
  {
    private const string LAST_PATH_PREF_KEY = "MeshRendererParamAnim_LastPath";

    private ObjectField rootGameObjectField;
    private ObjectField meshRendererField;
    private PopupField<string> paramTypePopup;
    private FloatField minValueField;
    private FloatField maxValueField;
    private TextField animationNameField;
    private TextField animationPathField;
    private Button generateButton;
    private PopupField<string> parameterPopup;
    private Dictionary<string, string> parameterTypes = new Dictionary<string, string>();

    private GameObject rootGameObject;
    private MeshRenderer meshRenderer;
    private string paramType;
    private string paramName;
    private float minValue;
    private float maxValue;
    private string animationName;
    private string animationPath;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Generation Tools/Mesh Renderer Parameter Animation")]
    public static void ShowWindow()
    {
      var window = GetWindow<MeshRendererParamAnim>();
      window.titleContent = new GUIContent("Mesh Renderer Parameter Animation");
      window.minSize = new Vector2(400, 300);
    }

    private void CreateGUI()
    {
      // Load UXML
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityUXML/CVRFuryMeshRendererParamAnim"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return;
      }

      // Load and apply stylesheet
      var styleSheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityStyleSheets/CVRFuryMeshRendererParamAnim"
      );
      if (styleSheet != null)
        rootVisualElement.styleSheets.Add(styleSheet);

      // Instantiate UXML
      visualTree.CloneTree(rootVisualElement);

      // Get references to UI elements
      rootGameObjectField = rootVisualElement.Q<ObjectField>("rootGameObjectField");
      meshRendererField = rootVisualElement.Q<ObjectField>("meshRendererField");
      var paramContainer = rootVisualElement.Q<VisualElement>("paramContainer");
      minValueField = rootVisualElement.Q<FloatField>("minValueField");
      maxValueField = rootVisualElement.Q<FloatField>("maxValueField");
      animationNameField = rootVisualElement.Q<TextField>("animationNameField");
      animationPathField = rootVisualElement.Q<TextField>("animationPathField");
      generateButton = rootVisualElement.Q<Button>("generateButton");

      // Initialize parameter popup with empty list
      parameterPopup = new PopupField<string>("Parameter", new List<string> { "No parameters available" }, 0);
      paramContainer.Add(parameterPopup);

      // Create parameter type popup (will be updated based on selection)
      paramTypePopup = new PopupField<string>("Type", new List<string> { "Bool", "Int", "Float" }, 0);
      paramContainer.Add(paramTypePopup);

      paramTypePopup.RegisterValueChangedCallback(evt =>
      {
        paramType = evt.newValue;
        UpdateMinMaxFieldVisibility();
        UpdateControlStates();
      });

      // Setup fields
      rootGameObjectField.objectType = typeof(GameObject);
      meshRendererField.objectType = typeof(MeshRenderer);

      // Register callbacks
      rootGameObjectField.RegisterValueChangedCallback(evt =>
      {
        rootGameObject = evt.newValue as GameObject;
        UpdateControlStates();
      });

      meshRendererField.RegisterValueChangedCallback(evt =>
      {
        meshRenderer = evt.newValue as MeshRenderer;
        RefreshParameters();
        UpdateControlStates();
      });

      parameterPopup.RegisterValueChangedCallback(evt =>
      {
        paramName = evt.newValue;
        if (parameterTypes.ContainsKey(paramName))
        {
          paramType = parameterTypes[paramName];
          paramTypePopup.value = paramType;
          UpdateMinMaxFieldVisibility();
        }
        animationNameField.value = paramName.Replace("material.", "");
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

      // Add callback for animation name field
      animationNameField.RegisterValueChangedCallback(evt =>
      {
        animationName = evt.newValue;
        UpdateControlStates();
      });

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

    private void RefreshParameters()
    {
      parameterTypes.Clear();
      var parameters = new List<string>();

      if (meshRenderer != null && meshRenderer.sharedMaterial != null)
      {
        Material material = meshRenderer.sharedMaterial;
        Shader shader = material.shader;

        // Get all properties from the material's shader
        int propertyCount = shader.GetPropertyCount();
        for (int i = 0; i < propertyCount; i++)
        {
          var propType = shader.GetPropertyType(i);
          string propName = shader.GetPropertyName(i);

          // Get property attributes to check for toggle properties
          string[] attributes = shader.GetPropertyAttributes(i);
          bool isToggle = attributes != null && attributes.Any(attr => attr.ToLower().Contains("toggle"));

          switch (propType)
          {
            case ShaderPropertyType.Float:
            case ShaderPropertyType.Range:
              if (isToggle)
              {
                parameters.Add($"material.{propName}");
                parameterTypes[$"material.{propName}"] = "Bool";
              }
              else
              {
                parameters.Add($"material.{propName}");
                parameterTypes[$"material.{propName}"] = "Float";
              }
              break;
            case ShaderPropertyType.Color:
              // Add individual color channels
              parameters.Add($"material.{propName}.r");
              parameters.Add($"material.{propName}.g");
              parameters.Add($"material.{propName}.b");
              parameters.Add($"material.{propName}.a");
              parameterTypes[$"material.{propName}.r"] = "Float";
              parameterTypes[$"material.{propName}.g"] = "Float";
              parameterTypes[$"material.{propName}.b"] = "Float";
              parameterTypes[$"material.{propName}.a"] = "Float";
              break;
            case ShaderPropertyType.Vector:
              // Add individual vector components
              parameters.Add($"material.{propName}.x");
              parameters.Add($"material.{propName}.y");
              parameters.Add($"material.{propName}.z");
              parameters.Add($"material.{propName}.w");
              parameterTypes[$"material.{propName}.x"] = "Float";
              parameterTypes[$"material.{propName}.y"] = "Float";
              parameterTypes[$"material.{propName}.z"] = "Float";
              parameterTypes[$"material.{propName}.w"] = "Float";
              break;
          }
        }
      }

      // Update parameter popup
      if (parameters.Count > 0)
      {
        var newParameterPopup = new PopupField<string>("Parameter", parameters, 0);
        var paramContainer = rootVisualElement.Q<VisualElement>("paramContainer");
        paramContainer.Remove(parameterPopup);
        parameterPopup = newParameterPopup;
        paramContainer.Add(parameterPopup);

        // Register callback for new popup
        parameterPopup.RegisterValueChangedCallback(evt =>
        {
          paramName = evt.newValue;
          if (parameterTypes.ContainsKey(paramName))
          {
            paramType = parameterTypes[paramName];
            paramTypePopup.value = paramType;
            UpdateMinMaxFieldVisibility();
          }
          animationNameField.value = paramName.Replace("material.", "");
          UpdateControlStates();
        });

        // Set initial values
        if (parameters.Count > 0)
        {
          paramName = parameters[0];
          if (parameterTypes.ContainsKey(paramName))
          {
            paramType = parameterTypes[paramName];
            paramTypePopup.value = paramType;
            UpdateMinMaxFieldVisibility();
          }
          animationNameField.value = paramName.Replace("material.", "");
        }
      }
      else
      {
        var newParameterPopup = new PopupField<string>(
          "Parameter",
          new List<string> { "No material parameters available" },
          0
        );
        var paramContainer = rootVisualElement.Q<VisualElement>("paramContainer");
        paramContainer.Remove(parameterPopup);
        parameterPopup = newParameterPopup;
        paramContainer.Add(parameterPopup);
      }
    }

    private void UpdateMinMaxFieldVisibility()
    {
      if (paramType == "Bool")
      {
        minValueField.style.display = DisplayStyle.None;
        maxValueField.style.display = DisplayStyle.None;
        minValue = 0;
        maxValue = 1;
      }
      else
      {
        minValueField.style.display = DisplayStyle.Flex;
        maxValueField.style.display = DisplayStyle.Flex;
        minValue = minValueField.value;
        maxValue = maxValueField.value;
      }
    }

    private void UpdateControlStates()
    {
      bool hasRequiredFields =
        rootGameObject != null
        && meshRenderer != null
        && !string.IsNullOrEmpty(paramName)
        && !string.IsNullOrEmpty(animationPathField.value)
        && !string.IsNullOrEmpty(animationNameField.value);

      generateButton.SetEnabled(hasRequiredFields);

      // For debugging
      if (!hasRequiredFields)
      {
        Debug.Log(
          $"Generate button disabled because:\n"
            + $"rootGameObject: {rootGameObject != null}\n"
            + $"meshRenderer: {meshRenderer != null}\n"
            + $"paramName: {!string.IsNullOrEmpty(paramName)}\n"
            + $"animationPath: {!string.IsNullOrEmpty(animationPathField.value)}\n"
            + $"animationName: {!string.IsNullOrEmpty(animationNameField.value)}"
        );
      }
    }

    private void ValidateAndGenerate()
    {
      if (!ValidateInputs())
        return;
      Generate();
    }

    private bool ValidateInputs()
    {
      if (rootGameObject == null)
      {
        EditorUtility.DisplayDialog("Error", "Root GameObject is required", "OK");
        return false;
      }

      if (meshRenderer == null)
      {
        EditorUtility.DisplayDialog("Error", "Mesh Renderer is required", "OK");
        return false;
      }

      if (string.IsNullOrEmpty(paramName))
      {
        EditorUtility.DisplayDialog("Error", "Parameter name is required", "OK");
        return false;
      }

      if (string.IsNullOrEmpty(animationPath))
      {
        EditorUtility.DisplayDialog("Error", "Animation path is required", "OK");
        return false;
      }

      return true;
    }

    private void Generate()
    {
      // Create directory if needed
      if (!Directory.Exists(animationPath))
      {
        Directory.CreateDirectory(animationPath);
      }

      // Get relative path from root to mesh renderer
      string meshRendererPath = GetGameObjectPath(meshRenderer.gameObject);
      string rootPath = GetGameObjectPath(rootGameObject);
      string relativePath = meshRendererPath.Replace(rootPath, "").TrimStart('/');

      // Create animation clips
      AnimationClip minClip = new AnimationClip();
      AnimationClip maxClip = new AnimationClip();

      // Set clip names based on parameter type
      if (paramType == "Bool")
      {
        minClip.name = $"{animationName}_false";
        maxClip.name = $"{animationName}_true";
      }
      else
      {
        minClip.name = $"{animationName}_min";
        maxClip.name = $"{animationName}_max";
      }

      // Create curve binding
      EditorCurveBinding binding = new EditorCurveBinding
      {
        type = typeof(MeshRenderer),
        path = relativePath,
        propertyName = paramName
      };

      // Create curves based on parameter type
      switch (paramType)
      {
        case "Bool":
          AnimationCurve minBoolCurve = AnimationCurve.Constant(0, 0, 0);
          AnimationCurve maxBoolCurve = AnimationCurve.Constant(0, 0, 1);
          AnimationUtility.SetEditorCurve(minClip, binding, minBoolCurve);
          AnimationUtility.SetEditorCurve(maxClip, binding, maxBoolCurve);
          break;

        case "Int":
        case "Float":
          AnimationCurve minCurve = AnimationCurve.Constant(0, 0, minValue);
          AnimationCurve maxCurve = AnimationCurve.Constant(0, 0, maxValue);
          AnimationUtility.SetEditorCurve(minClip, binding, minCurve);
          AnimationUtility.SetEditorCurve(maxClip, binding, maxCurve);
          break;
      }

      // Save animation clips
      string minPath = Path.Combine(animationPath, $"{minClip.name}.anim").Replace("\\", "/");
      string maxPath = Path.Combine(animationPath, $"{maxClip.name}.anim").Replace("\\", "/");

      AssetDatabase.CreateAsset(minClip, minPath);
      AssetDatabase.CreateAsset(maxClip, maxPath);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Success", "Animation clips generated successfully", "OK");
    }
  }
}
#endif
