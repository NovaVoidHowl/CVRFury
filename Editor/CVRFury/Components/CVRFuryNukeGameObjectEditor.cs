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
  [CustomEditor(typeof(CVRFuryNukeGameObject))]
  public class CVRFuryNukeGameObjectEditor : Editor
  {
    private VisualElement rootVisualElement;

    public override VisualElement CreateInspectorGUI()
    {
      // Create the root VisualElement
      rootVisualElement = new VisualElement();

      // set the name of the root element to allow styling
      rootVisualElement.name = "CVRFuryNukeGameObjectEditor";
     
      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryNukeGameObjectInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/CVRFuryNukeGameObjectInspector'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return rootVisualElement;
      }

      // apply stylesheet
      rootVisualElement.styleSheets.Add(stylesheet);
     
     
     
     
      // get the DSUNumber
      var DSUNumber = serializedObject.FindProperty("DSUNumber");

      // get nukeEnabled
      var nukeEnabled = serializedObject.FindProperty("nukeEnabled");

      // if nukeEnabled is true, add the class nukeEnabled to the root element
      if (nukeEnabled.boolValue)
      {
        rootVisualElement.AddToClassList("nukeEnabled");
        // add a label to say that the nuke is enabled
        var nukeEnabledLabel = new Label("Nuke Enabled");
        // add class to the label
        nukeEnabledLabel.AddToClassList("nukeEnabledLabel");
        rootVisualElement.Add(nukeEnabledLabel);
      }

      // add label to say that this component should not be added manually
      var warningLabel = new Label("This component should not be added manually."
                                   +"\n It is used to tag gameObjects for purge during the cleanup phase of CVRFury.");
      rootVisualElement.Add(warningLabel);
      // add a label to say what the related DSU is
      var DSUNumberLabel = new Label("Related DSU Number: " + DSUNumber.intValue);
      rootVisualElement.Add(DSUNumberLabel);
      // return the root element
      return rootVisualElement;
    }
  }
}
#endif
