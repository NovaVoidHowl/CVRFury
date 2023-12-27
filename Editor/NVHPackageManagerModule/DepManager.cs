// this whole file is only to be used in edit mode
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

// better hope to goodness that unity does not miss picking up the newtonsoft.json package when installing this package
// TODO: adding some sort of test here would be good
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// only need to change this line (and the asmdef) to bind to project specific constants
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public static class SharedData
  {
    public const string NVHPMM_VERSION = "2.0.0";
    public static List<string> appComponentsList = new List<string>();
    public static List<PrimaryPackageDependency> PrimaryDependencies = new List<PrimaryPackageDependency>();
    public static List<ThirdPartyPackageDependency> ThirdPartyDependencies = new List<ThirdPartyPackageDependency>();
  }

  [InitializeOnLoad]
  public class primaryDependenciesPackage
  {
    static primaryDependenciesPackage()
    {
      EditorApplication.delayCall += refreshPrimaryDependencies;
    }

    public static void refreshPrimaryDependencies()
    {
      EditorApplication.delayCall -= refreshPrimaryDependencies;

      TextAsset jsonFile = Resources.Load<TextAsset>("dependencies/PrimaryDependencies");

      if (jsonFile == null)
      {
        Debug.LogError("File not found: Assets/Resources/dependencies/PrimaryDependencies.json");
        SharedData.PrimaryDependencies = new List<PrimaryPackageDependency>(); // Set to empty list
        return;
      }

      try
      {
        var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(jsonFile.text);
      }
      catch (Newtonsoft.Json.JsonReaderException ex)
      {
        Debug.LogError("Invalid JSON: " + ex.Message);
        return;
      }

      SharedData.PrimaryDependencies = JsonConvert.DeserializeObject<List<PrimaryPackageDependency>>(jsonFile.text);
    }
  }

  public sealed class PrimaryPackageDependency
  {
    public string Name { get; }
    public string Version { get; }
    public string InstalledVersion { get; set; }

    public PrimaryPackageDependency(string name, string version)
    {
      Name = name;
      Version = version;
      InstalledVersion = GetInstalledVersion(name);
    }

    private string GetInstalledVersion(string packageName)
    {
      string manifestPath = "Packages/manifest.json";
      string manifestContent = File.ReadAllText(manifestPath);
      string pattern = $"\"{packageName}\": \"([^\"]+)\"";

      Match match = Regex.Match(manifestContent, pattern);
      return match.Success ? match.Groups[1].Value : "Not installed";
    }
  }

  [InitializeOnLoad]
  public class appInternalPackage
  {
    static appInternalPackage()
    {
      EditorApplication.delayCall += refreshAppComponentsList;
    }

    public static void refreshAppComponentsList()
    {
      EditorApplication.delayCall -= refreshAppComponentsList;

      // empty the list
      SharedData.appComponentsList.Clear();
      // look at all all the files in the appComponents folder under Resources and add them to the list if they are .source files
      string[] files = Directory.GetFiles(
        "Packages/" + Constants.PACKAGE_NAME + "/Assets/Resources/appComponents/Editor/",
        "*.source"
      );

      foreach (string file in files)
      {
        // get the filename without the extension
        string fileName = Path.GetFileNameWithoutExtension(file);
        // add the filename to the list
        SharedData.appComponentsList.Add(fileName);
      }
    }
  }

  public sealed class ThirdPartyPackageDependency
  {
    public string Name { get; }
    public string Description { get; }
    public string DependencyType { get; }
    public string InstallCheckMode { get; }
    public string InstallCheckValue { get; }
    public List<Button> Buttons { get; set; }

    public ThirdPartyPackageDependency(
      string name,
      string description,
      string dependencyType,
      string installCheckMode,
      string installCheckValue,
      List<Button> buttons
    )
    {
      Name = name;
      Description = description;
      DependencyType = dependencyType;
      InstallCheckMode = installCheckMode;
      InstallCheckValue = installCheckValue;
      Buttons = buttons;
    }
  }

  public sealed class Button
  {
    public string ButtonText { get; }
    public string ButtonLink { get; }

    public Button(string buttonText, string buttonLink)
    {
      ButtonText = buttonText;
      ButtonLink = buttonLink;
    }
  }

  [InitializeOnLoad]
  public class ThirdPartyDependenciesPackage
  {
    static ThirdPartyDependenciesPackage()
    {
      EditorApplication.delayCall += refreshThirdPartyDependencies;
    }

    public static void refreshThirdPartyDependencies()
    {
      EditorApplication.delayCall -= refreshThirdPartyDependencies;

      TextAsset jsonFile = Resources.Load<TextAsset>("dependencies/ThirdPartyDependencies");

      if (jsonFile == null)
      {
        Debug.LogError("File not found: Assets/Resources/dependencies/ThirdPartyDependencies.json");
        SharedData.ThirdPartyDependencies = new List<ThirdPartyPackageDependency>(); // Set to empty list
        return;
      }

      try
      {
        var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(jsonFile.text);
      }
      catch (Newtonsoft.Json.JsonReaderException ex)
      {
        Debug.LogError("Invalid JSON: " + ex.Message);
        return;
      }

      SharedData.ThirdPartyDependencies = JsonConvert.DeserializeObject<List<ThirdPartyPackageDependency>>(
        jsonFile.text
      );
    }
  }

  //--------------------------------------------------------------------------------------------------------------------

  [ExecuteInEditMode]
  public class ToolSetup : EditorWindow
  {
    private Vector2 scrollPosition;
    private const float MIN_WIDTH = 700f;
    private const float MIN_HEIGHT = 600f;

    Texture2D MakeTex(int width, int height, Color col)
    {
      Color[] pix = new Color[width * height];
      for (int i = 0; i < pix.Length; i++)
        pix[i] = col;
      Texture2D result = new Texture2D(width, height);
      result.SetPixels(pix);
      result.Apply();
      return result;
    }

    private void OnEnable()
    {
      minSize = new Vector2(MIN_WIDTH, position.height);
    }

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Tool Setup")]
    public static void ShowWindow()
    {
      Rect rect = new Rect(0, 0, 800, 600);
      ToolSetup window = (ToolSetup)
        EditorWindow.GetWindowWithRect(typeof(ToolSetup), rect, true, Constants.PROGRAM_DISPLAY_NAME + " Tool Setup");
      window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
    }

    private void OnGUI()
    {
      string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup
      );

      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

      EditorGUILayout.Space(10);

      renderPrimaryDependencies();

      EditorGUILayout.Space(20);

      renderInternalDependencies(scriptingDefines);

      EditorGUILayout.Space(20);

      renderThirdPartyDependencies(scriptingDefines);

      EditorGUILayout.Space(20);
      EditorGUILayout.EndScrollView();
    }

    public static bool IsImplicitPackageInstalled(string packageName)
    {
      string path = Path.Combine(Application.dataPath, "..", "Packages", "packages-lock.json");
      if (File.Exists(path))
      {
        JObject packagesLock = JObject.Parse(File.ReadAllText(path));
        return packagesLock["dependencies"]?[packageName] != null;
      }
      return false;
    }

    public static bool IsPackageInstalled(string packageName)
    {
      ListRequest request = Client.List(); // List packages installed in the project
      while (!request.IsCompleted) { } // Wait for the request to complete

      if (request.Status == StatusCode.Success)
      {
        foreach (var package in request.Result)
        {
          if (package.name == packageName)
          {
            return true; // Package is installed
          }
        }
      }
      else if (request.Status >= StatusCode.Failure)
      {
        Debug.LogError("Failed to list packages: " + request.Error.message);
      }

      return false; // Package is not installed
    }

    private static bool checkIfValidVersion(string version)
    {
      // check if version is of the format ?.?.?
      if (Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
      {
        // version is of the format ?.?.?
        return true;
      }
      else
      {
        // version is not of the format ?.?.?
        return false;
      }
    }

    private void ApplyDependencies()
    {
      string manifestPath = "Packages/manifest.json";
      string manifestContent = File.ReadAllText(manifestPath);
      foreach (var dependency in SharedData.PrimaryDependencies)
      {
        manifestContent = AddOrUpdatePackage(manifestContent, dependency.Name, dependency.Version);
      }
      File.WriteAllText(manifestPath, manifestContent);
      AssetDatabase.Refresh();
      Client.Resolve();
    }

    private static string AddOrUpdatePackage(string manifestContent, string packageName, string packageVersion)
    {
      string pattern = $"\"{packageName}\": \"([^\"]+)\"";
      Match match = Regex.Match(manifestContent, pattern);

      if (match.Success)
      {
        // found the package in the manifest
        // get the package manager 'version' part, this is the bit with the git url and the version number
        string currentPackageVersion = match.Groups[1].Value;

        // check if the currentPackageVersion contains a '#'
        if (currentPackageVersion.Contains("#"))
        {
          // if it does, then split the string at the '#' and get the second part
          currentPackageVersion = currentPackageVersion.Split('#')[1];
        }

        // check if the packageVersion contains a '#'
        if (packageVersion.Contains("#"))
        {
          // if it does, then split the string at the '#' and get the second part
          packageVersion = packageVersion.Split('#')[1];
        }

        // convert packageVersion to semver object for comparison
        Version packageVersionSemver = Version.Parse(packageVersion);
        Version currentPackageVersionSemver = Version.Parse("0.0.0");

        // check if the currentPackageVersion is valid semver and if it is convert it to semver object for comparison
        if (checkIfValidVersion(currentPackageVersion))
        {
          currentPackageVersionSemver = Version.Parse(currentPackageVersion);
        }

        if (packageVersionSemver > currentPackageVersionSemver)
        {
          // If the new version is greater than the current version, apply the update
          string replacement = $"\"{packageName}\": \"{packageVersion}\"";
          manifestContent = Regex.Replace(manifestContent, pattern, replacement);
        }
      }
      else
      {
        // If the package is not currently in the manifest, add it
        manifestContent = manifestContent.Replace(
          "\"dependencies\": {",
          $"\"dependencies\": {{\n    \"{packageName}\": \"{packageVersion}\","
        );
      }

      return manifestContent;
    }

    private void renderThirdPartyDependencies(string scriptingDefines)
    {
      // start horizontal layout
      EditorGUILayout.BeginHorizontal();
      // label for 3rd party dependencies
      EditorGUILayout.LabelField("3rd Party Dependencies", EditorStyles.boldLabel);
      // button to refresh the list of 3rd party dependencies
      if (GUILayout.Button("Refresh Dependencies List"))
      {
        ThirdPartyDependenciesPackage.refreshThirdPartyDependencies();
      }
      // end horizontal layout
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(5);

      // get count of 3rd party dependencies
      int thirdPartyDependenciesCount = SharedData.ThirdPartyDependencies.Count;

      // if there are no 3rd party dependencies
      if (thirdPartyDependenciesCount == 0)
      {
        // show message to say that there are no 3rd party dependencies
        EditorGUILayout.HelpBox("No 3rd party dependencies found.", MessageType.Info);
        return;
      }

      // for each 3rd party dependency in SharedData.ThirdPartyDependencies
      foreach (var dependency in SharedData.ThirdPartyDependencies)
      {
        // start box
        EditorGUILayout.BeginVertical("box");

        // start horizontal layout
        EditorGUILayout.BeginHorizontal();

        // label for dependency name
        EditorGUILayout.LabelField(dependency.Name, EditorStyles.boldLabel);

        // end horizontal layout
        EditorGUILayout.EndHorizontal();

        // start horizontal layout
        EditorGUILayout.BeginHorizontal();

        // label for dependency description
        EditorGUILayout.LabelField("Description: \n" + dependency.Description + "\n", EditorStyles.wordWrappedLabel);

        // end horizontal layout
        EditorGUILayout.EndHorizontal();

        // start horizontal layout
        EditorGUILayout.BeginHorizontal();

        // label for dependency type
        EditorGUILayout.LabelField("Type: " + dependency.DependencyType + "\n", EditorStyles.wordWrappedLabel);

        // end horizontal layout
        EditorGUILayout.EndHorizontal();

        // // start horizontal layout
        // EditorGUILayout.BeginHorizontal();

        // // label for dependency install check mode
        // EditorGUILayout.LabelField("Install Check Mode: " + dependency.InstallCheckMode, EditorStyles.wordWrappedLabel);

        // // end horizontal layout
        // EditorGUILayout.EndHorizontal();

        // // start horizontal layout
        // EditorGUILayout.BeginHorizontal();

        // // label for dependency install check value
        // EditorGUILayout.LabelField("Install Check Value: " + dependency.InstallCheckValue, EditorStyles.wordWrappedLabel);

        // // end horizontal layout
        // EditorGUILayout.EndHorizontal();

        // section to handel install checks

        // bool for install status
        bool installStatus = false;

        // case statement to handle the different install check modes
        switch (dependency.InstallCheckMode)
        {
          case "Scripting Define Symbol":
            // check if the scripting define symbol is defined
            if (scriptingDefines.Contains(dependency.InstallCheckValue))
            {
              // if it is, set installStatus to true
              installStatus = true;
            }
            break;
          case "Package Manager":
            // check if the package is installed
            if (IsPackageInstalled(dependency.InstallCheckValue))
            {
              // if it is, set installStatus to true
              installStatus = true;
            }
            break;
          case "Package Manager Implicit":
            // check if the package is installed
            if (IsImplicitPackageInstalled(dependency.InstallCheckValue))
            {
              // if it is, set installStatus to true
              installStatus = true;
            }
            break;
          case "File Exists":
            // check if the file exists
            if (File.Exists(dependency.InstallCheckValue))
            {
              // if it does, set installStatus to true
              installStatus = true;
            }
            break;
          case "Folder Exists":
            // check if the folder exists
            if (Directory.Exists(dependency.InstallCheckValue))
            {
              // if it does, set installStatus to true
              installStatus = true;
            }
            break;
          default:
            // if the install check mode is not recognised, show error
            EditorGUILayout.HelpBox("ERROR: Install Check Mode not recognised", MessageType.Error);
            break;
        }

        // start horizontal layout
        EditorGUILayout.BeginHorizontal();
        if (installStatus)
        {
          if (dependency.InstallCheckMode == "Package Manager Implicit")
          {
            // if installStatus is true, show label to say that the dependency is installed
            EditorGUILayout.LabelField("Install Status: Installed (Package Dependency)", EditorStyles.wordWrappedLabel);
          }
          else
          {
            // if installStatus is true, show label to say that the dependency is installed
            EditorGUILayout.LabelField("Install Status: Installed", EditorStyles.wordWrappedLabel);
          }
        }
        else
        {
          if (dependency.InstallCheckMode == "Package Manager Implicit")
          {
            // start vertical layout
            EditorGUILayout.BeginVertical();
            // if installStatus is false, show label to say that the dependency is not installed
            EditorGUILayout.LabelField(
              "Install Status: Not Installed (Package Dependency)",
              EditorStyles.wordWrappedLabel
            );
            // add warning message to say that the dependency is not installed, and the user should restart unity to
            // try and get unity to install the package
            EditorGUILayout.HelpBox(
              "Package Manager Implicit dependency not installed\n"
                + "Please restart Unity to trigger a package manager refresh/install run",
              MessageType.Error
            );
            // end vertical layout
            EditorGUILayout.EndVertical();
          }
          else
          {
            // if installStatus is false, show label to say that the dependency is not installed
            EditorGUILayout.LabelField("Install Status: Not Installed", EditorStyles.wordWrappedLabel);
          }
        }

        // end horizontal layout
        EditorGUILayout.EndHorizontal();

        // start horizontal layout
        EditorGUILayout.BeginHorizontal();

        // start vertical layout
        EditorGUILayout.BeginVertical();

        // for each button in dependency.Buttons
        foreach (var button in dependency.Buttons)
        {
          // button to open link
          if (GUILayout.Button(button.ButtonText))
          {
            Application.OpenURL(button.ButtonLink);
          }
          // gap
          EditorGUILayout.Space(4);
        }

        // end vertical layout
        EditorGUILayout.EndVertical();

        // end horizontal layout
        EditorGUILayout.EndHorizontal();

        // end box
        EditorGUILayout.EndVertical();
      }
    }

    /// <summary>
    /// Renders the primary/1st party dependencies
    /// </summary>
    private void renderPrimaryDependencies()
    {
      // begin horizontal layout
      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("1st Party Dependencies", EditorStyles.boldLabel);
      // button to refresh the list of Primary dependencies
      if (GUILayout.Button("Refresh Dependencies List"))
      {
        primaryDependenciesPackage.refreshPrimaryDependencies();
      }
      // end horizontal layout
      EditorGUILayout.EndHorizontal();
      // gap
      EditorGUILayout.Space(5);
      if (SharedData.PrimaryDependencies.Count == 0)
      {
        EditorGUILayout.HelpBox("No first party dependencies required", MessageType.Info);
        return;
      }
      // gap
      EditorGUILayout.Space(5);

      // bool to check if there are any version mismatches
      bool versionsMismatch = false;

      // start box
      EditorGUILayout.BeginVertical("box");
      foreach (var dependency in SharedData.PrimaryDependencies)
      {
        // Get versions, part after the '#' character in the version
        string displayedVersion = dependency.Version.Contains("#")
          ? dependency.Version.Split('#')[1]
          : dependency.Version;
        string displayedInstalledVersion = dependency.InstalledVersion.Contains("#")
          ? dependency.InstalledVersion.Split('#')[1]
          : dependency.InstalledVersion;

        // try to convert the version strings to semantic versioning
        Match matchCurrent = Regex.Match(
          displayedVersion,
          @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"
        );
        Match matchInstalled = Regex.Match(
          displayedInstalledVersion,
          @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"
        );

        Color boxColor = Constants.UI_UPDATE_NOT_INSTALLED_COLOR;
        // if the version strings are valid semantic versioning
        if (matchCurrent.Success && matchInstalled.Success)
        {
          // if the installed version is lower than the current version
          if (new Version(displayedInstalledVersion).CompareTo(new Version(displayedVersion)) < 0)
          {
            boxColor = Constants.UI_UPDATE_OUT_OF_DATE_COLOR;
          }
          // if the installed version is the same as the current version
          if (displayedInstalledVersion == displayedVersion)
          {
            boxColor = Constants.UI_UPDATE_OK_COLOR;
          }
          // if the installed version is higher than the current version
          if (new Version(displayedInstalledVersion).CompareTo(new Version(displayedVersion)) > 0)
          {
            boxColor = Constants.UI_UPDATE_DOWNGRADE_COLOR;
          }
        }

        // Set the background color
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.white; // Reset the color to white

        EditorGUILayout.Space(5);
        GUI.backgroundColor = boxColor; // Set the color for the child box

        // Create a custom GUIStyle

        GUIStyle customBoxStyle = new GUIStyle(GUI.skin.box);
        customBoxStyle.normal.background = MakeTex(2, 2, boxColor);
        // start box
        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField(
          "Name                     : "
            + dependency.Name
            + "\nVersion                  : "
            + displayedVersion
            + "\nInstalled Version : "
            + displayedInstalledVersion,
          EditorStyles.wordWrappedLabel
        );

        // Create a GUIStyle for the label
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        // Create a GUILayoutOption array for the label
        GUILayoutOption[] labelOptions = new GUILayoutOption[]
        {
          GUILayout.Width(200),
          GUILayout.Height(50) // Set the height of the box
        };

        // if the version strings are valid semantic versioning
        if (matchCurrent.Success && matchInstalled.Success)
        {
          // if the installed version is lower than the current version
          if (new Version(displayedInstalledVersion).CompareTo(new Version(displayedVersion)) < 0)
          {
            // show label to say that an update is available
            GUI.contentColor = Constants.UI_UPDATE_OUT_OF_DATE_COLOR_TEXT;
            EditorGUILayout.LabelField("Update available", labelStyle, labelOptions);
            GUI.contentColor = Color.white; // Reset the text color to white
            // set versionsMismatch to true
            versionsMismatch = true;
          }
          // if the installed version is the same as the current version
          if (displayedInstalledVersion == displayedVersion)
          {
            // show label to say that the package is up to date
            GUI.contentColor = Constants.UI_UPDATE_OK_COLOR_TEXT;
            EditorGUILayout.LabelField("Up to date", labelStyle, labelOptions);
            GUI.contentColor = Color.white; // Reset the text color to white
          }
          // if the installed version is higher than the current version
          if (new Version(displayedInstalledVersion).CompareTo(new Version(displayedVersion)) > 0)
          {
            // show label to say that the installed version is higher than the current version
            GUI.contentColor = Constants.UI_UPDATE_DOWNGRADE_COLOR_TEXT;
            EditorGUILayout.LabelField("Installed version is higher\n than required version", labelStyle, labelOptions);
            GUI.contentColor = Color.white; // Reset the text color to white
            // don't set versionsMismatch to true, as this is not a problem if the installed version is higher than the required version
          }
        }

        // if installed version is "Not installed"
        if (displayedInstalledVersion == "Not installed")
        {
          // show label to say that the package is not installed
          GUI.contentColor = Constants.UI_UPDATE_NOT_INSTALLED_COLOR_TEXT;
          EditorGUILayout.LabelField("Not installed", labelStyle, labelOptions);
          GUI.contentColor = Color.white; // Reset the text color to white
          // set versionsMismatch to true
          versionsMismatch = true;
        }

        EditorGUILayout.EndHorizontal();

        // Reset the background color
        GUI.backgroundColor = originalColor;
      }

      EditorGUILayout.Space(10);
      //show a warning box if there are any mismatches
      if (versionsMismatch)
      {
        EditorGUILayout.HelpBox(
          "There are out of date dependencies. Please update/install the dependencies.",
          MessageType.Error
        );
        EditorGUILayout.Space(10);
      }

      //if there are no mismatches, disable the gui for the button
      if (!versionsMismatch)
      {
        GUI.enabled = false;
      }
      // Display the button to update/install the dependencies
      if (GUILayout.Button("Update Dependencies"))
      {
        ApplyDependencies();
        //refresh the window to show the updated versions
        Repaint();
      }
      //enable gui
      GUI.enabled = true;
      // end box
      EditorGUILayout.EndVertical();
    }

    private void renderInternalDependencies(string scriptingDefines)
    {
      // start box
      EditorGUILayout.BeginHorizontal("box");

      // label for app components
      EditorGUILayout.LabelField("App Components", EditorStyles.boldLabel);
      // button to refresh the list of app components
      if (GUILayout.Button("Refresh Components List"))
      {
        // use the appInternalPackage class to refresh the list of app components
        appInternalPackage.refreshAppComponentsList();
      }
      // end box
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space(5);

      // start box
      EditorGUILayout.BeginVertical("box");

      bool blockInstall = false;

      if (!scriptingDefines.Contains("CVR_CCK_EXISTS"))
      {
        // CVR CCK is not installed
        //message to say that CVR CCK must be installed first
        EditorGUILayout.HelpBox("CVR CCK must be installed first.", MessageType.Info);
        blockInstall = true;
      }

      if (blockInstall)
      {
        //disable gui
        GUI.enabled = false;
      }
      // get count of app components
      int appComponentsCount = SharedData.appComponentsList.Count;

      // if there are no app components
      if (appComponentsCount == 0)
      {
        // show message to say that there are no app components
        EditorGUILayout.HelpBox("No extra app components found.", MessageType.Info);
      }

      // for each app component in SharedData.appComponentsList, check if it exists
      foreach (string appComponent in SharedData.appComponentsList)
      {
        EditorGUILayout.BeginHorizontal();

        string targetFile = Constants.ASSETS_MANAGED_FOLDER + "/Editor/" + appComponent;
        string sourceFile =
          "Packages/" + Constants.PACKAGE_NAME + "/Assets/Resources/appComponents/Editor/" + appComponent + ".source";
        bool notInstalled = false;
        bool canInstall = true; // true by default, set to false if the source file has the line 'canInstall: false'

        string installedVersion = "";
        string sourceVersion = "";

        // check if target file exists
        if (AssetDatabase.LoadAssetAtPath(targetFile, typeof(UnityEngine.Object)) != null)
        {
          // target file exists
          // get version of target file
          installedVersion = getVersionFromFile(targetFile);
          // render the target version string
          renderPackageVersionString(installedVersion, "Installed");
        }
        else
        {
          // target file does not exist
          EditorGUILayout.LabelField("Not installed", GUILayout.Width(110));
          notInstalled = true;
        }

        EditorGUILayout.LabelField(appComponent, EditorStyles.wordWrappedLabel);
        GUILayout.FlexibleSpace();

        // check if source file exists
        if (AssetDatabase.LoadAssetAtPath(sourceFile, typeof(UnityEngine.Object)) != null)
        {
          // source file exists
          // get version of source file
          sourceVersion = getVersionFromFile(sourceFile);

          // check if the source file is allowed to be installed
          canInstall = checkPackageCanInstall(sourceFile, canInstall);

          // get the button label based on the installed version and source version
          string buttonText = getInternalPackageButtonLabel(installedVersion, sourceVersion, notInstalled);

          // check if the source file is optional
          bool optional = false;
          optional = checkIfPackageIsOptional(sourceFile, optional);

          // render package state (mandatory/optional/unavailable)
          if (canInstall)
          {
            if (optional)
            {
              EditorGUILayout.LabelField("Optional", GUILayout.Width(80));
            }
            else
            {
              EditorGUILayout.LabelField("Mandatory", GUILayout.Width(80));
            }
          }
          else
          {
            EditorGUILayout.LabelField("Unavailable", GUILayout.Width(80));
          }

          // render the source version string
          renderPackageVersionString(sourceVersion, "Source");

          // render install button
          renderInternalPackageInstallButton(canInstall, buttonText, sourceFile, targetFile);

          // if the source file is optional, show a button to remove it, of not, show a disabled button
          renderInternalPackageRemoveButton(optional, targetFile);
        }
        else
        {
          // source file does not exist
          // this should not happen, show error
          EditorGUILayout.LabelField("ERROR: source file not found", GUILayout.Width(180));
        }
        EditorGUILayout.EndHorizontal();
      }
      //enable gui
      GUI.enabled = true;

      //end box
      EditorGUILayout.EndVertical();
    }

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
          Debug.Log(ex.Message);
          Debug.Log("suspect legacy format internal package");
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

    private void renderPackageVersionString(string version, string label)
    {
      if (version == null || version == "")
      {
        version = "?.?.?";
      }
      EditorGUILayout.LabelField(label + ": " + version, GUILayout.Width(110));
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

    private void renderInternalPackageRemoveButton(bool optional, string targetFile)
    {
      // check if target file exists
      if (AssetDatabase.LoadAssetAtPath(targetFile, typeof(UnityEngine.Object)) == null)
      {
        // target file does not exist
        // disable gui
        GUI.enabled = false;
      }

      if (optional)
      {
        //button to remove
        if (GUILayout.Button("Remove", GUILayout.Width(80)))
        {
          // get the scripting define symbol suffix from the target file
          Dictionary<string, object> dict = getInternalPackageInfoFromFile(targetFile);
          string scriptingDefineSymbolSuffix = "";
          if (dict != null)
          {
            // check if there is a 'defineSymbolSuffix' key in the dictionary
            if (dict.ContainsKey("defineSymbolSuffix"))
            {
              // get the value of the 'defineSymbolSuffix' key as a string
              scriptingDefineSymbolSuffix = dict["defineSymbolSuffix"].ToString();
            }
          }
          else
          {
            // if the dictionary is null, then the file does not contain the line we want
            // set the scriptingDefineSymbolSuffix string to empty
            scriptingDefineSymbolSuffix = "";
          }

          if (scriptingDefineSymbolSuffix == "")
          {
            // if the scriptingDefineSymbolSuffix string is empty, then we can't remove the define symbol
            // show error message
            Debug.LogError("Could not find defineSymbolSuffix in file at path: " + targetFile);
          }
          else
          {
            // symbol suffix found, remove the define symbol

            // concat full scripting define symbol
            string symbolToBeRemoved = Constants.SCRIPTING_DEFINE_SYMBOL + scriptingDefineSymbolSuffix;

            // print the symbol to be removed to the console
            Debug.Log("Removing scripting define symbol: " + symbolToBeRemoved);

            // get the current Scripting Define Symbols
            string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
              EditorUserBuildSettings.selectedBuildTargetGroup
            );

            // remove the Scripting Define Symbol (which is a concatenation of the core symbol and the defineSymbolSuffix)
            scriptingDefines = scriptingDefines.Replace(symbolToBeRemoved, "");

            // Set the new Scripting Define Symbols
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
              EditorUserBuildSettings.selectedBuildTargetGroup,
              scriptingDefines
            );
          }

          // remove file from assets folder
          if (File.Exists(targetFile))
          {
            File.Delete(targetFile);
          }
          // delete the meta file too
          if (File.Exists(targetFile + ".meta"))
          {
            File.Delete(targetFile + ".meta");
          }

          AssetDatabase.Refresh();
        }
      }
      else
      {
        // show disabled button
        GUI.enabled = false;
        GUILayout.Button("Remove", GUILayout.Width(80));
        //enable gui
        GUI.enabled = true;
      }

      // enable gui
      GUI.enabled = true;
    }

    private void renderInternalPackageInstallButton(
      bool canInstall,
      string buttonText,
      string sourceFile,
      string targetFile
    )
    {
      if (!canInstall)
      {
        // disable gui
        GUI.enabled = false;
      }

      //button to update/install
      if (GUILayout.Button(buttonText, GUILayout.Width(80)))
      {
        // copy file from resource folder to assets folder

        if (!File.Exists(sourceFile))
        {
          Debug.LogError("Could not find file at path: " + sourceFile);
          return;
        }
        string directoryPath = Path.GetDirectoryName(targetFile);
        Directory.CreateDirectory(directoryPath);

        // remove existing file
        if (File.Exists(targetFile))
        {
          File.Delete(targetFile);
        }

        FileUtil.CopyFileOrDirectory(sourceFile, targetFile);
        AssetDatabase.Refresh();

        // Install button clicked
      }
      //enable gui
      GUI.enabled = true;
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
