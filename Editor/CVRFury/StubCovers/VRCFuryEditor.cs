#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VF.Model;
using VF.Model.Feature;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using uk.novavoidhowl.dev.cvrfury.runtime;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury
{
  [CustomEditor(typeof(VRCFury))]
  public class VRCFuryEditor : Editor
  {
    // type list
    List<Type> derivedClasses;

    // dev mode enabled
    bool devModeEnabled = false;

    // Define rootVisualElement at the class level
    VisualElement rootVisualElement;

    public override VisualElement CreateInspectorGUI()
    {
      // Create the root VisualElement
      rootVisualElement = new VisualElement();

      // set the class of the root element to allow styling
      rootVisualElement.AddToClassList("cvr-fury-inspector");

      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/StubCovers/UnityUXML/VRCFuryInspector"
      );

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        CoreLogError(
          "Failed to load UXML file at 'UnityUXML/VRCFuryInspector'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return rootVisualElement;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/StubCovers/UnityStyleSheets/VRCFuryInspector"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/VRCFuryInspector'. Please ensure the file exists at the specified path."
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

      // Apply the StyleSheet
      rootVisualElement.styleSheets.Add(stylesheet);

      // subscribe to the CVRFuryDevModeEnabler component
      devModeSubscribe();
      // Call UpdateUI after creating the UI
      UpdateUI(devModeEnabled);

      return rootVisualElement;
    }

    private void OnEnable()
    {
      devModeSubscribe();
    }

    private void devModeSubscribe()
    {
      if (target is VRCFury vrcFury && vrcFury.gameObject.GetComponent<CVRFuryDevModeEnabler>() != null)
      {
        var CVRFuryDevModeEnabler = vrcFury.gameObject.GetComponent<CVRFuryDevModeEnabler>();
        CVRFuryDevModeEnabler.OnDevModeChanged.AddListener(UpdateUI);
        devModeEnabled = CVRFuryDevModeEnabler.DevModeEnabled;
      }
    }

    private void OnDisable()
    {
      if (target is VRCFury vrcFury)
      {
        var CVRFuryDevModeEnabler = vrcFury.gameObject.GetComponent<CVRFuryDevModeEnabler>();
        if (CVRFuryDevModeEnabler != null)
        {
          CVRFuryDevModeEnabler.OnDevModeChanged.RemoveListener(UpdateUI);
        }
      }
    }

    void UpdateUI(bool newValue)
    {
      devModeEnabled = newValue;

      derivedClasses = GetClassesDerivedFromAbstractClass(typeof(FeatureModel));

      // remove the blocklisted features from the derivedClasses list
      derivedClasses.RemoveAll(type => Constants.BLOCK_LISTED_VRCFURY_FEATURES.Contains(type.Name));

      if (rootVisualElement == null)
      {
        // if the rootVisualElement is null, return
        return;
      }

      if (devModeEnabled)
      {
        // load the devMode uss from resources
        var devModeStyleSheet = Resources.Load<StyleSheet>(
          Constants.PROGRAM_DISPLAY_NAME + "/DevMode/UnityStyleSheets/VRCFuryInspector-Dev"
        );

        // check if there is already a devModeTag VisualElement
        var devModeTagVisualElementExists = rootVisualElement.Q<VisualElement>("devModeTag");

        // if there is not already a devModeTag VisualElement
        if (devModeTagVisualElementExists == null)
        {
          // apply the devModeStyleSheet to the rootVisualElement
          rootVisualElement.styleSheets.Add(devModeStyleSheet);

          // create a new VisualElement
          var devModeTagVisualElement = new VisualElement();

          // set the name of the devModeTagVisualElement to allow styling
          devModeTagVisualElement.name = "devModeTag";

          // add the devModeTagVisualElement to the rootVisualElement
          rootVisualElement.Add(devModeTagVisualElement);

          // set the text of the devModeTagVisualElement to 'Dev Mode Enabled'
          devModeTagVisualElement.Add(new Label("Dev Mode Enabled"));
        }

        // check if there is already a defaultEditorContainer VisualElement
        var defaultEditorContainerExists = rootVisualElement.Q<VisualElement>("defaultEditorContainer");

        // if there is not already a defaultEditorContainer VisualElement
        if (defaultEditorContainerExists == null)
        {
          // create a new VisualElement
          var defaultEditorContainer = new VisualElement();

          // set the name of the defaultEditorContainer to allow styling
          defaultEditorContainer.name = "defaultEditorContainer";

          // add the defaultEditorContainer to the rootVisualElement
          rootVisualElement.Add(defaultEditorContainer);

          // show the default editor in an IMGUI container
          var defaultEditor = CreateEditor(target);
          var defaultEditorIMGUIContainer = new IMGUIContainer(() =>
          {
            defaultEditor.OnInspectorGUI();
          });
          defaultEditorContainer.Add(defaultEditorIMGUIContainer);

          // add container for the Feature adder
          var featureAdderContainer = new VisualElement();
          featureAdderContainer.name = "featureAdderContainer";
          defaultEditorContainer.Add(featureAdderContainer);

          // add dropdown for feature type
          var featureTypeDropdown = new PopupField<string>("Feature Type", derivedClasses.ConvertAll(x => x.Name), 0);
          featureAdderContainer.Add(featureTypeDropdown);

          // add button to add feature
          var addFeatureButton = new Button(() =>
          {
            var featureType = derivedClasses[featureTypeDropdown.index];
            if (typeof(FeatureModel).IsAssignableFrom(featureType))
            {
              var feature = Activator.CreateInstance(featureType) as FeatureModel;
              if (feature != null)
              {
                ((VRCFury)target).config.features.Add(feature);
                UpdateUI(devModeEnabled);
              }
              else
              {
                // Handle the case when the instance cannot be created or casted to FeatureModel
              }
            }
            else
            {
              // Handle the case when featureType is not a subclass of FeatureModel
            }
          });
          addFeatureButton.text = "Add Feature";
          featureAdderContainer.Add(addFeatureButton);
        }
      }
      else
      {
        // devmode not enabled so do a cleanup of the devmode elements

        // find and remove all instances of the devModeTagVisualElement
        var devModeTagVisualElements = rootVisualElement.Query<VisualElement>("devModeTag").ToList();
        foreach (var devModeTagVisualElement in devModeTagVisualElements)
        {
          rootVisualElement.Remove(devModeTagVisualElement);
        }

        // find and remove all instances of the defaultEditorContainer
        var defaultEditorContainers = rootVisualElement.Query<VisualElement>("defaultEditorContainer").ToList();
        foreach (var defaultEditorContainer in defaultEditorContainers)
        {
          rootVisualElement.Remove(defaultEditorContainer);
        }
      }
      Repaint();
    }

    public static List<Type> GetClassesDerivedFromAbstractClass(Type baseType)
    {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();

      var types = new List<Type>();

      foreach (var assembly in assemblies)
      {
        types.AddRange(assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType)));
      }

      return types;
    }
  }
}
#endif
