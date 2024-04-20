using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  ///
  /// </summary>
  [AddComponentMenu("CVRFury Data Storage Unit")]
  public class CVRFuryDataStorageUnit : MonoBehaviour
  {
    // label to Say that the DSU has not correctly loaded and
    // to check the Tool Setup, this is a bit of a hack to let the user
    // know that something is wrong, without needing
    // unityEditor in the runtime
    [Header("CVRFury UI failed to load\n\nPlease check Tool Setup\n\n NVH>CVRFury>Tool Setup\n")]
    public bool somethingIsBroken = true;

    public CVRFuryModuleInfo moduleInfo;
    public CVRFuryModules modules;

    public string VRCFuryPrefabName;
    public string CVRFuryPrefabName;

    // getter to get the VRCFuryPrefabName or the CVRFuryPrefabName depending on which is set
    public string prefabName
    {
      get
      {
        // if the VRCFuryPrefabName is set, return it
        if (!string.IsNullOrEmpty(VRCFuryPrefabName))
        {
          return VRCFuryPrefabName;
        }
        // if the CVRFuryPrefabName is set, return it
        else if (!string.IsNullOrEmpty(CVRFuryPrefabName))
        {
          return CVRFuryPrefabName;
        }
        // if neither are set, return an null string
        else
        {
          return null;
        }
      }
    }

  }

  [Serializable]
  public class CVRFuryModuleInfo
  {
    public string name;
    public CVRFurySemVer version;
  }

  [Serializable]
  public class CVRFuryModules
  {
    [SerializeReference]
    public List<CVRFuryModule> modules = new List<CVRFuryModule>();
  }
}
