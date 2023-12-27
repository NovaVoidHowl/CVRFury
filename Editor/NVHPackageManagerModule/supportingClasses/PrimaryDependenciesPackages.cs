using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace uk.novavoidhowl.dev.nvhpmm
{
  [InitializeOnLoad]
  public class PrimaryDependenciesPackages
  {
    static PrimaryDependenciesPackages()
    {
      EditorApplication.delayCall += refreshPrimaryDependencies;
    }

    public static void refreshPrimaryDependencies()
    {
      EditorApplication.delayCall -= refreshPrimaryDependencies;

      TextAsset jsonFile = Resources.Load<TextAsset>("Dependencies/PrimaryDependencies");

      if (jsonFile == null)
      {
        Debug.LogError("File not found: Assets/Resources/Dependencies/PrimaryDependencies.json");
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
}
