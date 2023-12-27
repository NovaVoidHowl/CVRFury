using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace uk.novavoidhowl.dev.nvhpmm
{
  [InitializeOnLoad]
  public class ThirdPartyDependenciesPackages
  {
    static ThirdPartyDependenciesPackages()
    {
      EditorApplication.delayCall += refreshThirdPartyDependencies;
    }

    public static void refreshThirdPartyDependencies()
    {
      EditorApplication.delayCall -= refreshThirdPartyDependencies;

      TextAsset jsonFile = Resources.Load<TextAsset>("Dependencies/ThirdPartyDependencies");

      if (jsonFile == null)
      {
        Debug.LogError("File not found: Assets/Resources/Dependencies/ThirdPartyDependencies.json");
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
}
