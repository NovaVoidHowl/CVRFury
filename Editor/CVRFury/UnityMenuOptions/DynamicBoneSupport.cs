#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.editor.menuoptions
{
  public class DynamicBoneSupport
  {
    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Integrations/Check Dynamic Bone Support", false, 0)]
    private static void AddDynamicBoneSupport()
    {
      // Check if Dynamic Bone is installed (path in DynamicBone/Scripts/DynamicBone.cs)
      string path = "Assets/DynamicBone/Scripts/DynamicBone.cs";

      // Get the current scripting define symbols for the selected build target group
      BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
      string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

      // Check if the file exists
      if (AssetDatabase.LoadAssetAtPath(path, typeof(Object)) != null)
      {
        CoreLog("Dynamic Bone is installed");

        // Check if the scripting define symbol DYNAMICBONE_PACKAGE_EXISTS exists
        if (!defineSymbols.Contains("DYNAMICBONE_PACKAGE_EXISTS"))
        {
          // Add scripting define symbol DYNAMICBONE_PACKAGE_EXISTS
          defineSymbols += ";DYNAMICBONE_PACKAGE_EXISTS";
          PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);
        }
      }
      else
      {
        CoreLog("Dynamic Bone is not installed");

        // Check if the scripting define symbol DYNAMICBONE_PACKAGE_EXISTS exists
        if (defineSymbols.Contains("DYNAMICBONE_PACKAGE_EXISTS"))
        {
          // Remove scripting define symbol DYNAMICBONE_PACKAGE_EXISTS
          defineSymbols = defineSymbols.Replace("DYNAMICBONE_PACKAGE_EXISTS", string.Empty);
          PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);
        }
      }
    }
  }
}
#endif