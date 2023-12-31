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

// only need to change the following line, here
// in 'supportingClasses\AppInternalPackages.cs'
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public partial class ToolSetup : EditorWindow
  {
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
      var visualTree = Resources.Load<VisualTreeAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/UnityUXML/FirstPartyDependency"
      );

      // bool for broken dependency versions
      bool brokenDependencyVersionData = false;
      // bool for broken installed dependency versions
      bool brokenInstalledDependencyVersionData = false;

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

        if (displayedInstalledVersion == "Not installed")
        {
          // set the install bubble to to not installed format
          dependencyContainer.AddToClassList("mandatoryDependencyNotInstalled");
          installStateSideBubble.AddToClassList("mandatoryDependencyNotInstalled");
          packageStateLabelHolder.AddToClassList("mandatoryDependencyNotInstalled");
          // set packageStateLabel to say that the dependency is not installed
          packageStateLabel.text = "Please Install";
        }
        else
        {
          // check if displayedInstalledVersion and displayedVersion are valid version strings
          bool targetHasVersion = checkIfValidVersion(displayedInstalledVersion);
          bool sourceHasVersion = checkIfValidVersion(displayedVersion);

          if (sourceHasVersion && targetHasVersion)
          {
            // both version strings are valid, so compare them

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

          if (!targetHasVersion)
          {
            // Target version string is not valid, set the install bubble to to Unversioned format
            dependencyContainer.AddToClassList("mismatchedDependencyUnversioned");
            installStateSideBubble.AddToClassList("mismatchedDependencyUnversioned");
            packageStateLabelHolder.AddToClassList("mismatchedDependencyUnversioned");
            // set the brokenDependencyVersionData bool to true
            brokenDependencyVersionData = true;
            // set packageStateLabel to say that the version strings are not valid
            packageStateLabel.text = "Un-know version installed";
          }

          if (!sourceHasVersion)
          {
            // this should never happen, implies that source config is broken,
            // set the install bubble to to unversioned format
            dependencyContainer.AddToClassList("mismatchedDependencyUnversioned");
            installStateSideBubble.AddToClassList("mismatchedDependencyUnversioned");
            packageStateLabelHolder.AddToClassList("mismatchedDependencyUnversioned");
            // set the brokenDependencyVersionData bool to true
            brokenInstalledDependencyVersionData = true;

            // Get the package info
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(
              "Packages/" + Constants.PACKAGE_NAME
            );

            // show popup message asking the user to report this error
            EditorUtility.DisplayDialog(
              "Error",
              "This should not have occurred, but it seems that the source version of "
                + dependency.Name
                + " is corrupted. Please report this error to the developer, via the \"Report Issue\" button, on the dependency. \n \n"
                + "Please include the following information in your report: \n"
                + "Installed Version: "
                + displayedInstalledVersion
                + "\n"
                + "Source Version: "
                + displayedVersion
                + "\n"
                + "Time of error: "
                + DateTime.Now.ToString("hh:mm:ss tt")
                + "\n"
                + "Date of error: "
                + DateTime.Now.ToString("dd/MM/yyyy")
                + "\n"
                + "Unity Version: "
                + Application.unityVersion
                + "\n"
                + "Package you are using: "
                + Constants.PROGRAM_DISPLAY_NAME
                + "\n"
                + "Its Version: "
                + packageInfo.version
                + "\n"
                + "Your OS: "
                + SystemInfo.operatingSystem
                + "\n",
              "OK"
            );
            // set packageStateLabel to say that the installed version is corrupted
            packageStateLabel.text = "Version Corrupted";
          }

          if (displayedVersion == displayedInstalledVersion)
          {
            // both version strings match, so the dependency is up to date
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
  }
}
#endif
