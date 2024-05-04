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
  [CustomEditor(typeof(CVRFuryGameObjectInfoTag))]
  public class CVRFuryGameObjectInfoTagEditor : Editor
  {
    private VisualElement rootVisualElement;

    public override VisualElement CreateInspectorGUI()
    {
      // Create the root VisualElement
      rootVisualElement = new VisualElement();

      // set the name of the root element to allow styling
      rootVisualElement.name = "CVRFuryGameObjectInfoTagEditor";

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryGameObjectInfoTagInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/CVRFuryGameObjectInfoTagInspector'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return rootVisualElement;
      }

      // apply stylesheet
      rootVisualElement.styleSheets.Add(stylesheet);

      // add element to hold an icon and the info box
      var iconAndInfoBox = new VisualElement();
      // add class to the element to allow styling
      iconAndInfoBox.AddToClassList("icon-and-info-box");
      // add the element to the root element
      rootVisualElement.Add(iconAndInfoBox);

      // add the info icon to the messageContainer
      var icon = new VisualElement();
      // add the icon class to the icon
      icon.AddToClassList("icon");
      // set the width and height of the icon to 40px
      icon.style.width = new Length(30, LengthUnit.Pixel);
      icon.style.height = new Length(30, LengthUnit.Pixel);
      // load the VectorImage from the Resources folder
      VectorImage infoIcon = Resources.Load<VectorImage>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/IconsAndImages/info"
      );
      // create a StyleBackground from the VectorImage
      StyleBackground infoBackground = new StyleBackground(infoIcon);
      // set the StyleBackground as the background image for the 'icon' UI element
      icon.style.backgroundImage = infoBackground;
      // add margin to the icon
      icon.style.marginBottom = new StyleLength(20f);

      // add the icon to the iconAndInfoBox
      iconAndInfoBox.Add(icon);

      // add info box to say that this component is used to store info about GameObjects that have been added to the base avatar via CVRFury
      var infoBox = new Label(
        "This component is used to store info about GameObjects that have been added to the base avatar via CVRFury".TrimEnd(
          '\n'
        )
      );
      // add the info box to the root element
      iconAndInfoBox.Add(infoBox);
      // add class to the info box to allow styling
      infoBox.AddToClassList("info-box");

      // add info box to display sourcePrefabName and sourceDSUNumber
      var sourcePrefabNameField = new TextField("Source Prefab Name");
      sourcePrefabNameField.value = (target as CVRFuryGameObjectInfoTag).sourcePrefabName;
      // make the field read only
      sourcePrefabNameField.SetEnabled(false);
      rootVisualElement.Add(sourcePrefabNameField);

      var sourceDSUNumberField = new IntegerField("Source DSU Number");
      sourceDSUNumberField.value = (target as CVRFuryGameObjectInfoTag).sourceDSUNumber;
      // make the field read only
      sourceDSUNumberField.SetEnabled(false);
      rootVisualElement.Add(sourceDSUNumberField);

      // return the root element
      return rootVisualElement;
    }
  }
}
#endif
