// editor ui for CVRFuryDevModeEnabler

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;

namespace uk.novavoidhowl.dev.cvrfury.editor.components
{
  [CustomEditor(typeof(CVRFuryDevModeEnabler))]
  public class CVRFuryDevModeEnablerEditor : Editor
  {
    private CVRFuryDevModeEnabler devModeEnabler;
    private bool devModeEnabled;
    private VisualElement rootVisualElement;

    public override VisualElement CreateInspectorGUI()
    {
      // Create the root VisualElement
      rootVisualElement = new VisualElement();

      // set the name of the root element to allow styling
      rootVisualElement.name = "CVRFuryDevModeEnablerEditor";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryDevModeEnablerInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/CVRFuryDevModeEnablerInspector'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return rootVisualElement;
      }

      // apply stylesheet
      rootVisualElement.styleSheets.Add(stylesheet);

      // add toggle to control DevModeEnabled
      var devModeEnabledToggle = new Toggle("Dev Mode Enabled");
      devModeEnabledToggle.value = devModeEnabler.DevModeEnabled;
      devModeEnabledToggle.RegisterValueChangedCallback(evt =>
      {
        devModeEnabler.DevModeEnabled = evt.newValue;
      });
      rootVisualElement.Add(devModeEnabledToggle);

      // Call UpdateUI after creating the UI
      UpdateUI(devModeEnabler.DevModeEnabled);

      // return the root element
      return rootVisualElement;
    }

    private void OnEnable()
    {
      devModeEnabler = (CVRFuryDevModeEnabler)target;
      devModeEnabled = devModeEnabler.DevModeEnabled;
      devModeEnabler.OnDevModeChanged.AddListener(UpdateUI);
    }

    private void OnDisable()
    {
      devModeEnabler.OnDevModeChanged.RemoveListener(UpdateUI);
    }

    void UpdateUI(bool newValue)
    {
      devModeEnabled = newValue;
      var toggle = rootVisualElement.Q<Toggle>("Dev Mode Enabled");
      if (toggle != null)
      {
        toggle.value = newValue;
      }

      // if dev mode is enabled set the root element background color to green
      if (devModeEnabled)
      {
        // set the background color to #009900
        rootVisualElement.style.backgroundColor = new StyleColor(new Color(0, 0.6f, 0));
      }
      else
      {
        // remove the background color
        rootVisualElement.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));
      }
      Repaint();
    }
  }
}
