//this whole file is editor only
#if UNITY_EDITOR

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
  [CustomEditor(typeof(CVRFuryDevModeEnabler))]
  public class CVRFuryDevModeEnablerEditor : Editor
  {
    private VisualElement rootElement;
    private Toggle devModeToggle;
    private Button devModeButton;

    private void UpdateButtonState(bool isEnabled)
    {
      if (devModeButton != null)
      {
        if (isEnabled)
        {
          devModeButton.AddToClassList("enabled");
          devModeButton.text = "Disable Dev Mode";
        }
        else
        {
          devModeButton.RemoveFromClassList("enabled");
          devModeButton.text = "Enable Dev Mode";
        }
      }
    }

    public override VisualElement CreateInspectorGUI()
    {
      rootElement = new VisualElement();

      // set the class of the root element to allow styling
      rootElement.AddToClassList("cvr-fury-inspector");

      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/DevMode/UnityUXML/CVRFuryDevModeEnablerInspector"
      );

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/DevMode/UnityStyleSheets/CVRFuryDevModeEnablerInspector"
      );

      if (baseTree == null || stylesheet == null)
      {
        CoreLogError("Failed to load UXML or StyleSheet. Creating fallback UI.");
        CreateFallbackUI();
        return rootElement;
      }

      // Apply the StyleSheet
      rootElement.styleSheets.Add(stylesheet);

      // Instantiate the UXML tree
      var ToolSetup = baseTree.Instantiate();

      // Create a temporary list to hold the children
      List<VisualElement> children = new List<VisualElement>(ToolSetup.Children());

      // Add the children of the instantiated UXML to the root
      foreach (var child in children)
      {
        rootElement.Add(child);
      }

      // Get the button and set up its behavior
      devModeButton = rootElement.Q<Button>("devModeButton");
      if (devModeButton != null)
      {
        var currentState = serializedObject.FindProperty("devModeEnabled").boolValue;
        UpdateButtonState(currentState);

        devModeButton.clicked += () =>
        {
          var component = target as CVRFuryDevModeEnabler;
          if (component != null)
          {
            component.DevModeEnabled = !component.DevModeEnabled;
            UpdateButtonState(component.DevModeEnabled);
            EditorUtility.SetDirty(target);
          }
        };
      }

      return rootElement;
    }

    private void CreateFallbackUI()
    {
      // Simple fallback UI when UXML/USS fails to load
      devModeToggle = new Toggle("Dev Mode") { value = serializedObject.FindProperty("devModeEnabled").boolValue };

      devModeToggle.RegisterValueChangedCallback(evt =>
      {
        var component = target as CVRFuryDevModeEnabler;
        if (component != null)
        {
          component.DevModeEnabled = evt.newValue;
          EditorUtility.SetDirty(target);
        }
      });

      rootElement.Add(devModeToggle);

      devModeButton = new Button();
      var currentState = serializedObject.FindProperty("devModeEnabled").boolValue;
      UpdateButtonState(currentState);

      devModeButton.clicked += () =>
      {
        var component = target as CVRFuryDevModeEnabler;
        if (component != null)
        {
          component.DevModeEnabled = !component.DevModeEnabled;
          UpdateButtonState(component.DevModeEnabled);
          EditorUtility.SetDirty(target);
        }
      };

      rootElement.Add(devModeButton);
    }
  }
}
#endif
