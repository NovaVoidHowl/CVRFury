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
      var versionLabel = new Label("Data Store Version: " + version);
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
      // allow clicking through
      componentTypeVisualElement.pickingMode = PickingMode.Ignore;

      // switch statement for the version of the VRCFury component
      switch (version)
      {
        case 2:
          componentTypeVisualElement.Add(
            CreateComponentTopBar(
              "VRC Fury",
              "Data Store",
              Constants.VRCFURY_HEADER_PREFIX_COLOUR,
              Constants.VRCFURY_HEADER_BACKGROUND_COLOUR,
              Constants.VRCFURY_HEADER_BACKGROUND_HOVER_COLOUR
            )
          );
          // debug print to say we are in version 2
          CoreLogDebug("VRCFury component version 2 - top bar set");
          break;
        case 3:
          SetComponentTopBarV3(componentTypeVisualElement, serializedObject);
          break;
        default:
          componentTypeVisualElement.Add(
            CreateComponentTopBar(
              "VRC Fury",
              "Data Store | Unsupported Version : " + version,
              Constants.VRCFURY_HEADER_PREFIX_COLOUR,
              Constants.VRCFURY_HEADER_BACKGROUND_COLOUR,
              Constants.VRCFURY_HEADER_BACKGROUND_HOVER_COLOUR
            )
          );
          break; // do nothing
      }

      // add the componentTypeVisualElement to the rootVisualElement
      rootVisualElement.Add(componentTypeVisualElement);

      #endregion //component type banner logic

      #region unsupported component banner logic v3 only

      if (version == 3)
      {
        // get the content property from the serializedObject
        var contentPropertyBanner = serializedObject.FindProperty("content");

        // get the type of the content property
        var contentTypeBanner = contentPropertyBanner.managedReferenceFullTypename;

        // check if the content is null/empty
        if (string.IsNullOrEmpty(contentTypeBanner)) { }
        else
        {
          // get the last part of the content type string (short class name)
          var contentClassNameBanner = contentTypeBanner.Split('.').Last();

          // check if the contentClassNameBanner is in the CVR_UN_NEEDED_VRCFURY_FEATURES list
          if (Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassNameBanner))
          {
            // ok this is a feature we don't need to import as its functionality is natively handled by CVR Fury
            // create a new visual element for the unsupported feature
            var unsupportedVisualElement = new VisualElement();

            // set the name of the unsupportedVisualElement to allow styling
            unsupportedVisualElement.name = "unsupportedVisualElement";

            // add the unsupportedVisualElement to the rootVisualElement
            rootVisualElement.Add(unsupportedVisualElement);

            // add label to the unsupportedVisualElement to say its an unsupported feature
            var unsupportedTitleLabel = new Label("UN-NEEDED");

            // add the unsupportedTitleLabel to the unsupportedVisualElement
            unsupportedVisualElement.Add(unsupportedTitleLabel);

            // get the visual element named VRCFuryStubCoverContent1
            var content1 = rootVisualElement.Q<VisualElement>("VRCFuryStubCoverContent1");

            //get the visual element named VRCFuryStubCoverContent2
            var content2 = rootVisualElement.Q<VisualElement>("VRCFuryStubCoverContent2");

            // get the VRCFuryStubCoverTitle visual element
            var title = rootVisualElement.Q<Label>("VRCFuryStubCoverTitle");

            // set the text of the title to the contentClassNameBanner
            title.text = "Un-Needed VRCFury Component";

            // get VRCFuryStubCoverContent1 visual element
            var content1Banner = rootVisualElement.Q<Label>("VRCFuryStubCoverContent1");

            // set the text of the content1Banner
            content1Banner.text = "This feature is natively handled by Fury, this component is not needed.";

            // get the VRCFuryStubCoverContent2 visual element
            var content2Banner = rootVisualElement.Q<Label>("VRCFuryStubCoverContent2");

            // set the text of the content2Banner
            content2Banner.text = "";

            // Show and configure the action button
            SetActionButtonVisible(true, "Remove Component");
            var removeButton = rootVisualElement.Q<Button>("VRCFuryStubCoverActionButton");
            if (removeButton != null)
            {
              removeButton.clicked += () =>
              {
                // Get the VRCFury component
                var vrcFury = target as VRCFury;
                // Cache the gameObject reference before destroying the component
                var gameObject = vrcFury.gameObject;

                // Record the object for undo
                Undo.DestroyObjectImmediate(vrcFury);

                // Mark the parent object as dirty after component removal
                if (gameObject != null)
                {
                  EditorUtility.SetDirty(gameObject);
                }
              };
            }

            // add the unsupportedVisualElement to the rootVisualElement
            rootVisualElement.Add(unsupportedVisualElement);
          }

          // check if the contentClassNameBanner is in the CVR_INCOMPATIBLE_VRCFURY_FEATURES list
          if (Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassNameBanner))
          {
            // ok this is a feature we don't support at all (not just a version issue)
            // create a new visual element for the unsupported feature
            var unsupportedVisualElement = new VisualElement();

            // set the name of the unsupportedVisualElement to allow styling
            unsupportedVisualElement.name = "unsupportedVisualElement";

            // add the unsupportedVisualElement to the rootVisualElement
            rootVisualElement.Add(unsupportedVisualElement);

            // add label to the unsupportedVisualElement to say its an unsupported feature
            var unsupportedTitleLabel = new Label("UNSUPPORTED");

            // add the unsupportedTitleLabel to the unsupportedVisualElement
            unsupportedVisualElement.Add(unsupportedTitleLabel);

            // get the visual element named VRCFuryStubCoverContent1
            var content1 = rootVisualElement.Q<VisualElement>("VRCFuryStubCoverContent1");

            //get the visual element named VRCFuryStubCoverContent2
            var content2 = rootVisualElement.Q<VisualElement>("VRCFuryStubCoverContent2");

            // blank the content1 and content2 visual elements
            if (content1 != null)
            {
              content1.style.display = DisplayStyle.None;
            }

            if (content2 != null)
            {
              content2.style.display = DisplayStyle.None;
            }

            // get the VRCFuryStubCoverTitle visual element
            var title = rootVisualElement.Q<Label>("VRCFuryStubCoverTitle");

            // set the text of the title to the contentClassNameBanner
            title.text = "Unsupported VRCFury Feature";

            // Show and configure the action button
            SetActionButtonVisible(true, "Remove Unsupported Component");
            var removeButton = rootVisualElement.Q<Button>("VRCFuryStubCoverActionButton");
            if (removeButton != null)
            {
              removeButton.clicked += () =>
              {
                // Get the VRCFury component
                var vrcFury = target as VRCFury;
                // Cache the gameObject reference before destroying the component
                var gameObject = vrcFury.gameObject;

                // Record the object for undo
                Undo.DestroyObjectImmediate(vrcFury);

                // Mark the parent object as dirty after component removal
                if (gameObject != null)
                {
                  EditorUtility.SetDirty(gameObject);
                }
              };
            }

            // add the unsupportedVisualElement to the rootVisualElement
            rootVisualElement.Add(unsupportedVisualElement);
          }
        }
      }

      #endregion //unsupported component banner logic v3 only



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
          var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Data Store Version");
          warningTitleLabel.AddToClassList("warning-title");
          errorVisualElement.Add(warningTitleLabel);
          errorVisualElement.Add(
            new Label("This VRC Fury component is not compatible with" + " the currently installed version of CVRFury.")
          );
          errorVisualElement.Add(
            new Label(
              "Note the converted copy of prefab will likely have a corrupted data store now so please delete it"
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
                var warningTitleLabel = new Label("WARNING: Corrupted VRC Fury Component");
                warningTitleLabel.AddToClassList("warning-title");
                errorVisualElement.Add(warningTitleLabel);
                errorVisualElement.Add(new Label("This VRC Fury component is corrupted and cannot be loaded."));
              }
              else
              {
                // ok so now we have a valid content type, we need to check what it is

                // get the last part of the content type string (short class name)
                var contentClassName = contentType.Split('.').Last();

                // debug log the content type
                CoreLogDebug("Content Type class: " + contentClassName);

                // check if the contentClassName is in the CVR_INCOMPATIBLE_VRCFURY_FEATURES list
                if (
                  Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName)
                  || Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassName)
                )
                {
                  // ok this is a feature we don't support at all (not just a version issue)
                  // content of the UI for this component will already be set to ask the user to remove it
                }
                else
                {
                  // only render detail errors on supported features

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
                      var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Import Version");
                      warningTitleLabel.AddToClassList("warning-title");
                      errorVisualElement.Add(warningTitleLabel);
                      errorVisualElement.Add(
                        new Label(
                          "This VRC Fury component is not import compatible with"
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
                    var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Import Version");
                    warningTitleLabel.AddToClassList("warning-title");
                    errorVisualElement.Add(warningTitleLabel);
                    errorVisualElement.Add(
                      new Label(
                        "This VRC Fury component '"
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
              var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Import Version");
              warningTitleLabel.AddToClassList("warning-title");
              errorVisualElement.Add(warningTitleLabel);
              errorVisualElement.Add(
                new Label(
                  "This VRC Fury component is not import compatible with"
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
        }
      }
      // After loading the UXML tree, get reference to the action button
      var actionButton = rootVisualElement.Q<Button>("VRCFuryStubCoverActionButton");

      if (actionButton != null)
      {
        // Set up click handler
        actionButton.clicked += () => {
          // Handle button click here
          // Debug.Log("Action button clicked"); // Debug log to confirm button click
        };
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
      try
      {
        if (target == null || !target)
          return;

        VRCFury vrcFury = target as VRCFury;
        if (vrcFury == null || !vrcFury || vrcFury.gameObject == null)
          return;

        var CVRFuryDevModeEnabler = vrcFury.gameObject.GetComponent<CVRFuryDevModeEnabler>();
        if (CVRFuryDevModeEnabler == null)
        {
          devModeEnabled = false;
          return;
        }

        // Ensure the event is initialized
        if (CVRFuryDevModeEnabler.OnDevModeChanged == null)
        {
          CVRFuryDevModeEnabler.OnDevModeChanged = new CVRFuryDevModeEnabler.BoolEvent();
        }

        // Get current state before changing subscriptions
        devModeEnabled = CVRFuryDevModeEnabler.DevModeEnabled;

        // Remove any existing subscription
        CVRFuryDevModeEnabler.OnDevModeChanged.RemoveListener(UpdateUI);

        // Add new subscription
        CVRFuryDevModeEnabler.OnDevModeChanged.AddListener(UpdateUI);
      }
      catch (System.Exception e)
      {
        Debug.LogWarning($"DevMode subscription warning: {e.Message}");
        devModeEnabled = false;
      }
    }

    private void OnDisable()
    {
      try
      {
        if (target is VRCFury vrcFury && vrcFury != null && vrcFury.gameObject != null)
        {
          var CVRFuryDevModeEnabler = vrcFury.gameObject.GetComponent<CVRFuryDevModeEnabler>();
          if (CVRFuryDevModeEnabler != null && CVRFuryDevModeEnabler.OnDevModeChanged != null)
          {
            CVRFuryDevModeEnabler.OnDevModeChanged.RemoveListener(UpdateUI);
          }
        }
      }
      catch (System.Exception e)
      {
        Debug.LogWarning($"DevMode cleanup warning: {e.Message}");
      }
    }

    void UpdateUI(bool newValue)
    {
      devModeEnabled = newValue;

      if (rootVisualElement == null)
        return;

      // Initialize derived classes for feature adder
      derivedClasses = GetClassesDerivedFromAbstractClass(typeof(FeatureModel));
      derivedClasses.RemoveAll(type => Constants.BLOCK_LISTED_VRCFURY_FEATURES.Contains(type.Name));

      // First, handle cleanup of existing elements
      var errorElements = rootVisualElement.Query<VisualElement>("errorVisualElement").ToList();
      foreach (var errorElement in errorElements)
      {
        rootVisualElement.Remove(errorElement);
      }

      var existingIncompatTag = rootVisualElement.Q<VisualElement>("incompatibilityTag");
      if (existingIncompatTag != null)
      {
        rootVisualElement.Remove(existingIncompatTag);
      }

      // Check for incompatibility
      bool isIncompatible = false;
      if (serializedObject != null)
      {
        int version = serializedObject.FindProperty("version").intValue;
        if (version > Constants.MAX_VRCFURY_VERSION_DATA)
        {
          isIncompatible = true;
        }
        else if (version > Constants.MAX_VRCFURY_VERSION_IMPORT && version <= Constants.MAX_VRCFURY_VERSION_DATA)
        {
          // For version 3, we need to check the content type
          if (version == 3)
          {
            var contentProperty = serializedObject.FindProperty("content");
            if (contentProperty != null)
            {
              var contentType = contentProperty.managedReferenceFullTypename;
              if (!string.IsNullOrEmpty(contentType))
              {
                var contentClassName = contentType.Split('.').Last();
                if (
                  Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName)
                  || Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassName)
                )
                {
                  isIncompatible = true;
                }
                else if (Constants.COMPATIBLE_VRCFURY_FEATURES.Any(x => x.Key == contentClassName))
                {
                  var compatibleVersion = Constants.COMPATIBLE_VRCFURY_FEATURES
                    .First(x => x.Key == contentClassName)
                    .Value;
                  isIncompatible = version > compatibleVersion;
                }
                else
                {
                  isIncompatible = true; // Unknown feature type
                }
              }
              else
              {
                isIncompatible = true; // Corrupted component
              }
            }
          }
          else
          {
            isIncompatible = true;
          }
        }
      }

      if (devModeEnabled)
      {
        // Load and apply dev mode stylesheet
        var devModeStyleSheet = Resources.Load<StyleSheet>(
          Constants.PROGRAM_DISPLAY_NAME + "/DevMode/UnityStyleSheets/VRCFuryInspector-Dev"
        );

        // Add incompatibility tag only if the component is actually incompatible
        if (isIncompatible)
        {
          var incompatTag = new VisualElement();
          incompatTag.name = "incompatibilityTag";
          incompatTag.Add(new Label("Incompatible Component"));
          rootVisualElement.Add(incompatTag);
        }

        // check if there is already a devModeTag VisualElement
        var devModeTagVisualElementExists = rootVisualElement.Q<VisualElement>("devModeTag");

        if (devModeTagVisualElementExists == null)
        {
          // apply the devModeStyleSheet to the rootVisualElement
          rootVisualElement.styleSheets.Add(devModeStyleSheet);

          // create a new VisualElement
          var devModeTagVisualElement = new VisualElement();
          devModeTagVisualElement.name = "devModeTag";
          devModeTagVisualElement.Add(new Label("Dev Mode Enabled"));
          rootVisualElement.Add(devModeTagVisualElement);
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
          var featureAdderLabel = new Label("V2 VRC Fury Feature adder controls");
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
        // Clean up dev mode elements
        var devModeTagVisualElements = rootVisualElement.Query<VisualElement>("devModeTag").ToList();
        foreach (var element in devModeTagVisualElements)
        {
          rootVisualElement.Remove(element);
        }

        var defaultEditorContainers = rootVisualElement.Query<VisualElement>("defaultEditorContainer").ToList();
        foreach (var container in defaultEditorContainers)
        {
          rootVisualElement.Remove(container);
        }

        // Re-run the compatibility checks
        if (serializedObject != null)
        {
          int version = serializedObject.FindProperty("version").intValue;
          // Re-run your existing compatibility check code here
          // This is the code block that checks versions and adds error elements
          // Copy the compatibility check logic from your CreateInspectorGUI method
          if (version > Constants.MAX_VRCFURY_VERSION_DATA)
          {
            // create a new visual element of the error type
            var errorVisualElement = new VisualElement();

            // set the name of the errorVisualElement to allow styling
            errorVisualElement.name = "errorVisualElement";

            // add the errorVisualElement to the rootVisualElement
            rootVisualElement.Add(errorVisualElement);

            // if it is, add a warning to the rootVisualElement
            var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Data Store Version");
            warningTitleLabel.AddToClassList("warning-title");
            errorVisualElement.Add(warningTitleLabel);
            errorVisualElement.Add(
              new Label(
                "This VRC Fury component is not compatible with" + " the currently installed version of CVRFury."
              )
            );
            errorVisualElement.Add(
              new Label(
                "Note the converted copy of prefab will likely have a corrupted data store now so please delete it"
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
          else if (version > Constants.MAX_VRCFURY_VERSION_IMPORT)
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
                var warningTitleLabel = new Label("WARNING: Corrupted VRC Fury Component");
                warningTitleLabel.AddToClassList("warning-title");
                errorVisualElement.Add(warningTitleLabel);
                errorVisualElement.Add(new Label("This VRC Fury component is corrupted and cannot be loaded."));
              }
              else
              {
                // ok so now we have a valid content type, we need to check what it is

                // get the last part of the content type string (short class name)
                var contentClassName = contentType.Split('.').Last();

                // debug log the content type
                CoreLogDebug("Content Type class: " + contentClassName);

                // check if tthe contentClassName is in the CVR_INCOMPATIBLE_VRCFURY_FEATURES list
                if (
                  Constants.CVR_INCOMPATIBLE_VRCFURY_FEATURES.Contains(contentClassName)
                  || Constants.CVR_UN_NEEDED_VRCFURY_FEATURES.Contains(contentClassName)
                )
                {
                  // ok this is a feature we don't support at all (not just a version issue)
                  // content of the UI for this component will already be set to ask the user to remove it
                }
                else
                {
                  // only render detail errors on supported features

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
                      var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Import Version");
                      warningTitleLabel.AddToClassList("warning-title");
                      errorVisualElement.Add(warningTitleLabel);
                      errorVisualElement.Add(
                        new Label(
                          "This VRC Fury component is not import compatible with"
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
                    var warningTitleLabel = new Label("WARNING: Incompatible VRC Fury Import Version");
                    warningTitleLabel.AddToClassList("warning-title");
                    errorVisualElement.Add(warningTitleLabel);
                    errorVisualElement.Add(
                      new Label(
                        "This VRC Fury component '"
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
                  "This VRC Fury component is not import compatible with"
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
        }
      }

      rootVisualElement.MarkDirtyRepaint();
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

    private void SetComponentTopBarV3(VisualElement componentTypeVisualElement, SerializedObject serializedObject)
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

        // render the component type banner
        componentTypeVisualElement.Add(
          CreateComponentTopBar(
            "VRC Fury",
            "Data Store  |  " + contentClassName,
            Constants.VRCFURY_HEADER_PREFIX_COLOUR,
            Constants.VRCFURY_HEADER_BACKGROUND_COLOUR,
            Constants.VRCFURY_HEADER_BACKGROUND_HOVER_COLOUR
          )
        );

        // If this is a Toggle component, add the action count summary to the main component body
        if (contentClassName == "Toggle")
        {
          // Create the summary after all the default content
          var summaryContainer = AddActionCountSummary(contentProperty);
          if (summaryContainer != null)
          {
            rootVisualElement.Add(summaryContainer);
          }
        }
      }
    }

    private VisualElement AddActionCountSummary(SerializedProperty contentProperty)
    {
      // Get the state property from the toggle
      var stateProperty = contentProperty.FindPropertyRelative("state");
      if (stateProperty == null)
        return null;

      // Get the actions list
      var actionsProperty = stateProperty.FindPropertyRelative("actions");
      if (actionsProperty == null)
        return null;

      // Create a dictionary to store counts of each action type
      Dictionary<string, int> actionCounts = new Dictionary<string, int>();

      // Count each type of action
      for (int i = 0; i < actionsProperty.arraySize; i++)
      {
        var actionProperty = actionsProperty.GetArrayElementAtIndex(i);
        string actionType = actionProperty.managedReferenceFullTypename?.Split('.').Last();
        if (!string.IsNullOrEmpty(actionType))
        {
          if (!actionCounts.ContainsKey(actionType))
          {
            actionCounts[actionType] = 0;
          }
          actionCounts[actionType]++;
        }
      }

      // If we have any actions, create the summary section
      if (actionCounts.Count > 0)
      {
        // Create container
        var container = new VisualElement();
        container.name = "actionSummaryContainer";
        container.AddToClassList("action-summary-container");
        container.style.flexGrow = 1;
        container.style.flexDirection = FlexDirection.Column;

        // Add header
        var header = new Label("Action Types Summary:");
        header.AddToClassList("action-summary-header");
        container.Add(header);

        // Add counts for each action type
        foreach (var kvp in actionCounts.OrderBy(x => x.Key))
        {
          var countLabel = new Label($"{kvp.Key}: {kvp.Value}");
          countLabel.AddToClassList("action-count-label");
          container.Add(countLabel);
        }

        // Add total count
        var totalCount = actionCounts.Values.Sum();
        var totalLabel = new Label($"Total Actions: {totalCount}");
        totalLabel.AddToClassList("action-total-label");
        container.Add(totalLabel);

        return container;
      }

      return null;
    }

    // Add this helper method to show/hide the button
    private void SetActionButtonVisible(bool visible, string buttonText = "")
    {
      var actionButton = rootVisualElement?.Q<Button>("VRCFuryStubCoverActionButton");
      if (actionButton != null)
      {
        actionButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (!string.IsNullOrEmpty(buttonText))
        {
          actionButton.text = buttonText;
        }
      }
    }
  }
}
#endif
