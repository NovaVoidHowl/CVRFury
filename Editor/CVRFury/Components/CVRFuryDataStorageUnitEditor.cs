#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VF.Model;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(CVRFuryDataStorageUnit))]
  public class CVRFuryDataStorageUnitEditor : Editor
  {
    public override VisualElement CreateInspectorGUI()
    {
      // Create the root VisualElement
      var rootVisualElement = new VisualElement();

      // set the class of the root element to allow styling
      rootVisualElement.AddToClassList("cvr-fury-inspector");

      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityUXML/CVRFuryDataStorageUnitInspector"
      );

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        Debug.LogError(
          "Failed to load UXML file at 'UnityUXML/CVRFuryDataStorageUnitInspector'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return rootVisualElement;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/CVRFuryComponents/UnityStyleSheets/CVRFuryDataStorageUnitInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/CVRFuryDataStorageUnitInspector'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return rootVisualElement;
      }

      // Instantiate the UXML tree
      var ToolSetup = baseTree.Instantiate();

      // Create a temporary list to hold the children
      List<VisualElement> children = new List<VisualElement>(ToolSetup.Children());

      // Add the children of the instantiated UXML to the root
      foreach (var child in children)
      {
        rootVisualElement.Add(child);
      }

      // get the logo VisualElement
      var logo = rootVisualElement.Q<VisualElement>("logo");

      // read the logo from the resources
      var logoTexture = Resources.Load<Texture2D>(Constants.PROGRAM_DISPLAY_NAME + "/Logos/CVRFuryLogo");

      // Check if the logo was loaded
      if (logoTexture == null)
      {
        Debug.LogError(
          "Failed to load logo at 'Logos/CVRFuryLogo'. Please ensure the file exists at the specified path."
        );
        // If the logo was not loaded add a new label to the root.
        logo.Add(new Label("CRITICAL ERROR : Logo could not be loaded."));
      }
      else
      {
        // If the logo was loaded set the texture
        logo.style.backgroundImage = logoTexture;
      }

      // Apply the StyleSheet
      rootVisualElement.styleSheets.Add(stylesheet);

      return rootVisualElement;
    }
  }
}
#endif
