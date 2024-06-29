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

      // get the version value from the VRCFury component
      int version = serializedObject.FindProperty("version").intValue;

      // add an element to show the version of the VRCFury component
      var versionLabel = new Label("Datastore Version: " + version);
      if (version > Constants.MAX_VRCFURY_VERSION_DATA)
      {
        versionLabel.AddToClassList("version-error-label");
      }
      else
      {
        versionLabel.AddToClassList("version-ok-label");
      }
      versionLabel.AddToClassList("version-label");
      rootVisualElement.Add(versionLabel);

      #region component type banner logic

      // create a new visual element of the component type banner
      var componentTypeVisualElement = new VisualElement();

      // set the name of the componentTypeVisualElement to allow styling
      componentTypeVisualElement.name = "componentTypeVisualElement";

      // switch statement for the version of the VRCFury component
      switch (version)
      {
        case 2:
          componentTypeVisualElement.Add(CreateComponentTopBar("VRCFury Datastore"));
          // debug print to say we are in version 2
          CoreLogDebug("VRCFury component version 2 - top bar set");
          break;
        case 3:
          SetComponentTopBarV3(componentTypeVisualElement, serializedObject);
          break;
        default:
          componentTypeVisualElement.Add(CreateComponentTopBar("VRCFury Datastore | Unsupported Version : " + version));
          break; // do nothing
      }

      // add the componentTypeVisualElement to the rootVisualElement
      rootVisualElement.Add(componentTypeVisualElement);

      #endregion //component type banner logic



      // only check for compatibility if dev mode is not enabled, as dev mode allows for data store inspection etc.
      if (!devModeEnabled)
      {

        // check if its higher than the max data version, if it is, display an error as the data store is incompatible
        if (version > Constants.MAX_VRCFURY_VERSION_DATA)
        {
          // create a new visual element of the error type
          var errorVisualElement = new VisualElement();

          // set the name of the errorVisualElement to allow styling
          errorVisualElement.name = "errorVisualElement";

          // add the errorVisualElement to the rootVisualElement
          rootVisualElement.Add(errorVisualElement);

          // if it is, add a warning to the rootVisualElement
          var warningTitleLabel = new Label("WARNING: Incompatible VRCFury Data Store Version");
          warningTitleLabel.AddToClassList("warning-title");
          errorVisualElement.Add(warningTitleLabel);
          errorVisualElement.Add(
            new Label("This VRCFury component is not compatible with" + " the currently installed version of CVRFury.")
          );
          errorVisualElement.Add(
            new Label(
              "Note the converted copy of prefab will likely have a corrupted datastore now so please delete it"
            )
          );
          errorVisualElement.Add(
            new Label(
              "Please check that it was not made with a version later than " + Constants.MAX_VRCFURY_DATA_USER_VERSION
            )
          );
          errorVisualElement.Add(new Label("Please check the CVRFury documentation for more information."));
          // add button to open the documentation
          var openDocumentationButton = new Button(() =>
          {
            Application.OpenURL(Constants.DOCS_URL);
          });
          openDocumentationButton.text = "Open Documentation";
          errorVisualElement.Add(openDocumentationButton);
        }
        else
        {
          // check if its higher than the max import version, if it is, display an error as the import is incompatible
          if (version > Constants.MAX_VRCFURY_VERSION_IMPORT)
          {
            // if the version is v3 we need to check what type of component is in the 'content' variable
            if (version == 3)
            {
              // get the content property from the serializedObject
              var contentProperty = serializedObject.FindProperty("content");

              // get the type of the content property
              var contentType = contentProperty.managedReferenceFullTypename;

              // check if the content is null/empty if it is show an error saying the component is corrupted
              if (string.IsNullOrEmpty(contentType))
              {
                // create a new visual element of the error type
                var errorVisualElement = new VisualElement();

                // set the name of the errorVisualElement to allow styling
                errorVisualElement.name = "errorVisualElement";

                // add the errorVisualElement to the rootVisualElement
                rootVisualElement.Add(errorVisualElement);

                // if it is, add a warning to the rootVisualElement
                var warningTitleLabel = new Label("WARNING: Corrupted VRCFury Component");
                warningTitleLabel.AddToClassList("warning-title");
                errorVisualElement.Add(warningTitleLabel);
                errorVisualElement.Add(new Label("This VRCFury component is corrupted and cannot be loaded."));
              }
              else
              {
                // ok so now we have a valid content type, we need to check what it is

                // get the last part of the content type string (short class name)
                var contentClassName = contentType.Split('.').Last();

                // debug log the content type
                CoreLogDebug("Content Type class: " + contentClassName);

                // compare the contentClassName to the COMPATIBLE_VRCFURY_FEATURES KeyValuePair list to see if it is compatible
                // first check if the contentClassName is in the COMPATIBLE_VRCFURY_FEATURES list
                if (Constants.COMPATIBLE_VRCFURY_FEATURES.Any(x => x.Key == contentClassName))
                {
                  // get the version from the COMPATIBLE_VRCFURY_FEATURES list
                  var compatibleVersion = Constants.COMPATIBLE_VRCFURY_FEATURES
                    .First(x => x.Key == contentClassName)
                    .Value;

                  // check if the version is compatible
                  if (version > compatibleVersion)
                  {
                    // create a new visual element of the error type
                    var errorVisualElement = new VisualElement();

                    // set the name of the errorVisualElement to allow styling
                    errorVisualElement.name = "errorVisualElement";

                    // add the errorVisualElement to the rootVisualElement
                    rootVisualElement.Add(errorVisualElement);

                    // if it is, add a warning to the rootVisualElement
                    var warningTitleLabel = new Label("WARNING: Incompatible VRCFury Import Version");
                    warningTitleLabel.AddToClassList("warning-title");
                    errorVisualElement.Add(warningTitleLabel);
                    errorVisualElement.Add(
                      new Label(
                        "This VRCFury component is not import compatible with"
                          + " the currently installed version of CVRFury."
                      )
                    );
                    errorVisualElement.Add(
                      new Label(
                        "Please check that it was not made with a version later than "
                          + Constants.MAX_VRCFURY_IMPORT_USER_VERSION
                      )
                    );
                    errorVisualElement.Add(
                      new Label(
                        "Note you can review the data store of this component by setting the inspector to debug mode."
                      )
                    );
                    errorVisualElement.Add(new Label("Please check the CVRFury documentation for more information."));
                    // add button to open the documentation
                    var openDocumentationButton = new Button(() =>
                    {
                      Application.OpenURL(Constants.DOCS_URL);
                    });
                    openDocumentationButton.text = "Open Documentation";
                    errorVisualElement.Add(openDocumentationButton);
                  }
                }
                else
                {
                  // error as the contentClassName is not in the COMPATIBLE_VRCFURY_FEATURES list, could be its new
                  // feature that is not supported yet. need to display a warning
                  // create a new visual element of the error type
                  var errorVisualElement = new VisualElement();

                  // set the name of the errorVisualElement to allow styling
                  errorVisualElement.name = "errorVisualElement";

                  // add the errorVisualElement to the rootVisualElement
                  rootVisualElement.Add(errorVisualElement);

                  // if it is, add a warning to the rootVisualElement
                  var warningTitleLabel = new Label("WARNING: Incompatible VRCFury Import Version");
                  warningTitleLabel.AddToClassList("warning-title");
                  errorVisualElement.Add(warningTitleLabel);
                  errorVisualElement.Add(
                    new Label(
                      "This VRCFury component '"
                        + contentClassName
                        + "' is not import compatible with"
                        + " the currently installed version of CVRFury."
                    )
                  );
                  errorVisualElement.Add(new Label("Please check the CVRFury documentation for more information."));
                  // add button to open the documentation
                  var openDocumentationButton = new Button(() =>
                  {
                    Application.OpenURL(Constants.DOCS_URL);
                  });
                  openDocumentationButton.text = "Open Documentation";
                  errorVisualElement.Add(openDocumentationButton);
                }
              }
            }
            else
            {
              // create a new visual element of the error type
              var errorVisualElement = new VisualElement();

              // set the name of the errorVisualElement to allow styling
              errorVisualElement.name = "errorVisualElement";

              // add the errorVisualElement to the rootVisualElement
              rootVisualElement.Add(errorVisualElement);

              // if it is, add a warning to the rootVisualElement
              var warningTitleLabel = new Label("WARNING: Incompatible VRCFury Import Version");
              warningTitleLabel.AddToClassList("warning-title");
              errorVisualElement.Add(warningTitleLabel);
              errorVisualElement.Add(
                new Label(
                  "This VRCFury component is not import compatible with" + " the currently installed version of CVRFury."
                )
              );
              errorVisualElement.Add(
                new Label(
                  "Please check that it was not made with a version later than "
                    + Constants.MAX_VRCFURY_IMPORT_USER_VERSION
                )
              );
              errorVisualElement.Add(
                new Label("Note you can review the data store of this component by setting the inspector to debug mode.")
              );
              errorVisualElement.Add(new Label("Please check the CVRFury documentation for more information."));
              // add button to open the documentation
              var openDocumentationButton = new Button(() =>
              {
                Application.OpenURL(Constants.DOCS_URL);
              });
              openDocumentationButton.text = "Open Documentation";
              errorVisualElement.Add(openDocumentationButton);
            }
          }
        }
        
      }
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
      if (target is VRCFury vrcFury && vrcFury != null)
      {
        var CVRFuryDevModeEnabler = vrcFury.gameObject?.GetComponent<CVRFuryDevModeEnabler>();
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
          featureAdderContainer.name = "featureAdderContainerV2";
          defaultEditorContainer.Add(featureAdderContainer);

          // add label for the feature adder to say its for V2 VRCFury features
          var featureAdderLabel = new Label("V2 VRCFury Feature adder controls");
          featureAdderContainer.Add(featureAdderLabel);

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

          // get the version value from the VRCFury component
          int version = serializedObject.FindProperty("version").intValue;

          if (version != 2)
          {
            featureAdderContainer.style.opacity = 0.5f;
            featureAdderContainer.SetEnabled(false);
          }
          else
          {
            featureAdderContainer.style.opacity = 1.0f;
            featureAdderContainer.SetEnabled(true);
          }

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


    private static void SetComponentTopBarV3(
      VisualElement componentTypeVisualElement,
      SerializedObject serializedObject
    )
    {
      /// get the content property from the serializedObject
      var contentProperty = serializedObject.FindProperty("content");

      // check if the content is not null/empty
      if (!string.IsNullOrEmpty(contentProperty.managedReferenceFullTypename))
      {
        // get the type of the content property
        var contentType = contentProperty.managedReferenceFullTypename;

        // get the last part of the content type string (short class name)
        var contentClassName = contentType.Split('.').Last();

        // ok we have the content class name, now we can add a banner to show the
        // user what type of component it is

        // render the component type banner
        componentTypeVisualElement.Add(CreateComponentTopBar("VRCFury Datastore  |  " + contentClassName));
      }
    }

    private static VisualElement CreateComponentTopBar(String title)
    {
      // Create a new VisualElement
      var topBar = new VisualElement();

      // Set the name of the topBar to allow styling
      topBar.name = "topBar";

      // create a visual element for the topBar content
      var topBarContent = new VisualElement();

      // Set the name of the topBarContent to allow styling
      topBarContent.name = "topBarContent";

      // Add the topBarContent to the topBar
      topBar.Add(topBarContent);

      // Add a label to the topBar to show the component type
      var label = new Label(title);
      label.style.unityFontStyleAndWeight = FontStyle.Bold; // Make the label text bold
      topBarContent.Add(label);

      return topBar;
    }
  }
}
#endif
