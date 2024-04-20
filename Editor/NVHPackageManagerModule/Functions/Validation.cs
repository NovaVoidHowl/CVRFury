// editor only script to manage the dependencies
#if UNITY_EDITOR

using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

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
        CoreLogError("Failed to list packages: " + request.Error.message);
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

    /// <summary>
    /// Checks if the config file is present and valid
    /// </summary>
    /// <returns>
    /// tuple of bools + a visualElement + DepManagerConfig,
    /// first is if the file was found,
    /// second is if the file was parsed ok
    /// then the error container
    /// and finally the config variable
    /// </returns>
    public static (bool, bool, VisualElement, DepManagerConfig) DepManagerConfValidateAndLoad()
    {
      // error container to return if the config file is missing
      VisualElement errorContainer = new VisualElement();

      // default to ok, if we find an error we'll change this
      bool fileFound = true;
      bool FileParseOK = true;

      TextAsset json = Resources.Load<TextAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/Dependencies/DepManagerConf"
      );
      if (json == null)
      {
        // add the errorContainer to the root
        errorContainer.Add(
          renderCoreError(
            "Config file is missing\n\nPlease check the file 'Assets/Resources/Dependencies/DepManagerConf.json' exists in this package",
            "File not found: Assets/Resources/Dependencies/DepManagerConf.json"
          )
        );

        // set the fileFound flag to false
        fileFound = false;
      }

      DepManagerConfig nvhpmmConfig = null;
      if (fileFound)
      { // file was found, try to parse it
        try
        {
          nvhpmmConfig = JsonConvert.DeserializeObject<DepManagerConfig>(json.text);
        }
        catch (JsonReaderException ex)
        {
          CoreLogError("JsonReaderException: " + ex.Message);
        }

        // Check if the config was loaded
        if (nvhpmmConfig == null)
        {
          // add the errorContainer to the root
          errorContainer.Add(
            renderCoreError(
              "Config file corrupted/invalid\n\n"
                + "Please check the file 'Assets/Resources/Dependencies/DepManagerConf.json' in this package is valid json,\n"
                + "see console logs for details",
              "Failed to load config file at 'Assets/Resources/Dependencies/DepManagerConf.json'."
            )
          );

          // set the FileParseOK flag to false
          FileParseOK = false;
        }
      }
      return (fileFound, FileParseOK, errorContainer, nvhpmmConfig);
    }
  }
}
#endif
