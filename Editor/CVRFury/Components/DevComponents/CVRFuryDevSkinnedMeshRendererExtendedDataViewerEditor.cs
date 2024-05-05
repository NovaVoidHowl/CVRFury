//#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  [CustomEditor(typeof(CVRFuryDevSkinnedMeshRendererExtendedDataViewer))]
  public class CVRFuryDevSkinnedMeshRendererExtendedDataViewerEditor : Editor
  {
    // refresh button
    private Button _refreshButton;

    // base visual element of the component
    private VisualElement _baseVisualElement;

    // skinned mesh renderer component
    private SkinnedMeshRenderer _skinnedMeshRenderer = null;

    // path to the USS stylesheet
    private readonly string _pathToStyleSheet =
      Constants.PROGRAM_DISPLAY_NAME
      + "/CVRFuryComponents/UnityStyleSheets/CVRFuryDevSkinnedMeshRendererExtendedDataViewer";

    public override VisualElement CreateInspectorGUI()
    {
      _baseVisualElement = new VisualElement();

      // set the name of the component to allow for styling
      _baseVisualElement.name = "CVRFuryDevSkinnedMeshRendererExtendedDataViewer";

      _refreshButton = new Button(OnRefreshButtonClicked) { text = "Refresh" };
      _baseVisualElement.Add(_refreshButton);

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(_pathToStyleSheet);

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at '"
            + _pathToStyleSheet
            + "'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        _baseVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
      }
      // Apply the StyleSheet to the root VisualElement
      _baseVisualElement.styleSheets.Add(stylesheet);

      return _baseVisualElement;
    }

    // when the refresh button is clicked
    private void OnRefreshButtonClicked()
    {
      // clear the base visual element of the component
      _baseVisualElement.Clear();

      // set the name of the component to allow for styling
      _baseVisualElement.name = "CVRFuryDevSkinnedMeshRendererExtendedDataViewer";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(_pathToStyleSheet);

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at '"
            + _pathToStyleSheet
            + "'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        _baseVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
      }
      // Apply the StyleSheet to the root VisualElement
      _baseVisualElement.styleSheets.Add(stylesheet);

      // add the refresh button back to the base visual element of the component
      _baseVisualElement.Add(_refreshButton);

      // get the skinned mesh renderer component
      _skinnedMeshRenderer = (
        (CVRFuryDevSkinnedMeshRendererExtendedDataViewer)target
      ).gameObject.GetComponent<SkinnedMeshRenderer>();

      // if the skinned mesh renderer component is null
      if (_skinnedMeshRenderer == null)
      {
        // create a new message box visual element
        var messageBox = new Label("Skinned Mesh Renderer component not found");

        // add the message box to the base visual element of the component
        _baseVisualElement.Add(messageBox);
      }
      else
      {
        // we now have the skinned mesh renderer component, so can show extra data about it like the bones list
        // create new visual element to show the extended data
        var extendedDataVisualElement = new VisualElement();

        // add the extended data visual element to the base visual element of the component
        _baseVisualElement.Add(extendedDataVisualElement);

        // create a new label
        var label = new Label("Extended Data");

        // add the label to the extended data visual element
        extendedDataVisualElement.Add(label);

        // create a new foldout
        var foldout = new Foldout { value = ((CVRFuryDevSkinnedMeshRendererExtendedDataViewer)target).foldoutState };
        foldout.RegisterValueChangedCallback(
          evt => ((CVRFuryDevSkinnedMeshRendererExtendedDataViewer)target).foldoutState = evt.newValue
        );

        // add the foldout to the extended data visual element
        extendedDataVisualElement.Add(foldout);

        // create a new visual element
        var content = new VisualElement();

        // add the content to the foldout
        foldout.Add(content);

        // create a new container
        var container = new VisualElement();
        // set the class of the container
        container.AddToClassList("container");

        #region Mesh reference
        // create a new label
        var meshLabel = new Label("Mesh");
        container.Add(meshLabel);

        // create a new value viewer for the mesh
        var meshValue = _skinnedMeshRenderer.sharedMesh;

        // create a parent VisualElement for the border effect
        var meshFieldBorder = new VisualElement();
        meshFieldBorder.AddToClassList("meshFieldBorder");
        container.Add(meshFieldBorder);

        // create a custom VisualElement that behaves like a disabled ObjectField
        var meshField = new VisualElement();
        meshField.AddToClassList("meshField");
        meshFieldBorder.Add(meshField); // add the meshField to the meshFieldBorder

        // create an Image to display the mesh icon
        var meshIcon = new Image();
        meshIcon.image = EditorGUIUtility.ObjectContent(meshValue, typeof(Mesh)).image;
        meshIcon.AddToClassList("meshIcon");
        meshField.Add(meshIcon);

        // create a label to display the mesh name
        var meshNameLabel = new Label(meshValue != null ? meshValue.name : "None");
        meshField.Add(meshNameLabel);

        // handle the MouseDownEvent to select the mesh in the project
        meshField.RegisterCallback<MouseDownEvent>(evt =>
        {
          if (meshValue != null)
          {
            EditorGUIUtility.PingObject(meshValue);
          }
        });

        // add the mesh value to the container
        container.Add(meshFieldBorder);

        // add the container to the content
        content.Add(container);
        #endregion

        #region Root Bone reference
        // create a new container for the root bone
        var rootBoneContainer = new VisualElement();
        rootBoneContainer.AddToClassList("container");

        // create a new label for the root bone
        var rootBoneLabel = new Label("Root Bone");
        rootBoneContainer.Add(rootBoneLabel);

        // get the root bone value
        var rootBoneValue = _skinnedMeshRenderer.rootBone;

        // create a parent VisualElement for the border effect
        var rootBoneFieldBorder = new VisualElement();
        rootBoneFieldBorder.AddToClassList("rootBoneFieldBorder");
        rootBoneContainer.Add(rootBoneFieldBorder);

        // create a custom VisualElement that behaves like a disabled ObjectField
        var rootBoneField = new VisualElement();
        rootBoneField.AddToClassList("rootBoneField");
        rootBoneFieldBorder.Add(rootBoneField); // add the rootBoneField to the rootBoneFieldBorder

        // create an Image to display the root bone icon
        var rootBoneIcon = new Image();
        rootBoneIcon.image = EditorGUIUtility.ObjectContent(rootBoneValue, typeof(Transform)).image;
        rootBoneIcon.AddToClassList("rootBoneIcon");
        rootBoneField.Add(rootBoneIcon);

        // create a label to display the root bone name
        var rootBoneNameLabel = new Label(rootBoneValue != null ? rootBoneValue.name : "None");
        rootBoneField.Add(rootBoneNameLabel);

        // handle the MouseDownEvent to select the root bone in the project
        rootBoneField.RegisterCallback<MouseDownEvent>(evt =>
        {
          if (rootBoneValue != null)
          {
            EditorGUIUtility.PingObject(rootBoneValue);
          }
        });

        // add the root bone value to the container
        rootBoneContainer.Add(rootBoneFieldBorder);

        // add the container to the content
        content.Add(rootBoneContainer);
        #endregion

        #region Bones list view
        // create a new label
        var bonesLabel = new Label("Bones");

        // add the bones label to the content
        content.Add(bonesLabel);

        // create a new container for the bone tree view
        var boneTreeViewContainer = new VisualElement();
        boneTreeViewContainer.AddToClassList("container");

        // create a new tree view
        var boneTreeView = new BoneTreeView(_skinnedMeshRenderer);

        // add the tree view to the bone tree view container
        boneTreeViewContainer.Add(boneTreeView);

        // add the container to the content
        content.Add(boneTreeViewContainer);
        #endregion
      }
    }
  }

  // supporting class ---------------------------------------

  // BoneTreeView class
  public class BoneTreeView : ScrollView
  {
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public BoneTreeView(SkinnedMeshRenderer skinnedMeshRenderer)
      : base()
    {
      _skinnedMeshRenderer = skinnedMeshRenderer;
      BuildTreeView(_skinnedMeshRenderer.rootBone, 0);
    }

    private Foldout BuildTreeView(Transform bone, int depth)
    {
      // Set the text of the Foldout to the bone name
      var boneFoldout = new Foldout { text = bone.name };
      boneFoldout.style.marginLeft = depth; // Set the indent level based on the depth
    
      // Calculate the color based on the depth
      int baseColor = 000;
      int colorValue = Math.Max(0, baseColor + depth * 10);
      Color32 color32 = new Color32((byte)colorValue, (byte)colorValue, (byte)colorValue, 255);
      Color color = color32;
      boneFoldout.style.backgroundColor = new StyleColor(color);    

      // Create a VisualElement to hold the Label and the Button
      var boneElement = new VisualElement();
      boneElement.AddToClassList("bone-element"); // Add a class to the VisualElement
    
      // Create a Button with the icon and add it to the VisualElement
      var boneIcon = new Button();
      var iconTexture = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
      boneIcon.style.backgroundImage = new StyleBackground(iconTexture); // Use a built-in icon
      boneIcon.clicked += () => EditorGUIUtility.PingObject(bone.gameObject); // Ping the bone in the hierarchy when the icon is clicked
      boneIcon.AddToClassList("bone-icon"); // Add a class to the Button
      boneElement.Add(boneIcon);
    
      // Add the VisualElement to the Foldout
      boneFoldout.Add(boneElement);
    
      // Recursively add child bones
      foreach (Transform child in bone)
      {
        var childFoldout = BuildTreeView(child, depth + 1);
        boneFoldout.Add(childFoldout);
      }
    
      // Add the bone foldout to the tree view
      this.Add(boneFoldout);
    
      return boneFoldout;
    }
  }
}
//#endif
