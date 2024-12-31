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
  public class MeshRendererParamAnimUI : EditorWindow
  {
    private const string LAST_PATH_PREF_KEY = "MeshRendererParamAnim_LastPath";

    private ObjectField rootGameObjectField;
    private ObjectField rendererField;
    private PopupField<string> rendererTypePopup;
    private PopupField<string> paramTypePopup;
    private FloatField minValueField;
    private FloatField maxValueField;
    private TextField animationNameField;
    private TextField animationPathField;
    private Button generateButton;
    private PopupField<string> parameterPopup;
    private Dictionary<string, string> parameterTypes = new Dictionary<string, string>();
    private TextField parameterFilterField;
    private List<string> allParameters = new List<string>();
    private Label warningLabel;

    private GameObject rootGameObject;
    private Component renderer; // Changed from MeshRenderer to Component
    private string paramType;
    private string paramName;
    private float minValue;
    private float maxValue;
    private string animationName;
    private string animationPath;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Generation Tools/Animation Clip/Mesh Renderer Parameter")]
    public static void ShowWindow()
    {
      ShowWindowWithPath("Assets");
    }

    [MenuItem("Assets/Create/CVRFury/Animation Clips/Mesh Renderer Parameter Animation", false, 1)]
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
      var window = GetWindow<MeshRendererParamAnimUI>();
      if (window == null)
      {
        Debug.LogError("Failed to create window");
        return;
      }
      window.titleContent = new GUIContent("Mesh Renderer Parameter Animation");
      window.minSize = new Vector2(400, 300);

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
      rendererField = rootVisualElement.Q<ObjectField>("meshRendererField");
      minValueField = rootVisualElement.Q<FloatField>("minValueField");
      maxValueField = rootVisualElement.Q<FloatField>("maxValueField");
      animationNameField = rootVisualElement.Q<TextField>("animationNameField");
      animationPathField = rootVisualElement.Q<TextField>("animationPathField");
      generateButton = rootVisualElement.Q<Button>("generateButton");
      parameterFilterField = rootVisualElement.Q<TextField>("parameterFilterField");
      warningLabel = rootVisualElement.Q<Label>("warningLabel");

      // Initialize parameter popup with empty list
      parameterPopup = new PopupField<string>("Parameter", new List<string> { "No parameters available" }, 0);
      var parameterPopupContainer = rootVisualElement.Q<VisualElement>("parameterPopupContainer");
      parameterPopupContainer.Add(parameterPopup);

      // Create parameter type popup
      paramTypePopup = new PopupField<string>("Type", new List<string> { "Bool", "Int", "Float" }, 0);
      var paramTypePopupContainer = rootVisualElement.Q<VisualElement>("paramTypePopupContainer");
      paramTypePopupContainer.Add(paramTypePopup);

      paramTypePopup.RegisterValueChangedCallback(evt =>
      {
        paramType = evt.newValue;
        UpdateMinMaxFieldVisibility();
        UpdateControlStates();
      });

      // Setup fields
      rootGameObjectField.objectType = typeof(GameObject);

      // Setup renderer type selection
      rendererTypePopup = new PopupField<string>(
        "Renderer Type",
        new List<string> { "MeshRenderer", "SkinnedMeshRenderer" },
        0
      );
      rootVisualElement.Q<VisualElement>("rendererContainer").Add(rendererTypePopup);

      // Update renderer field type based on selection
      rendererTypePopup.RegisterValueChangedCallback(evt =>
      {
        rendererField.objectType = evt.newValue == "MeshRenderer" ? typeof(MeshRenderer) : typeof(SkinnedMeshRenderer);
        renderer = null;
        rendererField.value = null;
        RefreshParameters();
        UpdateControlStates();
      });

      // Initialize renderer field
      rendererField.objectType = typeof(MeshRenderer);

      // Update renderer field callback
      rendererField.RegisterValueChangedCallback(evt =>
      {
        renderer = evt.newValue as Component;
        RefreshParameters();
        UpdateControlStates();
      });

      // Register callbacks
      rootGameObjectField.RegisterValueChangedCallback(evt =>
      {
        rootGameObject = evt.newValue as GameObject;
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

      // Add filter callback
      parameterFilterField.RegisterValueChangedCallback(evt =>
      {
        FilterParameters(evt.newValue);
      });

      // Load saved path or use the one set by ShowWindowWithPath
      if (string.IsNullOrEmpty(animationPath))
      {
        animationPath = EditorPrefs.GetString(LAST_PATH_PREF_KEY, "Assets");
      }
      animationPathField.value = animationPath;

      // Get reference to warning label
      warningLabel = rootVisualElement.Q<Label>("warningLabel");

      // Add value change callbacks for min/max fields
      minValueField.RegisterValueChangedCallback(evt =>
      {
        minValue = evt.newValue;
        UpdateWarningVisibility();
      });

      maxValueField.RegisterValueChangedCallback(evt =>
      {
        maxValue = evt.newValue;
        UpdateWarningVisibility();
      });

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
      allParameters.Clear(); // Clear all parameters
      var parameters = new List<string>();

      Material material = null;
      if (renderer != null)
      {
        if (renderer is MeshRenderer meshRenderer)
        {
          material = meshRenderer.sharedMaterial;
        }
        else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
          material = skinnedMeshRenderer.sharedMaterial;
        }
      }

      if (material != null)
      {
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

      // After gathering parameters, store them in allParameters
      allParameters = parameters;

      // Update the parameter popup with all parameters (or filtered if there's a filter)
      if (!string.IsNullOrEmpty(parameterFilterField?.value))
      {
        FilterParameters(parameterFilterField.value);
      }
      else
      {
        UpdateParameterPopup(allParameters);
      }
    }

    private void FilterParameters(string filterText)
    {
      if (string.IsNullOrEmpty(filterText))
      {
        UpdateParameterPopup(allParameters);
        return;
      }

      var filteredParams = allParameters.Where(p => p.ToLower().Contains(filterText.ToLower())).ToList();

      UpdateParameterPopup(filteredParams);
    }

    private void UpdateParameterPopup(List<string> parameters)
    {
      if (parameters.Count > 0)
      {
        var newParameterPopup = new PopupField<string>("Parameter", parameters, 0);
        var parameterPopupContainer = rootVisualElement.Q<VisualElement>("parameterPopupContainer");
        parameterPopupContainer.Clear();
        parameterPopup = newParameterPopup;
        parameterPopupContainer.Add(parameterPopup);

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
        var newParameterPopup = new PopupField<string>("Parameter", new List<string> { "No matching parameters" }, 0);
        var parameterPopupContainer = rootVisualElement.Q<VisualElement>("parameterPopupContainer");
        parameterPopupContainer.Clear();
        parameterPopup = newParameterPopup;
        parameterPopupContainer.Add(parameterPopup);
      }
    }

    private void UpdateWarningVisibility()
    {
      warningLabel.style.display =
        (paramType != "Bool" && Mathf.Approximately(minValue, maxValue)) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateMinMaxFieldVisibility()
    {
      if (paramType == "Bool")
      {
        minValueField.style.display = DisplayStyle.None;
        maxValueField.style.display = DisplayStyle.None;
        warningLabel.style.display = DisplayStyle.None;
        minValue = 0;
        maxValue = 1;
      }
      else
      {
        minValueField.style.display = DisplayStyle.Flex;
        maxValueField.style.display = DisplayStyle.Flex;
        UpdateWarningVisibility();
        minValue = minValueField.value;
        maxValue = maxValueField.value;
      }
    }

    private void UpdateControlStates()
    {
      bool hasRequiredFields =
        rootGameObject != null
        && renderer != null
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
            + $"renderer: {renderer != null}\n"
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

      var config = new MeshRendererParamAnimCreator.AnimationConfig
      {
        rootGameObject = rootGameObject,
        renderer = renderer,
        paramName = paramName,
        paramType = paramType,
        minValue = minValue,
        maxValue = maxValue,
        animationName = animationNameField.value,
        animationPath = animationPath
      };

      if (MeshRendererParamAnimCreator.GenerateAnimations(config))
      {
        EditorUtility.DisplayDialog("Success", "Animation clips generated successfully", "OK");
      }
      else
      {
        EditorUtility.DisplayDialog("Error", "Failed to generate animation clips", "OK");
      }
    }

    private bool ValidateInputs()
    {
      if (rootGameObject == null)
      {
        EditorUtility.DisplayDialog("Error", "Root GameObject is required", "OK");
        return false;
      }

      if (renderer == null)
      {
        EditorUtility.DisplayDialog("Error", "Renderer is required", "OK");
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

      if (paramType != "Bool" && Mathf.Approximately(minValue, maxValue))
      {
        bool proceed = EditorUtility.DisplayDialog(
          "Warning",
          "Min and Max values are the same. This will create identical animations. Do you want to proceed?",
          "Yes",
          "No"
        );
        if (!proceed)
          return false;
      }

      return true;
    }
  }
}
#endif
