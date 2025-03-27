//this whole file is editor only
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Animations;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  // Custom SwitchToggle class that creates a mobile-style toggle switch
  public class SwitchToggle : BindableElement, INotifyValueChanged<bool>
  {
    private VisualElement track;
    private VisualElement knob;
    private Label label;
    private bool _value;
    private SerializedProperty _boundProperty;

    public bool value
    {
      get { return _value; }
      set
      {
        if (_value != value)
        {
          bool previousValue = _value;
          _value = value;
          UpdateVisualState();
          using (ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(previousValue, _value))
          {
            evt.target = this;
            SendEvent(evt);
          }
        }
      }
    }

    // Event that clients can subscribe to
    public event EventCallback<ChangeEvent<bool>> onValueChanged;

    public SwitchToggle(string labelText = null)
    {
      AddToClassList("switch-toggle");

      // Create container for the label and toggle
      var container = new VisualElement();
      container.AddToClassList("switch-toggle-container");
      hierarchy.Add(container);

      // Add label if provided
      if (!string.IsNullOrEmpty(labelText))
      {
        label = new Label(labelText);
        label.AddToClassList("switch-toggle-label");
        container.Add(label);
      }

      // Create toggle track
      track = new VisualElement();
      track.AddToClassList("switch-toggle-track");
      container.Add(track);

      // Create toggle knob
      knob = new VisualElement();
      knob.AddToClassList("switch-toggle-knob");
      track.Add(knob);

      // Register callbacks
      RegisterCallback<ClickEvent>(OnClick);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
      UpdateVisualState();
    }

    private void OnClick(ClickEvent evt)
    {
      value = !_value;
      evt.StopPropagation();
    }

    private void UpdateVisualState()
    {
      if (_value)
      {
        track.AddToClassList("switch-toggle-track-active");
        knob.AddToClassList("switch-toggle-knob-active");
      }
      else
      {
        track.RemoveFromClassList("switch-toggle-track-active");
        knob.RemoveFromClassList("switch-toggle-knob-active");
      }
    }

    public void RegisterValueChangedCallback(EventCallback<ChangeEvent<bool>> callback)
    {
      onValueChanged += callback;
      RegisterCallback<ChangeEvent<bool>>(callback);
    }

    public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<bool>> callback)
    {
      onValueChanged -= callback;
      UnregisterCallback<ChangeEvent<bool>>(callback);
    }

    // Implementation of INotifyValueChanged<bool>
    public void SetValueWithoutNotify(bool newValue)
    {
      if (_value != newValue)
      {
        _value = newValue;
        UpdateVisualState();
      }
    }

    // Add method to bind to a SerializedProperty
    public void BindProperty(SerializedProperty property)
    {
      _boundProperty = property;
      if (_boundProperty != null)
      {
        SetValueWithoutNotify(_boundProperty.boolValue);
      }
    }

    // Method to update value from bound property
    public void UpdateFromProperty()
    {
      if (_boundProperty != null && _boundProperty.serializedObject != null)
      {
        SetValueWithoutNotify(_boundProperty.boolValue);
      }
    }
  }

  [CustomEditor(typeof(CVRFuryAvatarColliderInfoUnit))]
  public class CVRFuryAvatarColliderInfoUnitEditor : Editor
  {
    private bool showColliders = false;
    private Button showCollidersButton;
    private Button saveConfigButton;
    private Button loadConfigButton;
    private VisualElement collidersConfigSection;
    private Dictionary<string, Foldout> colliderFoldouts = new Dictionary<string, Foldout>();
    private Dictionary<string, string> mirrorPairs = new Dictionary<string, string>();
    private Dictionary<string, SwitchToggle> mirrorToggles = new Dictionary<string, SwitchToggle>();

    // Add a dictionary to store references to properties containers
    private Dictionary<string, VisualElement> propertiesContainers = new Dictionary<string, VisualElement>();

    private Dictionary<string, Label> statusLabels = new Dictionary<string, Label>();

    private SerializedProperty[] colliderProperties;
    private readonly string[] colliderNames = new string[]
    {
      "collider_head",
      "collider_torso",
      "collider_hips",
      "collider_handL",
      "collider_handR",
      "collider_footL",
      "collider_footR",
      "collider_upperArmL",
      "collider_upperArmR",
      "collider_lowerArmL",
      "collider_lowerArmR",
      "collider_upperLegL",
      "collider_upperLegR",
      "collider_lowerLegL",
      "collider_lowerLegR",
      "collider_fingerIndexL",
      "collider_fingerIndexR",
      "collider_fingerMiddleL",
      "collider_fingerMiddleR",
      "collider_fingerRingL",
      "collider_fingerRingR",
      "collider_fingerLittleL",
      "collider_fingerLittleR"
    };

    // Serializable classes for JSON export/import
    [Serializable]
    private class ColliderConfigData
    {
      public string state;
      public bool isMirrored;
      public float radius;
      public float height;
      public Vector3Data position;
      public QuaternionData rotation;
      public string transformPath;
    }

    [Serializable]
    private class Vector3Data
    {
      public float x;
      public float y;
      public float z;

      public Vector3Data(Vector3 v)
      {
        x = v.x;
        y = v.y;
        z = v.z;
      }

      public Vector3 ToVector3()
      {
        return new Vector3(x, y, z);
      }
    }

    [Serializable]
    private class QuaternionData
    {
      public float x;
      public float y;
      public float z;
      public float w;

      public QuaternionData(Quaternion q)
      {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
      }

      public Quaternion ToQuaternion()
      {
        return new Quaternion(x, y, z, w);
      }
    }

    // Modified class using serializable collections instead of Dictionary
    [Serializable]
    private class ColliderPresetData
    {
      public string presetName;
      public string version = "1.0";
      public List<ColliderEntry> colliderEntries = new List<ColliderEntry>();

      [Serializable]
      public class ColliderEntry
      {
        public string name;
        public ColliderConfigData data;

        public ColliderEntry(string name, ColliderConfigData data)
        {
          this.name = name;
          this.data = data;
        }
      }

      // Helper methods to work like a dictionary
      public void AddCollider(string name, ColliderConfigData data)
      {
        colliderEntries.Add(new ColliderEntry(name, data));
      }

      public Dictionary<string, ColliderConfigData> ToDictionary()
      {
        Dictionary<string, ColliderConfigData> result = new Dictionary<string, ColliderConfigData>();
        foreach (ColliderEntry entry in colliderEntries)
        {
          if (!string.IsNullOrEmpty(entry.name) && entry.data != null)
          {
            result[entry.name] = entry.data;
          }
        }
        return result;
      }
    }

    private readonly Dictionary<string, string> colliderDisplayNames = new Dictionary<string, string>()
    {
      { "collider_head", "Head" },
      { "collider_torso", "Torso" },
      { "collider_hips", "Hips" },
      { "collider_handL", "Left Hand" },
      { "collider_handR", "Right Hand" },
      { "collider_footL", "Left Foot" },
      { "collider_footR", "Right Foot" },
      { "collider_upperArmL", "Left Upper Arm" },
      { "collider_upperArmR", "Right Upper Arm" },
      { "collider_lowerArmL", "Left Lower Arm" },
      { "collider_lowerArmR", "Right Lower Arm" },
      { "collider_upperLegL", "Left Upper Leg" },
      { "collider_upperLegR", "Right Upper Leg" },
      { "collider_lowerLegL", "Left Lower Leg" },
      { "collider_lowerLegR", "Right Lower Leg" },
      { "collider_fingerIndexL", "Left Index Finger" },
      { "collider_fingerIndexR", "Right Index Finger" },
      { "collider_fingerMiddleL", "Left Middle Finger" },
      { "collider_fingerMiddleR", "Right Middle Finger" },
      { "collider_fingerRingL", "Left Ring Finger" },
      { "collider_fingerRingR", "Right Ring Finger" },
      { "collider_fingerLittleL", "Left Little Finger" },
      { "collider_fingerLittleR", "Right Little Finger" }
    };

    // Add bone mapping dictionary
    private readonly Dictionary<string, HumanBodyBones> colliderToBoneMapping = new Dictionary<string, HumanBodyBones>()
    {
      { "collider_head", HumanBodyBones.Head },
      { "collider_torso", HumanBodyBones.Chest },
      { "collider_hips", HumanBodyBones.Hips },
      { "collider_handL", HumanBodyBones.LeftHand },
      { "collider_handR", HumanBodyBones.RightHand },
      { "collider_footL", HumanBodyBones.LeftFoot },
      { "collider_footR", HumanBodyBones.RightFoot },
      { "collider_upperArmL", HumanBodyBones.LeftUpperArm },
      { "collider_upperArmR", HumanBodyBones.RightUpperArm },
      { "collider_lowerArmL", HumanBodyBones.LeftLowerArm },
      { "collider_lowerArmR", HumanBodyBones.RightLowerArm },
      { "collider_upperLegL", HumanBodyBones.LeftUpperLeg },
      { "collider_upperLegR", HumanBodyBones.RightUpperLeg },
      { "collider_lowerLegL", HumanBodyBones.LeftLowerLeg },
      { "collider_lowerLegR", HumanBodyBones.RightLowerLeg },
      { "collider_fingerIndexL", HumanBodyBones.LeftIndexDistal },
      { "collider_fingerIndexR", HumanBodyBones.RightIndexDistal },
      { "collider_fingerMiddleL", HumanBodyBones.LeftMiddleDistal },
      { "collider_fingerMiddleR", HumanBodyBones.RightMiddleDistal },
      { "collider_fingerRingL", HumanBodyBones.LeftRingDistal },
      { "collider_fingerRingR", HumanBodyBones.RightRingDistal },
      { "collider_fingerLittleL", HumanBodyBones.LeftLittleDistal },
      { "collider_fingerLittleR", HumanBodyBones.RightLittleDistal }
    };

    private void SetupMirrorPairs()
    {
      // Define which colliders mirror each other
      mirrorPairs.Add("collider_handL", "collider_handR");
      mirrorPairs.Add("collider_handR", "collider_handL");
      mirrorPairs.Add("collider_footL", "collider_footR");
      mirrorPairs.Add("collider_footR", "collider_footL");
      mirrorPairs.Add("collider_upperArmL", "collider_upperArmR");
      mirrorPairs.Add("collider_upperArmR", "collider_upperArmL");
      mirrorPairs.Add("collider_lowerArmL", "collider_lowerArmR");
      mirrorPairs.Add("collider_lowerArmR", "collider_lowerArmL");
      mirrorPairs.Add("collider_upperLegL", "collider_upperLegR");
      mirrorPairs.Add("collider_upperLegR", "collider_upperLegL");
      mirrorPairs.Add("collider_lowerLegL", "collider_lowerLegR");
      mirrorPairs.Add("collider_lowerLegR", "collider_lowerLegL");
      mirrorPairs.Add("collider_fingerIndexL", "collider_fingerIndexR");
      mirrorPairs.Add("collider_fingerIndexR", "collider_fingerIndexL");
      mirrorPairs.Add("collider_fingerMiddleL", "collider_fingerMiddleR");
      mirrorPairs.Add("collider_fingerMiddleR", "collider_fingerMiddleL");
      mirrorPairs.Add("collider_fingerRingL", "collider_fingerRingR");
      mirrorPairs.Add("collider_fingerRingR", "collider_fingerRingL");
      mirrorPairs.Add("collider_fingerLittleL", "collider_fingerLittleR");
      mirrorPairs.Add("collider_fingerLittleR", "collider_fingerLittleL");
    }

    public override VisualElement CreateInspectorGUI()
    {
      // Create a new VisualElement to be the root of our inspector UI
      VisualElement root = new VisualElement();

      // set the name of the root element to allow styling
      root.name = "CVRFuryAvatarColliderInfoUnitEditor";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryAvatarColliderInfoUnitEditor"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at '"
            + Constants.PROGRAM_DISPLAY_NAME
            + "/CVRFuryComponents/UnityStyleSheets/CVRFuryAvatarColliderInfoUnitEditor"
            + "'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        root.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return root;
      }
      // apply stylesheet
      root.styleSheets.Add(stylesheet);

      // Load UXML and stylesheet
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityUXML/CVRFuryAvatarColliderInfoUnitEditor"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return root;
      }

      // Instantiate UXML
      visualTree.CloneTree(root);

      // Check for 'CVR Avatar' component and update AvatarLinkContent
      var targetGameObject = ((CVRFuryAvatarColliderInfoUnit)target).gameObject;
      var avatarLinkContent = root.Q<Label>("AvatarLinkContent");
      if (targetGameObject.GetComponent("CVRAvatar") != null)
      {
        avatarLinkContent.text = "Active";
      }
      else
      {
        avatarLinkContent.text = "Inactive";
      }

      // Get the ShowColliders button and register a click event handler
      showCollidersButton = root.Q<Button>("ShowColliders");
      showCollidersButton.clicked += ToggleShowColliders;

      // Setup the collider config section
      collidersConfigSection = root.Q("CollidersConfig");
      if (collidersConfigSection == null)
      {
        collidersConfigSection = new VisualElement();
        collidersConfigSection.name = "CollidersConfig";
        collidersConfigSection.AddToClassList("colliders-config-section");
        root.Add(collidersConfigSection);
      }

      // Setup mirror pairs
      SetupMirrorPairs();

      // Cache serialized properties for all colliders
      colliderProperties = new SerializedProperty[colliderNames.Length];
      for (int i = 0; i < colliderNames.Length; i++)
      {
        colliderProperties[i] = serializedObject.FindProperty(colliderNames[i]);
      }

      // Create UI for each collider
      foreach (string colliderName in colliderNames)
      {
        CreateColliderUI(collidersConfigSection, colliderName);
      }

      // Get the CollidersTitle element
      VisualElement collidersTitle = root.Q("CollidersConfigTitle");

      // Create container for title and buttons to be on the same line
      VisualElement buttonsContainer = new VisualElement();
      buttonsContainer.style.flexDirection = FlexDirection.Row;
      buttonsContainer.style.justifyContent = Justify.FlexEnd;
      buttonsContainer.style.alignItems = Align.Center;
      buttonsContainer.style.marginBottom = 5;

      // Create Save Config button
      saveConfigButton = new Button(SaveConfigToJson) { text = "Save Config" };
      saveConfigButton.AddToClassList("config-button");
      saveConfigButton.style.marginRight = 5;
      buttonsContainer.Add(saveConfigButton);

      // Create Load Config button
      loadConfigButton = new Button(LoadConfigFromJson) { text = "Load Config" };
      loadConfigButton.AddToClassList("config-button");
      buttonsContainer.Add(loadConfigButton);

      // Add the title container to the appropriate place in the hierarchy
      collidersTitle.Add(buttonsContainer);

      // Return the finished inspector UI
      return root;
    }

    // Save the current collider configuration to a JSON file (updated method)
    private void SaveConfigToJson()
    {
      string avatarName = ((CVRFuryAvatarColliderInfoUnit)target).gameObject.name;
      string defaultPath = Path.Combine(Application.dataPath, $"{avatarName}_collider_preset.json");
      string path = EditorUtility.SaveFilePanel(
        "Save Collider Preset",
        Path.GetDirectoryName(defaultPath),
        Path.GetFileName(defaultPath),
        "json"
      );

      if (string.IsNullOrEmpty(path))
      {
        // User canceled the save dialog
        return;
      }

      try
      {
        CVRFuryAvatarColliderInfoUnit targetComponent = (CVRFuryAvatarColliderInfoUnit)target;
        GameObject avatarRoot = targetComponent.gameObject;

        ColliderPresetData presetData = new ColliderPresetData { presetName = avatarName + " Colliders" };

        // Save each collider configuration
        foreach (string colliderName in colliderNames)
        {
          // Use reflection to get the collider property
          var fieldInfo = typeof(CVRFuryAvatarColliderInfoUnit).GetField(colliderName);
          if (fieldInfo == null)
            continue;

          var colliderConfig = (CVRFuryAvatarColliderInfoUnit.ColliderConfig)fieldInfo.GetValue(targetComponent);

          // Skip completely empty/unset colliders
          if (
            colliderConfig.state == CVRFuryAvatarColliderInfoUnit.ColliderConfig.State.Disabled
            && colliderConfig.transform == null
            && colliderConfig.radius <= 0
            && colliderConfig.height <= 0
          )
          {
            continue;
          }

          ColliderConfigData configData = new ColliderConfigData
          {
            state = colliderConfig.state.ToString(),
            isMirrored = colliderConfig.isMirrored,
            radius = colliderConfig.radius,
            height = colliderConfig.height,
            position = new Vector3Data(colliderConfig.position),
            rotation = new QuaternionData(colliderConfig.rotation)
          };

          // Store transform path relative to avatar root if it exists
          if (colliderConfig.transform != null)
          {
            configData.transformPath = GetRelativePath(avatarRoot.transform, colliderConfig.transform);
          }

          presetData.AddCollider(colliderName, configData);
        }

        // Serialize to JSON and write to file
        string json = JsonUtility.ToJson(presetData, true);
        File.WriteAllText(path, json);

        CoreLog($"Collider configuration saved to {path}");
      }
      catch (Exception e)
      {
        CoreLogError($"Error saving collider configuration: {e.Message}");
        Debug.LogException(e); // Log full exception for debugging
      }
    }

    // Get the transform path relative to an ancestor
    private string GetRelativePath(Transform root, Transform target)
    {
      if (target == root)
        return "";

      string path = target.name;
      Transform parent = target.parent;

      while (parent != null && parent != root)
      {
        path = parent.name + "/" + path;
        parent = parent.parent;
      }

      return path;
    }

    // Load collider configuration from a JSON file (updated method)
    private void LoadConfigFromJson()
    {
      string path = EditorUtility.OpenFilePanel("Load Collider Preset", Application.dataPath, "json");

      if (string.IsNullOrEmpty(path))
      {
        // User canceled the load dialog
        return;
      }

      try
      {
        string json = File.ReadAllText(path);

        // Parse the JSON directly to our serialization-friendly structure
        ColliderPresetData presetData = JsonUtility.FromJson<ColliderPresetData>(json);
        if (presetData == null)
        {
          CoreLogError("Failed to parse JSON file. The file may be corrupted or have an invalid format.");
          return;
        }

        // Convert from our serialization structure to a dictionary for easier access
        Dictionary<string, ColliderConfigData> colliderDataDict = presetData.ToDictionary();

        CVRFuryAvatarColliderInfoUnit targetComponent = (CVRFuryAvatarColliderInfoUnit)target;
        GameObject avatarRoot = targetComponent.gameObject;

        Undo.RecordObject(targetComponent, "Load Collider Configuration");

        // Process each collider in the preset
        foreach (var entry in colliderDataDict)
        {
          string colliderName = entry.Key;
          ColliderConfigData configData = entry.Value;

          // Use reflection to get and set the collider property
          var fieldInfo = typeof(CVRFuryAvatarColliderInfoUnit).GetField(colliderName);
          if (fieldInfo == null)
            continue;

          var colliderConfig = (CVRFuryAvatarColliderInfoUnit.ColliderConfig)fieldInfo.GetValue(targetComponent);

          // Set enum state by parsing the string
          if (Enum.TryParse(configData.state, out CVRFuryAvatarColliderInfoUnit.ColliderConfig.State state))
          {
            colliderConfig.state = state;
          }

          colliderConfig.isMirrored = configData.isMirrored;
          colliderConfig.radius = configData.radius;
          colliderConfig.height = configData.height;

          if (configData.position != null)
            colliderConfig.position = configData.position.ToVector3();

          if (configData.rotation != null)
            colliderConfig.rotation = configData.rotation.ToQuaternion();

          // Resolve transform reference if path exists
          if (!string.IsNullOrEmpty(configData.transformPath))
          {
            Transform foundTransform = avatarRoot.transform.Find(configData.transformPath);
            colliderConfig.transform = foundTransform;
          }

          // Set the updated config back to the component
          fieldInfo.SetValue(targetComponent, colliderConfig);
        }

        // Update the serialized object and UI
        serializedObject.Update();

        // Update all UI elements
        foreach (string colliderName in colliderNames)
        {
          UpdateColliderUI(colliderName);

          // Update mirror toggle values
          if (mirrorToggles.ContainsKey(colliderName))
          {
            mirrorToggles[colliderName].UpdateFromProperty();
          }
        }

        EditorUtility.SetDirty(targetComponent);
        CoreLog($"Collider configuration loaded from {path}");
        SceneView.RepaintAll();
      }
      catch (Exception e)
      {
        CoreLogError($"Error loading collider configuration: {e.Message}");
        Debug.LogException(e); // Log full exception for debugging
      }
    }

    private void CreateColliderUI(VisualElement parent, string colliderName)
    {
      SerializedProperty colliderProp = serializedObject.FindProperty(colliderName);
      if (colliderProp == null)
        return;

      // Add this check at the start of CreateColliderUI
      if (mirrorPairs.ContainsKey(colliderName))
      {
        string pairedCollider = mirrorPairs[colliderName];
        SerializedProperty thisProp = serializedObject.FindProperty(colliderName);
        SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);

        SerializedProperty thisIsMirrored = thisProp.FindPropertyRelative("isMirrored");
        SerializedProperty pairedIsMirrored = pairedProp.FindPropertyRelative("isMirrored");

        // If both are mirrored, and this is the left side, unset this one's mirror
        if (thisIsMirrored.boolValue && pairedIsMirrored.boolValue)
        {
          // Check if this is the left side (ends with 'L')
          if (colliderName.EndsWith("L"))
          {
            thisIsMirrored.boolValue = false;
            serializedObject.ApplyModifiedProperties();
            CoreLogWarning(
              $"Automatically unmarked {colliderDisplayNames[colliderName]} as mirrored to prevent mirror loop."
            );
          }
        }
      }

      string displayName = colliderDisplayNames.ContainsKey(colliderName)
        ? colliderDisplayNames[colliderName]
        : ObjectNames.NicifyVariableName(colliderName);

      // Create foldout for this collider - setting value to false makes it start collapsed
      Foldout foldout = new Foldout();
      foldout.text = displayName;
      foldout.value = false; // Start collapsed

      // Create a container for the foldout header
      VisualElement headerContainer = new VisualElement();
      headerContainer.style.flexDirection = FlexDirection.Row;
      headerContainer.style.alignItems = Align.Center;

      // Move the foldout's content to the new container
      headerContainer.Add(foldout);

      // Create status label
      Label statusLabel = new Label();
      statusLabel.AddToClassList("status-label");
      statusLabel.style.marginLeft = 10;
      statusLabel.style.color = new StyleColor(new Color(1, 0.7f, 0));
      headerContainer.Add(statusLabel);
      statusLabels[colliderName] = statusLabel;

      // Add the header container to parent
      headerContainer.AddToClassList("collider-foldout");
      parent.Add(headerContainer);
      colliderFoldouts[colliderName] = foldout;

      // Main container for collider properties
      VisualElement container = new VisualElement();
      container.AddToClassList("collider-container");
      foldout.Add(container);

      // Create mirror toggle if this collider has a mirror pair
      if (mirrorPairs.ContainsKey(colliderName))
      {
        SerializedProperty isMirroredProp = colliderProp.FindPropertyRelative("isMirrored");

        // Replace standard toggle with our custom SwitchToggle
        SwitchToggle mirrorToggle = new SwitchToggle("Mirror from paired collider");
        mirrorToggle.value = isMirroredProp.boolValue;
        mirrorToggle.RegisterValueChangedCallback(evt =>
        {
          // Check if trying to enable mirroring
          if (evt.newValue)
          {
            // Get the paired collider's property
            string pairedCollider = mirrorPairs[colliderName];
            SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);
            SerializedProperty pairedIsMirrored = pairedProp.FindPropertyRelative("isMirrored");

            // If the paired collider is already mirrored, prevent this change
            if (pairedIsMirrored.boolValue)
            {
              CoreLogWarning(
                $"Cannot mirror both colliders in a pair. Please unmirror {colliderDisplayNames[pairedCollider]} first."
              );
              evt.StopPropagation();
              mirrorToggle.SetValueWithoutNotify(false);
              return;
            }
          }

          isMirroredProp.boolValue = evt.newValue;
          UpdateMirrorState(colliderName, evt.newValue);
          serializedObject.ApplyModifiedProperties();
          SceneView.RepaintAll();
        });

        // Bind the property directly
        mirrorToggle.BindProperty(isMirroredProp);

        // Store the toggle for later updates
        mirrorToggles[colliderName] = mirrorToggle;

        container.Add(mirrorToggle);
      }
      else
      {
        // For non-mirrorable colliders (head, torso, hips), ensure isMirrored is false
        if (colliderName == "collider_head" || colliderName == "collider_torso" || colliderName == "collider_hips")
        {
          SerializedProperty isMirroredProp = colliderProp.FindPropertyRelative("isMirrored");
          if (isMirroredProp.boolValue)
          {
            isMirroredProp.boolValue = false;
            serializedObject.ApplyModifiedProperties();
          }
        }
      }

      // Create a separate container for transform-related controls
      VisualElement transformContainer = new VisualElement();
      transformContainer.AddToClassList("transform-container");
      container.Add(transformContainer);

      // Create transform field and auto-assign button if applicable
      Button autoAssignButton = null;
      if (colliderToBoneMapping.ContainsKey(colliderName))
      {
        autoAssignButton = new Button(() => AutoAssignTransform(colliderName)) { text = "Auto-Assign Transform" };
        autoAssignButton.AddToClassList("auto-assign-button");
      }

      SerializedProperty transformProp = colliderProp.FindPropertyRelative("transform");
      ObjectField transformField = new ObjectField("Transform");
      transformField.objectType = typeof(Transform);
      transformField.bindingPath = transformProp.propertyPath;
      transformField.RegisterValueChangedCallback(evt =>
      {
        serializedObject.ApplyModifiedProperties();
        if (autoAssignButton != null)
        {
          autoAssignButton.SetEnabled(evt.newValue == null);
        }
        UpdateStatusLabel(colliderName);
        SceneView.RepaintAll();
      });
      transformContainer.Add(transformField);

      // Add the auto-assign button after the transform field if it was created
      if (autoAssignButton != null)
      {
        // Disable button if transform is already assigned
        autoAssignButton.SetEnabled(transformProp.objectReferenceValue == null);
        transformContainer.Add(autoAssignButton);
      }

      // Properties container that will be enabled/disabled based on mirroring
      VisualElement propertiesContainer = new VisualElement();
      propertiesContainer.AddToClassList("properties-container");
      container.Add(propertiesContainer);

      // Store reference to properties container for this collider
      propertiesContainers[colliderName] = propertiesContainer;

      // Create state dropdown (Automatic, Custom, Disabled) and add it directly to properties container
      SerializedProperty stateProp = colliderProp.FindPropertyRelative("state");
      EnumField stateField = new EnumField("State");
      stateField.bindingPath = stateProp.propertyPath;
      stateField.RegisterValueChangedCallback(evt =>
      {
        serializedObject.ApplyModifiedProperties();
        UpdateColliderUI(colliderName);
        SceneView.RepaintAll();
      });
      propertiesContainer.Add(stateField);

      // Create radius field
      SerializedProperty radiusProp = colliderProp.FindPropertyRelative("radius");
      FloatField radiusField = new FloatField("Radius");
      radiusField.bindingPath = radiusProp.propertyPath;
      radiusField.RegisterValueChangedCallback(evt =>
      {
        serializedObject.ApplyModifiedProperties();

        // If this is the source of a mirror, update the mirrored collider
        if (mirrorPairs.ContainsKey(colliderName))
        {
          string pairedCollider = mirrorPairs[colliderName];
          SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);
          if (pairedProp.FindPropertyRelative("isMirrored").boolValue)
          {
            SyncMirroredProperties(colliderName, pairedCollider);
          }
        }

        UpdateStatusLabel(colliderName);
        SceneView.RepaintAll();
      });
      propertiesContainer.Add(radiusField);

      // Create height field
      SerializedProperty heightProp = colliderProp.FindPropertyRelative("height");
      FloatField heightField = new FloatField("Height");
      heightField.bindingPath = heightProp.propertyPath;
      heightField.RegisterValueChangedCallback(evt =>
      {
        serializedObject.ApplyModifiedProperties();

        // If this is the source of a mirror, update the mirrored collider
        if (mirrorPairs.ContainsKey(colliderName))
        {
          string pairedCollider = mirrorPairs[colliderName];
          SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);
          if (pairedProp.FindPropertyRelative("isMirrored").boolValue)
          {
            SyncMirroredProperties(colliderName, pairedCollider);
          }
        }

        UpdateStatusLabel(colliderName);
        SceneView.RepaintAll();
      });
      propertiesContainer.Add(heightField);

      // Container for custom properties (position/rotation)
      VisualElement customPropsContainer = new VisualElement();
      customPropsContainer.AddToClassList("custom-properties-container");
      propertiesContainer.Add(customPropsContainer);

      // Create position field
      SerializedProperty positionProp = colliderProp.FindPropertyRelative("position");
      Vector3Field positionField = new Vector3Field("Position");
      positionField.bindingPath = positionProp.propertyPath;
      positionField.RegisterValueChangedCallback(evt =>
      {
        serializedObject.ApplyModifiedProperties();

        // If this is the source of a mirror, update the mirrored collider
        if (mirrorPairs.ContainsKey(colliderName))
        {
          string pairedCollider = mirrorPairs[colliderName];
          SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);
          if (pairedProp.FindPropertyRelative("isMirrored").boolValue)
          {
            SyncMirroredProperties(colliderName, pairedCollider);
          }
        }

        SceneView.RepaintAll();
      });
      customPropsContainer.Add(positionField);

      // Create rotation field using Vector3Field with a label
      SerializedProperty rotationProp = colliderProp.FindPropertyRelative("rotation");
      Vector3Field rotationField = new Vector3Field("Rotation (Euler Angles)");
      rotationField.value = rotationProp.quaternionValue.eulerAngles;
      rotationField.RegisterValueChangedCallback(evt =>
      {
        // Update the quaternion value when euler angles change
        rotationProp.quaternionValue = Quaternion.Euler(evt.newValue);
        serializedObject.ApplyModifiedProperties();

        // If this is the source of a mirror, update the mirrored collider
        if (mirrorPairs.ContainsKey(colliderName))
        {
          string pairedCollider = mirrorPairs[colliderName];
          SerializedProperty pairedProp = serializedObject.FindProperty(pairedCollider);
          if (pairedProp.FindPropertyRelative("isMirrored").boolValue)
          {
            SyncMirroredProperties(colliderName, pairedCollider);
          }
        }

        SceneView.RepaintAll();
      });
      customPropsContainer.Add(rotationField);

      // Initial update of the UI based on current state
      UpdateColliderUI(colliderName);
      // Update status label initially
      UpdateStatusLabel(colliderName);
    }

    private void AutoAssignTransform(string colliderName)
    {
      var targetGameObject = ((CVRFuryAvatarColliderInfoUnit)target).gameObject;
      var animator = targetGameObject.GetComponent<Animator>();

      if (animator == null || !animator.isHuman)
      {
        CoreLogWarning("No humanoid animator found on the game object. Cannot auto-assign transform.");
        return;
      }

      if (colliderToBoneMapping.TryGetValue(colliderName, out HumanBodyBones bone))
      {
        Transform boneTransform = animator.GetBoneTransform(bone);
        if (boneTransform != null)
        {
          SerializedProperty colliderProp = serializedObject.FindProperty(colliderName);
          SerializedProperty transformProp = colliderProp.FindPropertyRelative("transform");
          transformProp.objectReferenceValue = boneTransform;
          serializedObject.ApplyModifiedProperties();
          CoreLogDebug($"Successfully assigned transform for {colliderName}");
        }
        else
        {
          CoreLogWarning($"Could not find bone transform for {bone}");
        }
      }
    }

    private void UpdateColliderUI(string colliderName)
    {
      if (!colliderFoldouts.ContainsKey(colliderName))
        return;

      SerializedProperty colliderProp = serializedObject.FindProperty(colliderName);
      bool isMirrored = colliderProp.FindPropertyRelative("isMirrored").boolValue;

      // Get properties container from dictionary instead of using Q method
      if (propertiesContainers.TryGetValue(colliderName, out VisualElement propertiesContainer))
      {
        // Disable all children of properties container when mirrored
        propertiesContainer.SetEnabled(!isMirrored);
      }
      UpdateStatusLabel(colliderName);
    }

    private void SyncMirroredProperties(string sourceColliderName, string targetColliderName)
    {
      SerializedProperty sourceProp = serializedObject.FindProperty(sourceColliderName);
      SerializedProperty targetProp = serializedObject.FindProperty(targetColliderName);

      // Copy radius and height
      targetProp.FindPropertyRelative("radius").floatValue = sourceProp.FindPropertyRelative("radius").floatValue;
      targetProp.FindPropertyRelative("height").floatValue = sourceProp.FindPropertyRelative("height").floatValue;

      // Copy position with mirrored X coordinate
      SerializedProperty sourcePos = sourceProp.FindPropertyRelative("position");
      SerializedProperty targetPos = targetProp.FindPropertyRelative("position");
      targetPos.vector3Value = new Vector3(
        -sourcePos.vector3Value.x,
        sourcePos.vector3Value.y,
        sourcePos.vector3Value.z
      );

      // Copy rotation with mirrored X and Z rotations
      SerializedProperty sourceRot = sourceProp.FindPropertyRelative("rotation");
      SerializedProperty targetRot = targetProp.FindPropertyRelative("rotation");
      Vector3 sourceEuler = sourceRot.quaternionValue.eulerAngles;
      targetRot.quaternionValue = Quaternion.Euler(-sourceEuler.x, sourceEuler.y, -sourceEuler.z);

      serializedObject.ApplyModifiedProperties();
    }

    private void UpdateMirrorState(string colliderName, bool isMirrored)
    {
      if (mirrorPairs.ContainsKey(colliderName))
      {
        string pairedCollider = mirrorPairs[colliderName];

        // If enabling mirror, sync properties from source to target
        if (isMirrored)
        {
          SyncMirroredProperties(pairedCollider, colliderName);
        }

        // Update the UI state
        UpdateColliderUI(colliderName);
        UpdateColliderUI(pairedCollider);
      }
    }

    private void UpdateStatusLabel(string colliderName)
    {
      if (!statusLabels.ContainsKey(colliderName))
        return;

      SerializedProperty colliderProp = serializedObject.FindProperty(colliderName);
      if (colliderProp == null)
        return;

      SerializedProperty transformProp = colliderProp.FindPropertyRelative("transform");
      SerializedProperty radiusProp = colliderProp.FindPropertyRelative("radius");
      SerializedProperty heightProp = colliderProp.FindPropertyRelative("height");
      SerializedProperty stateProp = colliderProp.FindPropertyRelative("state");

      // Check if the collider is disabled
      if (
        (CVRFuryAvatarColliderInfoUnit.ColliderConfig.State)stateProp.enumValueIndex
        == CVRFuryAvatarColliderInfoUnit.ColliderConfig.State.Disabled
      )
      {
        statusLabels[colliderName].text = "(Disabled)";
        statusLabels[colliderName].style.color = new StyleColor(Color.white);
        return;
      }

      // check if the collider is mirrored
      if (colliderProp.FindPropertyRelative("isMirrored").boolValue)
      {
        statusLabels[colliderName].text = "MIRRORED";
        statusLabels[colliderName].style.color = new StyleColor(new Color(0.3f, 0.8f, 1f)); // Light blue color
        return;
      }

      List<string> warnings = new List<string>();

      if (transformProp.objectReferenceValue == null)
      {
        warnings.Add("No Transform");
      }
      if (radiusProp.floatValue <= 0)
      {
        // note no need to look for heightProp.floatValue <= 0 as zero is a valid value for height
        warnings.Add("Zero Size");
      }

      statusLabels[colliderName].text = warnings.Count > 0 ? $"({string.Join(", ", warnings)})" : "";
      statusLabels[colliderName].style.color = new StyleColor(new Color(1, 0.7f, 0)); // Warning orange color
    }

    private void ToggleShowColliders()
    {
      // Toggle the showColliders state
      showColliders = !showColliders;

      // Update the button appearance based on the new state
      if (showColliders)
      {
        showCollidersButton.AddToClassList("enabled");
        showCollidersButton.text = "Hide Colliders";
      }
      else
      {
        showCollidersButton.RemoveFromClassList("enabled");
        showCollidersButton.text = "Show Colliders";
      }

      // Force the scene view to update to show/hide colliders
      SceneView.RepaintAll();
    }

    // Rest of the existing OnSceneGUI and DrawCollider methods remain the same
    private void OnSceneGUI()
    {
      // Only draw the colliders if requested
      if (!showColliders)
        return;

      CVRFuryAvatarColliderInfoUnit collidersInfo = (CVRFuryAvatarColliderInfoUnit)target;

      // Draw all colliders defined in the component
      DrawCollider(collidersInfo.collider_fingerLittleR, "Finger Little Right");
      DrawCollider(collidersInfo.collider_fingerRingR, "Finger Ring Right");
      DrawCollider(collidersInfo.collider_fingerMiddleR, "Finger Middle Right");
      DrawCollider(collidersInfo.collider_fingerIndexR, "Finger Index Right");
      DrawCollider(collidersInfo.collider_fingerLittleL, "Finger Little Left");
      DrawCollider(collidersInfo.collider_fingerRingL, "Finger Ring Left");
      DrawCollider(collidersInfo.collider_fingerMiddleL, "Finger Middle Left");
      DrawCollider(collidersInfo.collider_fingerIndexL, "Finger Index Left");
      DrawCollider(collidersInfo.collider_handL, "Hand Left");
      DrawCollider(collidersInfo.collider_handR, "Hand Right");
      DrawCollider(collidersInfo.collider_footL, "Foot Left");
      DrawCollider(collidersInfo.collider_footR, "Foot Right");
      DrawCollider(collidersInfo.collider_torso, "Torso");
      DrawCollider(collidersInfo.collider_head, "Head");
      DrawCollider(collidersInfo.collider_hips, "Hips");
      DrawCollider(collidersInfo.collider_upperLegL, "Upper Leg Left");
      DrawCollider(collidersInfo.collider_upperLegR, "Upper Leg Right");
      DrawCollider(collidersInfo.collider_lowerLegL, "Lower Leg Left");
      DrawCollider(collidersInfo.collider_lowerLegR, "Lower Leg Right");
      DrawCollider(collidersInfo.collider_upperArmL, "Upper Arm Left");
      DrawCollider(collidersInfo.collider_upperArmR, "Upper Arm Right");
      DrawCollider(collidersInfo.collider_lowerArmL, "Lower Arm Left");
      DrawCollider(collidersInfo.collider_lowerArmR, "Lower Arm Right");
    }

    private void DrawCollider(CVRFuryAvatarColliderInfoUnit.ColliderConfig collider, string label)
    {
      // Skip if disabled or transform is null
      if (collider.state == CVRFuryAvatarColliderInfoUnit.ColliderConfig.State.Disabled || collider.transform == null)
      {
        return;
      }
      // Determine base position and rotation from transform
      Vector3 position = collider.transform ? collider.transform.position : Vector3.zero;
      Quaternion rotation = collider.transform ? collider.transform.rotation : Quaternion.identity;

      // Apply offsets regardless of state (except when disabled)
      if (collider.state != CVRFuryAvatarColliderInfoUnit.ColliderConfig.State.Disabled)
      {
        // Apply position offset in local space
        position += rotation * collider.position;
        // Combine rotations
        rotation *= collider.rotation;
      }

      // Set color based on mirrored state
      Handles.color = collider.isMirrored ? Color.cyan : Color.green;

      Matrix4x4 originalMatrix = Handles.matrix;

      // Set matrix for the capsule with proper position and rotation
      Handles.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

      // Draw capsule
      DrawWireCapsule(Vector3.zero, Quaternion.identity, collider.radius, collider.height);

      // Restore original matrix
      Handles.matrix = originalMatrix;

      // Draw label with (mirrored) suffix if applicable
      string displayLabel = collider.isMirrored ? $"{label} (mirrored)" : label;
      Handles.Label(position, displayLabel);
    }

    private void DrawWireCapsule(Vector3 position, Quaternion rotation, float radius, float height)
    {
      if (radius <= 0)
        return;

      // Ensure height is never negative
      height = Mathf.Max(0, height);

      // Calculate the actual height of the cylinder part (can be 0)
      float cylinderHeight = Mathf.Max(0, height - 2 * radius);

      // When height is 0 or less than 2*radius, just draw a sphere
      if (cylinderHeight <= 0)
      {
        // Draw complete sphere
        Handles.DrawWireArc(Vector3.zero, Vector3.forward, Vector3.right, 180, radius);
        Handles.DrawWireArc(Vector3.zero, Vector3.forward, Vector3.left, 180, radius);
        Handles.DrawWireArc(Vector3.zero, Vector3.right, Vector3.back, 180, radius);
        Handles.DrawWireArc(Vector3.zero, Vector3.right, Vector3.forward, 180, radius);
        Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
        return;
      }

      // If we have actual height, draw full capsule
      Vector3 upperSphere = new Vector3(0, cylinderHeight / 2, 0);
      Vector3 lowerSphere = new Vector3(0, -cylinderHeight / 2, 0);

      // Draw hemisphere caps
      Handles.DrawWireArc(upperSphere, Vector3.forward, Vector3.right, 180, radius);
      Handles.DrawWireArc(upperSphere, Vector3.right, Vector3.back, 180, radius);
      Handles.DrawWireDisc(upperSphere, Vector3.up, radius);

      Handles.DrawWireArc(lowerSphere, Vector3.forward, Vector3.left, 180, radius);
      Handles.DrawWireArc(lowerSphere, Vector3.right, Vector3.forward, 180, radius);
      Handles.DrawWireDisc(lowerSphere, Vector3.up, radius);

      // Draw connecting lines
      Handles.DrawLine(upperSphere + Vector3.right * radius, lowerSphere + Vector3.right * radius);
      Handles.DrawLine(upperSphere + Vector3.left * radius, lowerSphere + Vector3.left * radius);
      Handles.DrawLine(upperSphere + Vector3.forward * radius, lowerSphere + Vector3.forward * radius);
      Handles.DrawLine(upperSphere + Vector3.back * radius, lowerSphere + Vector3.back * radius);
    }

    // Fix for SerializedObject version mismatch error
    private void OnEnable()
    {
      // Ensure we're using an updated serializedObject
      Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
      // Clean up event subscription
      Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnUndoRedo()
    {
      // Refresh serializedObject when undo/redo occurs
      serializedObject.Update();

      // Refresh UI elements
      if (colliderFoldouts != null)
      {
        foreach (var colliderName in colliderNames)
        {
          UpdateColliderUI(colliderName);

          // Update mirror toggle values from properties
          if (mirrorToggles.ContainsKey(colliderName))
          {
            mirrorToggles[colliderName].UpdateFromProperty();
          }
        }
      }

      // Force repaint
      SceneView.RepaintAll();
    }
  }
}
#endif
