//this whole file is editor only
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using uk.novavoidhowl.dev.cvrfury.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  [CustomEditor(typeof(CVRFuryAvatarConfiguration))]
  public class CVRFuryAvatarConfigurationEditor : Editor
  {
    // Reference to the configuration options container
    private VisualElement configOptionsContainer;

    // Dictionary to track switch toggles by property name
    private Dictionary<string, SwitchToggle> switchToggles = new Dictionary<string, SwitchToggle>();

    public override VisualElement CreateInspectorGUI()
    {
      // Create a new VisualElement to be the root of our inspector UI
      VisualElement root = new VisualElement();

      // set the name of the root element to allow styling
      root.name = "CVRFuryAvatarConfigurationEditor";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryAvatarConfigurationEditor"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at '"
            + Constants.PROGRAM_DISPLAY_NAME
            + "/CVRFuryComponents/UnityStyleSheets/CVRFuryAvatarConfigurationEditor"
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
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityUXML/CVRFuryAvatarConfigurationEditor"
      );
      if (visualTree == null)
      {
        CoreLogError("Failed to load UXML");
        return root;
      }

      // Instantiate UXML
      visualTree.CloneTree(root);

      // Check for 'CVR Avatar' component and update AvatarLinkContent
      var targetGameObject = ((CVRFuryAvatarConfiguration)target).gameObject;
      var avatarLinkContent = root.Q<Label>("AvatarLinkContent");
      if (targetGameObject.GetComponent("CVRAvatar") != null)
      {
        avatarLinkContent.text = "Active";
      }
      else
      {
        avatarLinkContent.text = "Inactive";
      }

      // Get reference to the configuration options container
      configOptionsContainer = root.Q("ConfigOptions");

      // If ConfigOptions is a Label, we need to replace it with a VisualElement container
      if (configOptionsContainer is Label)
      {
        var parent = configOptionsContainer.parent;
        int index = parent.IndexOf(configOptionsContainer);

        // Create a new container
        configOptionsContainer = new VisualElement();
        configOptionsContainer.name = "ConfigOptions";
        configOptionsContainer.AddToClassList("config-options-container");

        // Remove the label and add our container at the same position
        parent.RemoveAt(index);
        parent.Insert(index, configOptionsContainer);
      }

      // Create toggle switches for each boolean property
      CreateToggleSwitches();

      // return the root element to be displayed in the inspector
      return root;
    }

    private void CreateToggleSwitches()
    {
      // Make sure we have a valid container
      if (configOptionsContainer == null)
      {
        CoreLogError("Config options container not found in the UXML.");
        return;
      }
#region  Toggle Switches
      // visual element for the feature toggles section
      var featureTogglesSection = new VisualElement();
      featureTogglesSection.AddToClassList("feature-toggles-section");

      // Create the toggle for enableDirectTreeOptimizer
      var enableDirectTreeOptimizerProperty = serializedObject.FindProperty("enableDirectTreeOptimiser");
      var enableDirectTreeOptimizerToggle = new SwitchToggle("Direct Tree Optimiser");
      enableDirectTreeOptimizerToggle.BindProperty(enableDirectTreeOptimizerProperty);
      enableDirectTreeOptimizerToggle.RegisterValueChangedCallback(evt =>
      {
        enableDirectTreeOptimizerProperty.boolValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();
      });
      featureTogglesSection.Add(enableDirectTreeOptimizerToggle);
      switchToggles["enableDirectTreeOptimiser"] = enableDirectTreeOptimizerToggle;

      // Create the toggle for enableBlendShapeOptimizer
      var enableBlendShapeOptimizerProperty = serializedObject.FindProperty("enableBlendShapeOptimiser");
      var enableBlendShapeOptimizerToggle = new SwitchToggle("Blend Shape Optimiser");
      enableBlendShapeOptimizerToggle.BindProperty(enableBlendShapeOptimizerProperty);
      enableBlendShapeOptimizerToggle.RegisterValueChangedCallback(evt =>
      {
        enableBlendShapeOptimizerProperty.boolValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();
      });
      featureTogglesSection.Add(enableBlendShapeOptimizerToggle);
      switchToggles["enableBlendShapeOptimiser"] = enableBlendShapeOptimizerToggle;

      // Create the toggle for enableBlinking
      var enableBlinkingProperty = serializedObject.FindProperty("enableBlinking");
      var enableBlinkingToggle = new SwitchToggle("Blinking");
      enableBlinkingToggle.BindProperty(enableBlinkingProperty);
      enableBlinkingToggle.RegisterValueChangedCallback(evt =>
      {
        enableBlinkingProperty.boolValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();
      });
      featureTogglesSection.Add(enableBlinkingToggle);
      switchToggles["enableBlinking"] = enableBlinkingToggle;

      // Create the toggle for enableMMDCompatibility
      var enableMMDCompatibilityProperty = serializedObject.FindProperty("enableMMDCompatibility");
      var enableMMDCompatibilityToggle = new SwitchToggle("MMD Compatibility");
      enableMMDCompatibilityToggle.BindProperty(enableMMDCompatibilityProperty);
      enableMMDCompatibilityToggle.RegisterValueChangedCallback(evt =>
      {
        enableMMDCompatibilityProperty.boolValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();
      });
      featureTogglesSection.Add(enableMMDCompatibilityToggle);
      switchToggles["enableMMDCompatibility"] = enableMMDCompatibilityToggle;

      // Create the toggle for enableUnlimitedParameters
      var enableUnlimitedParametersProperty = serializedObject.FindProperty("enableUnlimitedParameters");
      var enableUnlimitedParametersToggle = new SwitchToggle("Unlimited Parameters");
      enableUnlimitedParametersToggle.BindProperty(enableUnlimitedParametersProperty);
      enableUnlimitedParametersToggle.RegisterValueChangedCallback(evt =>
      {
        enableUnlimitedParametersProperty.boolValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();
      });
      featureTogglesSection.Add(enableUnlimitedParametersToggle);
      switchToggles["enableUnlimitedParameters"] = enableUnlimitedParametersToggle;

      configOptionsContainer.Add(featureTogglesSection);
#endregion // Toggle Switches

#region colliders section

      // Create a new VisualElement for the colliders section
      var collidersSection = new VisualElement();
      collidersSection.AddToClassList("colliders-section");

      // Check if CVRFuryAvatarColliderInfoUnit component is on the same GameObject as this component
      var avatarColliderInfoUnit = ((CVRFuryAvatarConfiguration)target).GetComponent<CVRFuryAvatarColliderInfoUnit>();

      // Flag to track if section should be disabled
      bool disableColliderSection = (avatarColliderInfoUnit == null);

      // If the component is not found, add disabled header
      if (disableColliderSection)
      {
        // Add disabled indicator header
        var disabledHeader = new Label("Avatar Collider Info component not found.\nSection disabled.");
        disabledHeader.AddToClassList("disabled-section-header");
        collidersSection.Add(disabledHeader);

        // Apply disabled styling
        collidersSection.AddToClassList("disabled-section");
      }

      // Create the toggle for enableMagica1Colliders
      var enableMagica1CollidersProperty = serializedObject.FindProperty("enableMagica1Colliders");
      var enableMagica1CollidersToggle = new SwitchToggle("Magica 1 Colliders");
      enableMagica1CollidersToggle.BindProperty(enableMagica1CollidersProperty);

      // Only register callback if section is enabled
      if (!disableColliderSection)
      {
        enableMagica1CollidersToggle.RegisterValueChangedCallback(evt =>
        {
          enableMagica1CollidersProperty.boolValue = evt.newValue;
          serializedObject.ApplyModifiedProperties();
        });
      }
      else
      {
        // Make the toggle control itself disabled
        enableMagica1CollidersToggle.SetEnabled(false);
      }

      collidersSection.Add(enableMagica1CollidersToggle);
      switchToggles["enableMagica1Colliders"] = enableMagica1CollidersToggle;

      // Create the toggle for enableMagica2Colliders
      var enableMagica2CollidersProperty = serializedObject.FindProperty("enableMagica2Colliders");
      var enableMagica2CollidersToggle = new SwitchToggle("Magica 2 Colliders");
      enableMagica2CollidersToggle.BindProperty(enableMagica2CollidersProperty);

      // Only register callback if section is enabled
      if (!disableColliderSection)
      {
        enableMagica2CollidersToggle.RegisterValueChangedCallback(evt =>
        {
          enableMagica2CollidersProperty.boolValue = evt.newValue;
          serializedObject.ApplyModifiedProperties();
        });
      }
      else
      {
        // Make the toggle control itself disabled
        enableMagica2CollidersToggle.SetEnabled(false);
      }

      collidersSection.Add(enableMagica2CollidersToggle);
      switchToggles["enableMagica2Colliders"] = enableMagica2CollidersToggle;

      // Create the toggle for enableDynamicBoneColliders
      var enableDynamicBoneCollidersProperty = serializedObject.FindProperty("enableDynamicBoneColliders");
      var enableDynamicBoneCollidersToggle = new SwitchToggle("Dynamic Bone Colliders");
      enableDynamicBoneCollidersToggle.BindProperty(enableDynamicBoneCollidersProperty);

      // Only register callback if section is enabled
      if (!disableColliderSection)
      {
        enableDynamicBoneCollidersToggle.RegisterValueChangedCallback(evt =>
        {
          enableDynamicBoneCollidersProperty.boolValue = evt.newValue;
          serializedObject.ApplyModifiedProperties();
        });
      }
      else
      {
        // Make the toggle control itself disabled
        enableDynamicBoneCollidersToggle.SetEnabled(false);
      }

      collidersSection.Add(enableDynamicBoneCollidersToggle);
      switchToggles["enableDynamicBoneColliders"] = enableDynamicBoneCollidersToggle;

      // if the section is disabled, turn off all the toggles
      if (disableColliderSection)
      {
        enableMagica1CollidersToggle.SetValueWithoutNotify(false);
        enableMagica2CollidersToggle.SetValueWithoutNotify(false);
        enableDynamicBoneCollidersToggle.SetValueWithoutNotify(false);
      }

      // Add the colliders section to the config options container
      configOptionsContainer.Add(collidersSection);

#endregion // Colliders
    }

    // Update toggle states when undo/redo is performed
    private void OnEnable()
    {
      Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
      Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnUndoRedo()
    {
      RefreshToggleValues();
    }

    // Public method to refresh toggle values from properties
    public void RefreshToggleValues()
    {
      serializedObject.Update();

      // Update all toggle values from properties
      foreach (var kvp in switchToggles)
      {
        kvp.Value.UpdateFromProperty();
      }
    }

    // Add this to ensure the inspector refreshes when returning to it
    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      base.OnInspectorGUI();
    }

    // Static method to refresh any avatar config editor
    public static void RefreshAllAvatarConfigEditors()
    {
      // Find all open editor windows
      var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
      foreach (var window in windows)
      {
        if (window.titleContent.text == "Inspector")
        {
          // Force the inspector to repaint
          window.Repaint();
        }
      }

      // Force Unity to refresh all inspectors
      UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
  }
}
#endif
