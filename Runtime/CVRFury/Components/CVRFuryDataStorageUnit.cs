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
  }

  [Serializable]
  public class CVRFuryModuleInfo
  {
    public string name;
    public SemVer version;
  }

  [Serializable]
  public class CVRFuryModules
  {
    [SerializeReference]
    public List<CVRFuryModule> modules = new List<CVRFuryModule>();
  }
}
