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

// only need to change the following line, here
// in 'supportingClasses\AppInternalPackages.cs'
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  [ExecuteInEditMode]
  public partial class ToolSetup : EditorWindow
  {
    private Vector2 scrollPosition;
    private const float MIN_WIDTH = 700f;
    private const float MIN_HEIGHT = 600f;

    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Tool Setup")]
    public static void ShowWindow()
    {
      ToolSetup window = (ToolSetup)EditorWindow.GetWindow(typeof(ToolSetup), true, "Tool Setup");
      window.maxSize = new Vector2(2000, 2000);
      window.minSize = new Vector2(600, 300);
      window.Show();
    }

    private void OnEnable()
    {
      renderDepMgrUI();
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
      var baseTree = Resources.Load<VisualTreeAsset>("UnityUXML/ToolSetup");

      // Check if the UXML file was loaded
      if (baseTree == null)
      {
        Debug.LogError(
          "Failed to load UXML file at 'UnityUXML/ToolSetup'. Please ensure the file exists at the specified path."
        );
        // If the UXML file was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : UXML could not be loaded."));
        return;
      }

      // Load and apply the stylesheet
      var stylesheet = Resources.Load<StyleSheet>("UnityStyleSheets/DepManager");

      // Check if the StyleSheet was loaded
      if (stylesheet == null)
      {
        Debug.LogError(
          "Failed to load StyleSheet at 'UnityStyleSheets/DepManager'. Please ensure the file exists at the specified path."
        );
        // If the StyleSheet was not loaded add a new label to the root.
        rootVisualElement.Add(new Label("CRITICAL ERROR : StyleSheet could not be loaded."));
        return;
      }

      // Instantiate the UXML tree and add it to the root
      var ToolSetup = baseTree.Instantiate();

      // apply the UXML to the root
      rootVisualElement.Add(ToolSetup);

      // Apply the StyleSheet
      rootVisualElement.styleSheets.Add(stylesheet);

      // Get the containers
      var primaryDependenciesContainer = ToolSetup.Q("primaryDependenciesContainer");
      var appComponentsContainer = ToolSetup.Q("appComponentsContainer");
      var thirdPartyDependenciesContainer = ToolSetup.Q("thirdPartyDependenciesContainer");

      // Check if the containers were found
      if (primaryDependenciesContainer == null)
      {
        Debug.LogError(
          "Failed to find 'primaryDependenciesContainer'. Please ensure the element exists in the UXML file."
        );
        return;
      }
      if (appComponentsContainer == null)
      {
        Debug.LogError("Failed to find 'appComponentsContainer'. Please ensure the element exists in the UXML file.");
        return;
      }
      if (thirdPartyDependenciesContainer == null)
      {
        Debug.LogError(
          "Failed to find 'thirdPartyDependenciesContainer'. Please ensure the element exists in the UXML file."
        );
        return;
      }

      // Add the IMGUIContainers
      //primaryDependenciesContainer.Add(new IMGUIContainer(() => renderPrimaryDependencies()));
      appComponentsContainer.Add(new IMGUIContainer(() => renderInternalDependencies(scriptingDefines)));
      //thirdPartyDependenciesContainer.Add(new IMGUIContainer(() => renderThirdPartyDependencies(scriptingDefines)));
      primaryDependenciesContainer.Add(RenderPrimaryDependencies());
      thirdPartyDependenciesContainer.Add(RenderThirdPartyDependencies(scriptingDefines));
    }

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

    private VisualElement RenderPrimaryDependencies()
    {
      // Create a new VisualElement root.
      var root = new VisualElement();
      // add a container for the

      // add the title
      var title = new Label("Primary Dependencies");
      title.AddToClassList("sectionTitle");
      root.Add(title);

      // Create a button to refresh the list of Primary dependencies
      var refreshButton = new Button(() =>
      {
        PrimaryDependenciesPackages.refreshPrimaryDependencies();
        refreshDepMgrUI();
      })
      {
        text = "Refresh Dependencies List"
      };
      refreshButton.AddToClassList("refreshButton");
      root.Add(refreshButton);

      // Check if there are any primary dependencies
      if (SharedData.PrimaryDependencies.Count == 0)
      {
        var helpBox = new Label("No first party dependencies required");
        root.Add(helpBox);
        return root;
      }

      // Load the UXML
      var visualTree = Resources.Load<VisualTreeAsset>("UnityUXML/FirstPartyDependency");

      // Iterate over each dependency
      foreach (var dependency in SharedData.PrimaryDependencies)
      {
        // Get versions, part after the '#' character in the version
        string displayedVersion = dependency.Version.Contains("#")
          ? dependency.Version.Split('#')[1]
          : dependency.Version;
        string displayedInstalledVersion = dependency.InstalledVersion.Contains("#")
          ? dependency.InstalledVersion.Split('#')[1]
          : dependency.InstalledVersion;

        // Clone the UXML for each dependency
        var templateContainer = visualTree.CloneTree();
        var dependencyContainer = templateContainer.Q<VisualElement>("dependencyContainer");

        // Remove the dependencyContainer from the templateContainer
        templateContainer.Remove(dependencyContainer);

        // install state bool
        bool installStatus = true;

        if (dependency.InstalledVersion == "Not installed")
        {
          installStatus = false;
        }

        // Set the labels
        dependencyContainer.Q<Label>("dependencyName").text = dependency.Name;
        dependencyContainer.Q<Label>("dependencyVersion").text = "Version: " + displayedVersion;
        dependencyContainer.Q<Label>("dependencyInstalledVersion").text =
          "Installed Version: " + displayedInstalledVersion;
        dependencyContainer.Q<Label>("dependencyDescription").text = dependency.Description;

        foreach (var button in dependency.Buttons)
        {
          var linkButton = new Button(() => Application.OpenURL(button.ButtonLink)) { text = button.ButtonText };
          // add the linkButton class to the button
          linkButton.AddToClassList("linkButton");
          dependencyContainer.Q<VisualElement>("ButtonsContainer").Add(linkButton);
        }

        // Get a reference to the 'installStateSideBubble' UI element
        var installStateSideBubble = dependencyContainer.Q("installStateSideBubble");

        // get a reference to the 'installStateSideBubbleLabel' UI element
        var installStateSideBubbleLabel = dependencyContainer.Q<Label>("installStateSideBubbleLabel");

        // set the text of the 'installStateSideBubbleLabel' UI element
        installStateSideBubbleLabel.text = installStatus ? "Installed" : "Not Installed";

        // rotate the 'installStateSideBubbleLabel' UI element to the correct angle (90 degrees)
        installStateSideBubbleLabel.transform.rotation = Quaternion.Euler(0, 0, 90);

        // get a reference to the 'packageStateLabelHolder' UI element
        var packageStateLabelHolder = dependencyContainer.Q("packageStateLabelHolder");

        //get a reference to the 'packageStateLabel' UI element
        var packageStateLabel = dependencyContainer.Q<Label>("packageStateLabel");

        // never will be messages here so turn off the message support
        installStateSideBubble.AddToClassList("plainBubble");

        // bool for broken dependency versions
        bool brokenDependencyVersionData = false;
        // bool for broken installed dependency versions
        bool brokenInstalledDependencyVersionData = false;

        if (displayedVersion != displayedInstalledVersion)
        {
          // check if dependency.InstalledVersion and dependency.Version are valid version strings
          bool targetHasVersion = checkIfValidVersion(displayedInstalledVersion);
          bool sourceHasVersion = checkIfValidVersion(displayedVersion);

          if (sourceHasVersion && targetHasVersion)
          {
            // if source file version is higher than target file version
            if (new Version(displayedVersion).CompareTo(new Version(displayedInstalledVersion)) > 0)
            {
              // set the install bubble to to update format
              dependencyContainer.AddToClassList("mismatchedDependencyLower");
              installStateSideBubble.AddToClassList("mismatchedDependencyLower");
              packageStateLabelHolder.AddToClassList("mismatchedDependencyLower");
              // set packageStateLabel to say the dependency is out of date
              packageStateLabel.text = "Out of date";
            }
            // if source file version is lower than target file version
            if (new Version(displayedVersion).CompareTo(new Version(displayedInstalledVersion)) < 0)
            {
              // set the install bubble to to optional format
              dependencyContainer.AddToClassList("mismatchedDependencyHigher");
              installStateSideBubble.AddToClassList("mismatchedDependencyHigher");
              packageStateLabelHolder.AddToClassList("mismatchedDependencyHigher");
              // set packageStateLabel to say that the installed dependency is newer than the required version
              packageStateLabel.text = "Newer version installed";
            }
          }
          else
          {
            // if the version strings are not valid, set the install bubble to to Unversioned format
            dependencyContainer.AddToClassList("mismatchedDependencyUnversioned");
            installStateSideBubble.AddToClassList("mismatchedDependencyUnversioned");
            packageStateLabelHolder.AddToClassList("mismatchedDependencyUnversioned");
            // set the brokenDependencyVersionData bool to true
            brokenDependencyVersionData = true;
            // set packageStateLabel to say that the version strings are not valid
            packageStateLabel.text = "Version strings invalid";
          }
        }
        else
        {
          // version strings are the same, so really has to be installed but lets check anyway
          if (dependency.InstalledVersion == "Not installed")
          {
            // this should never happen, but if it does,
            // set the install bubble to to unversioned format, how the heck did this happen?
            dependencyContainer.AddToClassList("mismatchedDependencyUnversioned");
            installStateSideBubble.AddToClassList("mismatchedDependencyUnversioned");
            packageStateLabelHolder.AddToClassList("mismatchedDependencyUnversioned");
            // set the brokenDependencyVersionData bool to true
            brokenInstalledDependencyVersionData = true;
            // show popup message asking the user to report this error
            EditorUtility.DisplayDialog(
              "Error",
              "This should not have occurred, but it seems that the installed version of "
                + dependency.Name
                + " is corrupted. Please report this error to the developer.",
              "OK"
            );
            // set packageStateLabel to say that the installed version is corrupted
            packageStateLabel.text = "Installed version corrupted";
          }
          else
          {
            // set the install bubble to to installed format
            dependencyContainer.AddToClassList("installedDependency");
            installStateSideBubble.AddToClassList("installedDependency");
            packageStateLabelHolder.AddToClassList("installedDependency");
            // set packageStateLabel to say that the dependency is up to date
            packageStateLabel.text = "Up to date";
          }
        }

        // Add the dependency box to the root
        root.Add(dependencyContainer);
      }

      // Add button to install the dependencies
      var installOrUpdateButton = new Button(() =>
      {
        ApplyDependencies();
        // Refresh the UI
        refreshDepMgrUI();
      })
      {
        text = "Update Dependencies"
      };
      // set the button class to 'linkButton'
      installOrUpdateButton.AddToClassList("linkButton");
      // Add the button to the root
      root.Add(installOrUpdateButton);

      // return the root
      return root;
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
        // use the AppInternalPackages class to refresh the list of app components
        AppInternalPackages.refreshAppComponentsList();
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
          "Packages/" + Constants.PACKAGE_NAME + "/Assets/Resources/AppComponents/Editor/" + appComponent + ".source";
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
