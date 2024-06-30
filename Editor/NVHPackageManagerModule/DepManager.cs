// Version 2.1.1
// editor only script to manage the dependencies
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Only need to change the following line, in the following files:
//
// DepManager.cs
// render1stPartyDeps.cs
// render3rdPartyDeps.cs
// renderAppComponents.cs
// renderCoreError.cs
// Validation.cs
// AppInternalPackages.cs
// DepManagerConfig.cs
// PrimaryDependenciesPackages.cs
// ThirdPartyDependenciesPackages.cs
//
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

// NVH Package Manager Module
namespace uk.novavoidhowl.dev.cvrfury.nvhpmm
{
  [ExecuteInEditMode]
  public partial class ToolSetup : EditorWindow
  {
    private Vector2 scrollPosition;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Tool Setup", false, 10000)]
    public static void ShowWindow()
    {
      ToolSetup window = (ToolSetup)
        EditorWindow.GetWindow(typeof(ToolSetup), true, "Tool Setup - " + Constants.PROGRAM_DISPLAY_NAME);
      window.maxSize = new Vector2(2000, 2000);
      window.minSize = new Vector2(900, 300);
      window.Show();
    }

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Tool Setup", true)]
    public static bool ValidateShowWindow()
    {
      // get the config values
      var result = DepManagerConfValidateAndLoad();

      bool fileFound = result.Item1;
      bool FileParseOK = result.Item2;
      VisualElement errorContainer = result.Item3;
      DepManagerConfig nvhpmmConfig = result.Item4;

      // if the config file was not found, or was not parsed ok, then allow the menu item to be disabled
      // as there are error messages to be shown
      if (!fileFound || !FileParseOK)
      {
        return true;
      }

      // check if all the deps are disabled
      if (
        !nvhpmmConfig.ThirdPartyDepsEnabled && !nvhpmmConfig.FirstPartyDepsEnabled && !nvhpmmConfig.AppComponentsEnabled
      )
      {
        // if all the deps are disabled, then hide the menu item
        return false;
      }

      // if we get here, then the config file was found, and parsed ok, and at least one dep is enabled
      // so allow the menu item to be enabled
      return true;
    }

    private void OnEnable()
    {
      refreshDepMgrUI();
      EditorApplication.projectChanged += refreshDepMgrUI;
    }

    private void OnDisable()
    {
      EditorApplication.projectChanged -= refreshDepMgrUI;
    }

    private void refreshDepMgrUI()
    {
      // remove all children from the root
      rootVisualElement.Clear();
      // re-render the UI
      renderDepMgrUI();
    }

    private void renderDepMgrUI()
    {
      // Get the scripting defines
      string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup
      );

      // load base UXML
      var baseTree = Resources.Load<VisualTreeAsset>(Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/UnityUXML/ToolSetup");

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        CoreLogError(
          "Failed to load UXML file at 'UnityUXML/ToolSetup'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/UnityStyleSheets/DepManager"
      );

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        CoreLogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/DepManager'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return;
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

      // Get the containers
      var scrollViewsContainer = rootVisualElement.Q("scrollViewContainer");
      var primaryDependenciesContainer = rootVisualElement.Q("primaryDependenciesContainer");
      var appComponentsContainer = rootVisualElement.Q("appComponentsContainer");
      var thirdPartyDependenciesContainer = rootVisualElement.Q("thirdPartyDependenciesContainer");

      // Check if the containers were found
      if (primaryDependenciesContainer == null)
      {
        CoreLogError(
          "Failed to find 'primaryDependenciesContainer'. Please ensure the element exists in the UXML file."
        );
        return;
      }
      if (appComponentsContainer == null)
      {
        CoreLogError("Failed to find 'appComponentsContainer'. Please ensure the element exists in the UXML file.");
        return;
      }
      if (thirdPartyDependenciesContainer == null)
      {
        CoreLogError(
          "Failed to find 'thirdPartyDependenciesContainer'. Please ensure the element exists in the UXML file."
        );
        return;
      }

      var result = DepManagerConfValidateAndLoad();

      bool fileFound = result.Item1;
      bool FileParseOK = result.Item2;
      VisualElement errorContainer = result.Item3;
      DepManagerConfig nvhpmmConfig = result.Item4;

      if (!fileFound || !FileParseOK)
      {
        // add the errorContainer to the root
        rootVisualElement.Add(errorContainer);
        // make the errorContainer use all the available space
        errorContainer.style.height = new Length(100, LengthUnit.Percent);
        errorContainer.style.width = new Length(100, LengthUnit.Percent);

        // hide the other containers
        scrollViewsContainer.style.display = DisplayStyle.None;
        primaryDependenciesContainer.style.display = DisplayStyle.None;
        appComponentsContainer.style.display = DisplayStyle.None;
        thirdPartyDependenciesContainer.style.display = DisplayStyle.None;

        // add a button to refresh the UI
        var refreshButton = new Button(() =>
        {
          refreshDepMgrUI();
        })
        {
          text = "Refresh UI"
        };
        // add the refreshButton class to the refreshButton
        refreshButton.AddToClassList("refreshButton");

        // add margin to the refreshButton
        refreshButton.style.marginTop = new StyleLength(20f);
        refreshButton.style.marginRight = new StyleLength(20f);

        // add the refreshButton to the errorContainer
        errorContainer.Add(refreshButton);

        // if the config file was not found, or was not parsed ok, then we can't continue
        return;
      }

      // if the config was loaded, check if the 3rd party deps are enabled
      if (!nvhpmmConfig.ThirdPartyDepsEnabled)
      {
        // hide the 3rd party deps container
        thirdPartyDependenciesContainer.style.display = DisplayStyle.None;
      }
      else
      {
        // 3rd party deps are enabled, so render the 3rd party deps container
        thirdPartyDependenciesContainer.Add(RenderThirdPartyDependencies(scriptingDefines));
      }

      // if the config was loaded, check if the 1st party deps are enabled
      if (!nvhpmmConfig.FirstPartyDepsEnabled)
      {
        // hide the 1st party deps container
        primaryDependenciesContainer.style.display = DisplayStyle.None;
      }
      else
      {
        // 1st party deps are enabled, so render the 1st party deps container
        primaryDependenciesContainer.Add(RenderPrimaryDependencies());
      }

      // if the config was loaded, check if the app components are enabled
      if (!nvhpmmConfig.AppComponentsEnabled)
      {
        // hide the app components container
        appComponentsContainer.style.display = DisplayStyle.None;
      }
      else
      {
        // app components are enabled, so render the app components container
        appComponentsContainer.Add(renderAppComponents(scriptingDefines));
      }

      // if all the deps are disabled, then hide the scrollViewsContainer, and show an info message
      if (
        !nvhpmmConfig.ThirdPartyDepsEnabled && !nvhpmmConfig.FirstPartyDepsEnabled && !nvhpmmConfig.AppComponentsEnabled
      )
      {
        // hide the scrollViewsContainer
        scrollViewsContainer.style.display = DisplayStyle.None;

        // add message container to the root
        var messageContainer = new VisualElement();
        // make message container flex layout with vertical direction
        messageContainer.style.flexDirection = FlexDirection.Column;
        // set sub element alignment to center
        messageContainer.style.alignItems = Align.Center;
        // set justify content to center
        messageContainer.style.justifyContent = Justify.Center;
        // set the height to 100%
        messageContainer.style.height = new Length(100, LengthUnit.Percent);
        // set the width to 100%
        messageContainer.style.width = new Length(100, LengthUnit.Percent);

        // add the info icon to the messageContainer
        var icon = new VisualElement();
        // add the icon class to the icon
        icon.AddToClassList("icon");
        // set the width and height of the icon to 40px
        icon.style.width = new Length(40, LengthUnit.Pixel);
        icon.style.height = new Length(40, LengthUnit.Pixel);
        // load the VectorImage from the Resources folder
        VectorImage infoIcon = Resources.Load<VectorImage>(
          Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/IconsAndImages/info"
        );
        // create a StyleBackground from the VectorImage
        StyleBackground infoBackground = new StyleBackground(infoIcon);
        // set the StyleBackground as the background image for the 'icon' UI element
        icon.style.backgroundImage = infoBackground;
        // add margin to the icon
        icon.style.marginBottom = new StyleLength(20f);

        // add the icon to the messageContainer
        messageContainer.Add(icon);

        // add a label to say that all deps are disabled
        var infoMessage = new Label("All dependency/component are disabled in the config file");
        // add the info message class
        infoMessage.AddToClassList("infoMessage");
        messageContainer.Add(infoMessage);

        // add a button to refresh the UI
        var refreshButton = new Button(() =>
        {
          refreshDepMgrUI();
        })
        {
          text = "Refresh UI"
        };
        // add the refreshButton class to the refreshButton
        refreshButton.AddToClassList("refreshButton");

        // add margin to the refreshButton
        refreshButton.style.marginTop = new StyleLength(20f);
        refreshButton.style.marginRight = new StyleLength(20f);

        // add the refreshButton to the messageContainer
        messageContainer.Add(refreshButton);

        // add the messageContainer to the root
        rootVisualElement.Add(messageContainer);
      }
    }

    // Texture2D MakeTex(int width, int height, Color col)
    // {
    //   Color[] pix = new Color[width * height];
    //   for (int i = 0; i < pix.Length; i++)
    //     pix[i] = col;
    //   Texture2D result = new Texture2D(width, height);
    //   result.SetPixels(pix);
    //   result.Apply();
    //   return result;
    // }

    public static Dictionary<string, object> getInternalPackageInfoFromFile(string file)
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      string firstLine = File.ReadLines(file).FirstOrDefault();

      //if the first two characters of the first line are '//' then it its the line we want
      // remove the // from the start of the line
      if (firstLine != null && firstLine.StartsWith("//"))
      {
        firstLine = firstLine.Substring(2);
        //trim any blank space from the start and end of the line
        firstLine = firstLine.Trim();

        // check to see if the rest of the line will parse as json
        try
        {
          dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(firstLine);
        }
        catch (JsonReaderException ex)
        {
          CoreLog(ex.Message);
          CoreLog("suspect legacy format internal package"+ firstLine);

          // if it does not parse as json, then it is not the line we want
          // set dict to null, so we can see that it is not valid
          dict = null;
        }
      }
      else
      {
        // if the first two characters of the first line are not '//' then it is not the line we want
        // set dict to null, so we can see that it is not valid
        dict = null;
      }

      return dict;
    }

    private string getTypeOfAppComponentFromFile(string file)
    {
      Dictionary<string, object> dict = getInternalPackageInfoFromFile(file);
      string typeOfAppComponent;

      if (dict != null)
      {
        // check if there is a 'type' key in the dictionary
        if (!dict.ContainsKey("type"))
        {
          // if there is no 'type' key, so we can't get the typeOfAppComponent string
          // set the typeOfAppComponent string to empty
          typeOfAppComponent = "";
        }
        else
        {
          // get the value of the 'type' key as a string
          typeOfAppComponent = dict["type"].ToString();
        }
      }
      else
      {
        // if the dictionary is null, then the file does not contain the line we want
        // set the typeOfAppComponent string to empty
        typeOfAppComponent = "Unknown";
      }
      return typeOfAppComponent;
    }

    private string getVersionFromFile(string file)
    {
      Dictionary<string, object> dict = getInternalPackageInfoFromFile(file);
      string fileVersion;

      if (dict != null)
      {
        // check if there is a 'version' key in the dictionary
        if (!dict.ContainsKey("version"))
        {
          // if there is no 'version' key, so we can't get the version string
          // set the version string to empty
          fileVersion = "";
        }
        else
        {
          // get the value of the 'version' key as a string
          fileVersion = dict["version"].ToString();
        }
      }
      else
      {
        // if the dictionary is null, then the file does not contain the line we want
        // set the version string to empty
        fileVersion = "";
      }
      return fileVersion;
    }

    private bool checkPackageCanInstall(string file, bool canInstall)
    {
      Dictionary<string, object> dict = getInternalPackageInfoFromFile(file);
      if (dict != null)
      {
        // check if there is a 'canInstall' key in the dictionary
        if (dict.ContainsKey("canInstall"))
        {
          // get the value of the 'canInstall' key as a bool
          canInstall = (bool)dict["canInstall"];
        }
      }
      else
      {
        // if the dictionary is null, then the file does not contain the line we want
        // set the canInstall bool to false, as we have no way of knowing if it is allowed to be installed
        canInstall = false;
      }
      return canInstall;
    }

    private bool checkIfPackageIsOptional(string file, bool optional)
    {
      Dictionary<string, object> dict = getInternalPackageInfoFromFile(file);
      if (dict != null)
      {
        // check if there is a 'optional' key in the dictionary
        if (dict.ContainsKey("optional"))
        {
          // get the value of the 'optional' key as a bool
          optional = (bool)dict["optional"];
        }
      }
      else
      {
        // if the dictionary is null, then the file does not contain the line we want
        // set the optional bool to false, as we have no way of knowing if it is optional
        optional = false;
      }
      return optional;
    }

    private string getInternalPackageButtonLabel(string installedVersion, string sourceVersion, bool notInstalled)
    {
      string buttonText = ".";

      // validate versions strings
      bool targetHasVersion = checkIfValidVersion(installedVersion);
      bool sourceHasVersion = checkIfValidVersion(sourceVersion);

      if (sourceHasVersion && targetHasVersion)
      {
        // if source file version is higher than target file version
        if (new Version(sourceVersion).CompareTo(new Version(installedVersion)) > 0)
        {
          // show update button
          buttonText = "Update";
        }
        // if source file version is lower than target file version
        if (new Version(sourceVersion).CompareTo(new Version(installedVersion)) < 0)
        {
          // show install button
          buttonText = "Downgrade";
        }
      }

      if (sourceHasVersion && !targetHasVersion)
      {
        // show update button
        buttonText = "Update";
      }

      // if source file version is the same as target file version
      if (sourceVersion == installedVersion)
      {
        // show install button
        buttonText = "Reinstall";
      }

      // if target file does not exist
      if (notInstalled)
      {
        buttonText = "Install";
      }

      return buttonText;
    }
  }
}
#endif
