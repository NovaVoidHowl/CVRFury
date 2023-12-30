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
