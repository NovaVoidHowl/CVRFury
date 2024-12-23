// this editor script generates a pair of animations for the chosen blendshape,
// one that sets the blendshape to its minimum value and one that sets it to its maximum value
#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class BlendShapeMinMaxAnim : EditorWindow
  {
    private const string LAST_PATH_PREF_KEY = "BlendShapeMinMaxAnim_LastPath";

    private VisualElement rootElement;
    private VisualElement blendShapeContainer;

    private ObjectField rootGameObjectField;
    private ObjectField meshRendererField;
    private PopupField<string> blendShapePopup;
    private TextField animationNameField;
    private TextField animationPathField;
    private Button generateButton;

    private GameObject rootGameObject;
    private SkinnedMeshRenderer meshRenderer;
    private string blendShapeName;
    private string animationName;
    private string animationPath;
    private string[] blendShapeNames;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Generation Tools/Min-Max BlendShape Animation")]
    public static void ShowWindow()
    {
      var window = GetWindow<BlendShapeMinMaxAnim>();
      window.titleContent = new GUIContent("Min-Max BlendShape Animation");
      window.minSize = new Vector2(400, 250);
    }

    private void CreateGUI()
    {
      // Load UXML
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityUXML/CVRFuryBlendShapeMinMaxAnim"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return;
      }

      // Load and apply stylesheet
      var styleSheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryGenUI/UnityStyleSheets/CVRFuryBlendShapeMinMaxAnim"
      );
      if (styleSheet != null)
        rootVisualElement.styleSheets.Add(styleSheet);

      // Instantiate UXML
      visualTree.CloneTree(rootVisualElement);

      // Get references to UI elements
      rootGameObjectField = rootVisualElement.Q<ObjectField>("rootGameObjectField");
      meshRendererField = rootVisualElement.Q<ObjectField>("meshRendererField");
      blendShapeContainer = rootVisualElement.Q<VisualElement>("blendShapeContainer");
      animationNameField = rootVisualElement.Q<TextField>("animationNameField");
      animationPathField = rootVisualElement.Q<TextField>("animationPathField");
      generateButton = rootVisualElement.Q<Button>("generateButton");

      // Create and setup PopupField
      blendShapePopup = new PopupField<string>("BlendShape Name", new List<string> { "No blendshapes available" }, 0);
      blendShapeContainer.Add(blendShapePopup);

      // Setup ObjectFields
      rootGameObjectField.objectType = typeof(GameObject);
      rootGameObjectField.value = rootGameObject;
      meshRendererField.objectType = typeof(SkinnedMeshRenderer);
      meshRendererField.value = meshRenderer;

      // Register callbacks
      rootGameObjectField.RegisterValueChangedCallback(evt =>
      {
        rootGameObject = evt.newValue as GameObject;
        UpdateControlStates();
      });

      meshRendererField.RegisterValueChangedCallback(evt =>
      {
        meshRenderer = evt.newValue as SkinnedMeshRenderer;
        RefreshBlendShapeNames();
      });

      blendShapePopup.RegisterValueChangedCallback(evt =>
      {
        blendShapeName = evt.newValue;
        // set the default animation name to the blendshape name
        string normalizedName = evt.newValue.Replace(" ", "_");
        animationNameField.value = normalizedName;
        animationName = normalizedName;
        UpdateControlStates();
      });

      animationNameField.RegisterValueChangedCallback(evt =>
      {
        animationName = evt.newValue;
        UpdateControlStates();
      });

      // on click of animationPathField open trigger the BrowsePath method
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

      // Initial state refresh/load
      RefreshBlendShapeNames();
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

    private void RefreshBlendShapeNames()
    {
      if (meshRenderer != null && meshRenderer.sharedMesh != null)
      {
        var names = new List<string>();
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
        {
          names.Add(meshRenderer.sharedMesh.GetBlendShapeName(i));
        }
        blendShapeNames = names.ToArray();

        // Create new PopupField with updated choices and initial selection
        if (names.Count > 0)
        {
          var newPopup = new PopupField<string>("BlendShape Name", names, 0);
          blendShapeContainer.Remove(blendShapePopup);
          blendShapePopup = newPopup;
          blendShapeContainer.Add(blendShapePopup);

          // Set the first blendshape as the default
          blendShapeName = blendShapeNames[0];

          // Re-register the callback with normalized name update
          blendShapePopup.RegisterValueChangedCallback(evt =>
          {
            blendShapeName = evt.newValue;
            string normalizedName = evt.newValue.Replace(" ", "_");
            animationNameField.value = normalizedName;
            animationName = normalizedName;
            UpdateControlStates();
          });
        }
      }
      else
      {
        blendShapeNames = new string[0];
        var newPopup = new PopupField<string>("BlendShape Name", new List<string> { "No blendshapes available" }, 0);
        blendShapeContainer.Remove(blendShapePopup);
        blendShapePopup = newPopup;
        blendShapeContainer.Add(blendShapePopup);
      }

      UpdateControlStates();
    }

    private void UpdateControlStates()
    {
      bool hasBlendShapes = blendShapeNames != null && blendShapeNames.Length > 0;
      blendShapePopup.SetEnabled(hasBlendShapes);
      generateButton.SetEnabled(
        rootGameObjectField.value != null
          && meshRenderer != null
          && !string.IsNullOrEmpty(blendShapeName)
          && !string.IsNullOrEmpty(animationName)
          && !string.IsNullOrEmpty(animationPath)
      );
    }

    private void ValidateAndGenerate()
    {
      if (rootGameObjectField.value == null)
      {
        EditorUtility.DisplayDialog("Error", "Root GameObject is null", "OK");
        return;
      }

      if (meshRenderer == null)
      {
        EditorUtility.DisplayDialog("Error", "Mesh Renderer is null", "OK");
        return;
      }

      if (string.IsNullOrEmpty(blendShapeName))
      {
        EditorUtility.DisplayDialog("Error", "BlendShape Name is empty", "OK");
        return;
      }

      if (string.IsNullOrEmpty(animationName))
      {
        EditorUtility.DisplayDialog("Error", "Animation Name is empty", "OK");
        return;
      }

      if (string.IsNullOrEmpty(animationPath))
      {
        EditorUtility.DisplayDialog("Error", "Animation Path is empty", "OK");
        return;
      }

      Generate();
    }

    private void Generate()
    {
      // First check and create directory if needed
      string finalPath = animationPath;
      if (!Directory.Exists(finalPath))
      {
        try
        {
          Directory.CreateDirectory(finalPath);
        }
        catch (System.Exception ex)
        {
          EditorUtility.DisplayDialog("Error", $"Failed to create directory: {ex.Message}", "OK");
          return;
        }
      }

      // get the blendshape index
      int blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);

      if (blendShapeIndex == -1)
      {
        EditorUtility.DisplayDialog("Error", "BlendShape not found", "OK");
        return;
      }

      // create the animation clip
      AnimationClip animationClipMin = new AnimationClip();
      AnimationClip animationClipMax = new AnimationClip();

      // set the animation clip name
      animationClipMin.name = animationName + "_min";
      animationClipMax.name = animationName + "_max";

      // get game object paths
      string meshRendererGameObjectPath = GetGameObjectPath(meshRenderer.gameObject);
      string rootGameObjectPath = GetGameObjectPath(rootGameObject);

      // remove the root game object path from the mesh renderer game object path
      string relativePath = meshRendererGameObjectPath.Replace(rootGameObjectPath, "");

      // if present, remove the leading slash
      if (relativePath.StartsWith("/"))
      {
        relativePath = relativePath.Substring(1);
      }

      // create the curve bindings
      EditorCurveBinding curveBindingMin = new EditorCurveBinding();
      curveBindingMin.type = typeof(SkinnedMeshRenderer);
      curveBindingMin.path = relativePath;
      curveBindingMin.propertyName = "blendShape." + blendShapeName;

      EditorCurveBinding curveBindingMax = new EditorCurveBinding();
      curveBindingMax.type = typeof(SkinnedMeshRenderer);
      curveBindingMax.path = relativePath;
      curveBindingMax.propertyName = "blendShape." + blendShapeName;

      // create the keyframes
      Keyframe[] keyframesMin = new Keyframe[1];
      keyframesMin[0] = new Keyframe(0, 0);

      Keyframe[] keyframesMax = new Keyframe[1];
      keyframesMax[0] = new Keyframe(0, 100);

      // create the animation curves
      AnimationCurve curveMin = new AnimationCurve(keyframesMin);
      AnimationCurve curveMax = new AnimationCurve(keyframesMax);

      // set the curve bindings and curves
      AnimationUtility.SetEditorCurve(animationClipMin, curveBindingMin, curveMin);
      AnimationUtility.SetEditorCurve(animationClipMax, curveBindingMax, curveMax);

      // Fix the path concatenation when saving
      string minPath = Path.Combine(finalPath, $"{animationClipMin.name}.anim").Replace("\\", "/");
      string maxPath = Path.Combine(finalPath, $"{animationClipMax.name}.anim").Replace("\\", "/");

      // save the animation clips
      AssetDatabase.CreateAsset(animationClipMin, minPath);
      AssetDatabase.CreateAsset(animationClipMax, maxPath);

      // refresh the asset database
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Success", "Animation clips generated", "OK");
    }
  }
}
#endif // UNITY_EDITOR
