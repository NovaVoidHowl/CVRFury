// editor only script to manage the dependencies
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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

      TextAsset jsonFile = Resources.Load<TextAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/Dependencies/PrimaryDependencies"
      );

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
#endif
