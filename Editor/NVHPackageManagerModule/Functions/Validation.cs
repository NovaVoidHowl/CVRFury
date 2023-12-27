using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;

namespace uk.novavoidhowl.dev.nvhpmm
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
  }
}
