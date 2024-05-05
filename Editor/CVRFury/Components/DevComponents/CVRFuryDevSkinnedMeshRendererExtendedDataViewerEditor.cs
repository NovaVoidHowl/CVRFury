//#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    private readonly string _pathToStyleSheet = Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryDevSkinnedMeshRendererExtendedDataViewer";



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
          "Failed to load StyleSheet at '" + _pathToStyleSheet + "'. Please ensure the file exists at the specified path."
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
          "Failed to load StyleSheet at '" + _pathToStyleSheet + "'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        _baseVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
      }
      // Apply the StyleSheet to the root VisualElement
      _baseVisualElement.styleSheets.Add(stylesheet);

      // add the refresh button back to the base visual element of the component
      _baseVisualElement.Add(_refreshButton);

      // get the skinned mesh renderer component
      _skinnedMeshRenderer = ((CVRFuryDevSkinnedMeshRendererExtendedDataViewer)target).gameObject.GetComponent<SkinnedMeshRenderer>();

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
        foldout.RegisterValueChangedCallback(evt => ((CVRFuryDevSkinnedMeshRendererExtendedDataViewer)target).foldoutState = evt.newValue);
        
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
        
        // create a new label
        var bonesLabel = new Label("Bones");
        
        // add the bones label to the content
        content.Add(bonesLabel);


        var treeViewState = new UnityEditor.IMGUI.Controls.TreeViewState();
        var boneTreeView = new BoneTreeView(treeViewState, _skinnedMeshRenderer);
        boneTreeView.Reload();

        var boneCount = _skinnedMeshRenderer.bones.Length;
        var boneHeight = 16; // adjust this value based on the height of each bone item
        var treeViewHeight = boneCount * boneHeight;

        var treeViewContainer = new IMGUIContainer(() =>
        {
          boneTreeView.OnGUI(new Rect(0, 0, content.contentRect.width, treeViewHeight));
        });

        content.Add(treeViewContainer);

        // Set the height of the parent VisualElement
        content.style.height = treeViewHeight;



      }
      
    }
  }


  // supporting class ---------------------------------------
  public class BoneTreeViewItem : UnityEditor.IMGUI.Controls.TreeViewItem
  {
    public Transform Bone { get; set; }
  }
  
  public class BoneTreeView : UnityEditor.IMGUI.Controls.TreeView
  {
    private SkinnedMeshRenderer _skinnedMeshRenderer;
  
    public BoneTreeView(UnityEditor.IMGUI.Controls.TreeViewState treeViewState, SkinnedMeshRenderer skinnedMeshRenderer) : base(treeViewState)
    {
      _skinnedMeshRenderer = skinnedMeshRenderer;
      Reload(); // Reload the tree to build it and assign the data
      ExpandAll(); // Expand all items in the tree
    }
  
    protected override UnityEditor.IMGUI.Controls.TreeViewItem BuildRoot()
    {
      var root = new BoneTreeViewItem { id = -1, depth = -1, displayName = "Root" };
    
      // Create a dictionary to store the TreeViewItems for each bone
      var boneItems = new Dictionary<Transform, BoneTreeViewItem>();
    
      foreach (var bone in _skinnedMeshRenderer.bones)
      {
        // If the bone has a parent and the parent is also a bone, set the depth to be one more than the parent's depth
        // Otherwise, set the depth to 0
        var depth = bone.parent != null && boneItems.ContainsKey(bone.parent) ? boneItems[bone.parent].depth + 1 : 0;
    
        var boneItem = new BoneTreeViewItem { id = bone.GetInstanceID(), depth = depth, displayName = bone.name, Bone = bone };
        boneItems[bone] = boneItem;
    
        // If the bone has a parent and the parent is also a bone, add the boneItem as a child of the parent's TreeViewItem
        // Otherwise, add the boneItem as a child of the root
        if (bone.parent != null && boneItems.ContainsKey(bone.parent))
        {
          boneItems[bone.parent].AddChild(boneItem);
        }
        else
        {
          root.AddChild(boneItem);
        }
      }
    
      return root;
    }
  
    protected override void SelectionChanged(IList<int> selectedIds)
    {
      base.SelectionChanged(selectedIds);

      if (selectedIds.Count > 0)
      {
        var boneItem = FindItem(selectedIds[0], rootItem) as BoneTreeViewItem;
        if (boneItem != null)
        {
          EditorGUIUtility.PingObject(boneItem.Bone);
        }
      }
    }

  }
}
//#endif