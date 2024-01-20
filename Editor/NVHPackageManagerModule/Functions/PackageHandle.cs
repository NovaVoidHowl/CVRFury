// editor only script to manage the dependencies
#if UNITY_EDITOR

using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace uk.novavoidhowl.dev.cvrfury.nvhpmm
{
  public partial class ToolSetup : EditorWindow
  {
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
  }
}
#endif
