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

namespace uk.novavoidhowl.dev.cvrfury.nvhpmm
{
  public partial class ToolSetup : EditorWindow
  {
    
    private VisualElement renderAppComponents(string scriptingDefines)
    {
      // create a new section root element
      var root = new VisualElement();

      // add title
      var title = new Label("App Components");
      // add the sectionTitle class to the title
      title.AddToClassList("sectionTitle");
      root.Add(title);

      // refresh the list of app components, on load
      AppInternalPackages.refreshAppComponentsList();

      // UIElements button to refresh the list of Third Party dependencies
      var refreshButton = new Button(() =>
      {
        // use the AppInternalPackages class to refresh the list of app components
        AppInternalPackages.refreshAppComponentsList();
        refreshDepMgrUI();
      })
      {
        text = "Refresh App Components List"
      };

      // add the refreshButton class to the refreshButton
      refreshButton.AddToClassList("refreshButton");
      root.Add(refreshButton);

      // TODO:  need this checks bit to be defined in json like the packages are
      // and make it per component, not just a global thing
      if (!scriptingDefines.Contains("CVR_CCK_EXISTS"))
      {
        // CVR CCK is not installed

        // add a label to say that the CVR CCK must be installed first, before this section can be used
        var infoMessage = new Label("CVR CCK must be installed first, before using this section");
        // add the info message class
        infoMessage.AddToClassList("infoMessage");
        root.Add(infoMessage);

        // as this is the break point return the root
        return root;
      }

      // get count of app components
      int appComponentsCount = SharedData.appComponentsList.Count;

      // if there are no app components
      if (appComponentsCount == 0)
      {
        // add a label to say that the No extra app components found
        var infoMessage = new Label("No extra app components found");
        // add the info message class
        infoMessage.AddToClassList("infoMessage");
        root.Add(infoMessage);

        // again this is a break point so send the root back
        return root;
      }

      // Load the UXML
      var appComponentVisualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/UnityUXML/AppComponent"
      );

      // there are app components in the list so handle them
      // for each app component in SharedData.appComponentsList, check if it exists

      foreach (string appComponent in SharedData.appComponentsList)
      {
        // bool to track the state of the app component
        bool notInstalled = false;
        bool canInstall = true; // true by default, set to false if the source file has the line 'canInstall: false'

        // strings to hold the version numbers
        string installedVersion = "";
        string sourceVersion = "";

        // paths to the source and target files
        string targetFile = "";
        string targetFileBase = Constants.ASSETS_MANAGED_FOLDER + "/";
        string sourceFile =
          "Packages/"
          + Constants.PACKAGE_NAME
          + "/Assets/Resources/"
          + Constants.PROGRAM_DISPLAY_NAME
          + Constants.COMPONENTS_SOURCE_FOLDER
          + appComponent
          + ".source";


        // check if the source file is runtime or editor
        string appComponentType = getTypeOfAppComponentFromFile(sourceFile);
        switch (appComponentType)
        {
          case "Runtime":
            targetFile = targetFileBase + "Runtime/" + appComponent;
            break;
          case "Editor":
            targetFile = targetFileBase + "Editor/" + appComponent;
            break;
          default:
            // unclear type, default to editor (as older files may not have the type value)
            targetFile = targetFileBase + "Editor/" + appComponent;
            break;
        }


        // Clone the UXML for each dependency
        var templateContainer = appComponentVisualTree.CloneTree();
        var appComponentContainer = templateContainer.Q<VisualElement>("appComponentContainer");

        // Remove the appComponentContainer from the templateContainer
        templateContainer.Remove(appComponentContainer);

        // get the labels for the app component
        var componentStateLabel = appComponentContainer.Q<Label>("componentStateLabel");
        var installedVersionLabel = appComponentContainer.Q<Label>("installedVersionLabel");
        var componentNameLabel = appComponentContainer.Q<Label>("componentNameLabel");
        var componentTypeLabel = appComponentContainer.Q<Label>("componentTypeLabel");
        var sourceVersionLabel = appComponentContainer.Q<Label>("sourceVersionLabel");
        var installButton = appComponentContainer.Q<Button>("installOrUpdateButton");
        var removeButton = appComponentContainer.Q<Button>("removeButton");
        var statusPip = appComponentContainer.Q<VisualElement>("statusPip");
        var typePip = appComponentContainer.Q<VisualElement>("typePip");
        var typePipLabel = appComponentContainer.Q<Label>("typePipLabel");

        // check if target file exists
        if (AssetDatabase.LoadAssetAtPath(targetFile, typeof(UnityEngine.Object)) != null)
        {
          // target file exists
          // get version of target file
          installedVersion = getVersionFromFile(targetFile);

          // set the componentStateLabel to 'Installed'
          componentStateLabel.text = "Installed";

          if (installedVersion == null || installedVersion == "")
          {
            // installed version is null/empty
            installedVersion = "?.?.?";
          }

          // set the installedVersionLabel to the installed version
          installedVersionLabel.text = installedVersion;
        }
        else
        {
          // target file does not exist

          // set the componentStateLabel to 'Not installed'
          componentStateLabel.text = "Not installed";

          // blank the installedVersionLabel
          installedVersionLabel.text = "";
          notInstalled = true;
          // disable the remove button
          removeButton.SetEnabled(false);
        }

        // set the componentNameLabel to the appComponent name
        componentNameLabel.text = appComponent;

        // bool to track if the source file is optional
        bool? optionalExt = null;

        //check if source file exists
        if (AssetDatabase.LoadAssetAtPath(sourceFile, typeof(UnityEngine.Object)) != null)
        {
          // source file exists
          // get version of source file
          sourceVersion = getVersionFromFile(sourceFile);

          // check if the source file is allowed to be installed
          canInstall = checkPackageCanInstall(sourceFile, canInstall);

          // get the button label based on the installed version and source version
          string buttonText = getInternalPackageButtonLabel(installedVersion, sourceVersion, notInstalled);

          // set installOrUpdateButton text to buttonText
          installButton.text = buttonText;

          // check if the source file is optional
          bool optional = false;
          optionalExt = false;
          optional = checkIfPackageIsOptional(sourceFile, optional);

          // render package state (mandatory/optional/unavailable)
          if (canInstall)
          {
            if (optional)
            {
              // set the componentTypeLabel to 'Optional'
              componentTypeLabel.text = "Optional";
            }
            else
            {
              // set the componentTypeLabel to 'Mandatory'
              componentTypeLabel.text = "Mandatory";
            }
          }
          else
          {
            // set the componentTypeLabel to 'Unavailable'
            componentTypeLabel.text = "Unavailable";
          }

          if (sourceVersion == null || sourceVersion == "")
          {
            // installed version is null/empty
            sourceVersion = "?.?.?";
          }

          // set the sourceVersionLabel to the source version
          sourceVersionLabel.text = "Source: " + sourceVersion;

          // check if the source file is allowed to be installed
          if (canInstall)
          {
            // set the installOrUpdateButton to enabled
            installButton.SetEnabled(true);
          }
          else
          {
            // set the installOrUpdateButton to disabled
            installButton.SetEnabled(false);
          }

          // if the source file is optional, show a button to remove it, of not, show a disabled button
          if (optional)
          {
            // set the removeButton to enabled
            removeButton.SetEnabled(true);
          }
          else
          {
            // set the removeButton to disabled
            removeButton.SetEnabled(false);
          }

          // if the target file does not exist set the installButton text to 'Install'
          if (notInstalled)
          {
            // set the installOrUpdateButton text to 'Install'
            installButton.text = "Install";
          }
          else
          {
            // check to see if the source and target versions are valid semver
            if (checkIfValidVersion(sourceVersion) && checkIfValidVersion(installedVersion))
            {
              // convert sourceVersion and installedVersion to semver objects
              Version sourceVersionSemver = Version.Parse(sourceVersion);
              Version installedVersionSemver = Version.Parse(installedVersion);

              // check if the source version is greater than the installed version
              if (sourceVersionSemver > installedVersionSemver)
              {
                // set the installOrUpdateButton text to 'Update'
                installButton.text = "Update";
              }

              // check if the source version is less than the installed version
              if (sourceVersionSemver < installedVersionSemver)
              {
                // set the installOrUpdateButton text to 'Downgrade'
                installButton.text = "Downgrade";
              }

              // check if the source version is equal to the installed version
              if (sourceVersionSemver == installedVersionSemver)
              {
                // set the installOrUpdateButton text to 'Reinstall'
                installButton.text = "Reinstall";
              }
            }
            else
            {
              // as the source and target versions are not valid semver
              // we can't compare them, so just set the installOrUpdateButton text to 'Patch'
              installButton.text = "Patch";
            }
          }

          installButton.clicked += () =>
          {
            // copy file from resource folder to assets folder
            if (!File.Exists(sourceFile))
            {
              CoreLogError("Could not find file at path: " + sourceFile);
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

            // get the scripting define symbol suffix from the source file
            Dictionary<string, object> dict = getInternalPackageInfoFromFile(sourceFile);
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
              // if the scriptingDefineSymbolSuffix string is empty, then we can't add the define symbol
              // show error message
              CoreLogError("Could not find defineSymbolSuffix in file at path: " + sourceFile);
            }
            else
            {
              // symbol suffix found, add the define symbol

              // concat full scripting define symbol
              string symbolToBeAdded = Constants.SCRIPTING_DEFINE_SYMBOL + scriptingDefineSymbolSuffix;

              // print the symbol to be added to the console
              CoreLog("Adding scripting define symbol: " + symbolToBeAdded);

              // get the current Scripting Define Symbols
              string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
              );

              // check if the symbol is already in the scripting define symbols
              if (!scriptingDefines.Contains(symbolToBeAdded))
              {
                // add the Scripting Define Symbol (which is a concatenation of the core symbol and the defineSymbolSuffix)
                scriptingDefines += ";" + symbolToBeAdded;

                // Set the new Scripting Define Symbols
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                  EditorUserBuildSettings.selectedBuildTargetGroup,
                  scriptingDefines
                );
              }
            }

            AssetDatabase.Refresh();
            refreshDepMgrUI();
          };

          removeButton.clicked += () =>
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
              CoreLogError("Could not find defineSymbolSuffix in file at path: " + targetFile);
            }
            else
            {
              // symbol suffix found, remove the define symbol

              // concat full scripting define symbol
              string symbolToBeRemoved = Constants.SCRIPTING_DEFINE_SYMBOL + scriptingDefineSymbolSuffix;

              // print the symbol to be removed to the console
              CoreLog("Removing scripting define symbol: " + symbolToBeRemoved);

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
            refreshDepMgrUI();
          };

          // export the optional bool to the optionalExt bool
          optionalExt = optional;
        }
        else
        {
          // source file does not exist (this should not happen ever)
          // only possible case is a de-sync on the shared data, show error
          // print error message to console
          CoreLog("ERROR: source file not found: " + sourceFile);

          // set the componentStateLabel to 'Unavailable'
          componentStateLabel.text = "Unavailable";

          // set the componentTypeLabel to 'Unavailable'
          componentTypeLabel.text = "Unavailable";

          // blank the sourceVersionLabel
          sourceVersionLabel.text = "Source: ?.?.?";

          // set the installOrUpdateButton to disabled
          installButton.SetEnabled(false);

          // set the removeButton to disabled
          removeButton.SetEnabled(false);

          // hide the installOrUpdateButton
          installButton.style.display = DisplayStyle.None;

          // hide the removeButton
          removeButton.style.display = DisplayStyle.None;
        }

        // add the appComponentActionButton class to the installButton & removeButton
        installButton.AddToClassList("appComponentActionButton");
        removeButton.AddToClassList("appComponentActionButton");

        // formatting section for the appComponentContainer

        // type pip formatting
        switch (appComponentType)
        {
          case "Runtime":
            typePip.AddToClassList("runtimeType");
            // set the value of the typePipLabel to 'R'
            typePipLabel.text = "R";
            break;
          case "Editor":
            typePip.AddToClassList("editorType");
            // add a label with the letters 'E' to the typePip
            typePipLabel.text = "E";
            break;
          default:
            // Handle other cases or do nothing
            typePip.AddToClassList("unknownType");
            // add a label with the letters 'U' to the typePip
            typePipLabel.text = "U";
            break;
        }

        // install state pip formatting
        if (notInstalled)
        {
          // check if it can be installed
          if (canInstall)
          {
            // check if the source file is optional
            if (optionalExt.Value)
            {
              // set the appComponentContainer to the optional class
              appComponentContainer.AddToClassList("optionalDependencyNotInstalled");
              // set the statusPip to the optional class
              statusPip.AddToClassList("optionalDependencyNotInstalled");
            }
            else
            {
              // set the appComponentContainer to the mandatory class
              appComponentContainer.AddToClassList("mandatoryDependencyNotInstalled");
              // set the statusPip to the mandatory class
              statusPip.AddToClassList("mandatoryDependencyNotInstalled");
            }
          }
          else
          {
            // set the appComponentContainer to the unavailable class
            appComponentContainer.AddToClassList("unavailableDependency");
            // set the statusPip to the unavailable class
            statusPip.AddToClassList("unavailableDependency");
          }
        }
        else
        {
          // set appComponentContainer class to 'installedDependency'
          appComponentContainer.AddToClassList("installedDependency");
          // set the statusPip to the installed class
          statusPip.AddToClassList("installedDependency");
        }

        if (notInstalled)
        {
          // disable the remove button
          removeButton.SetEnabled(false);
        }

        // add the appComponentContainer to the root
        root.Add(appComponentContainer);
      }

      return root;
    }
  }
}
#endif
