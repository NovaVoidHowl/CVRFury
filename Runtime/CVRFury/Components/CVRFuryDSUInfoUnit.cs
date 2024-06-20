using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  ///
  /// </summary>
  [DisallowMultipleComponent]
  [AddComponentMenu("CVRFury/System/DSU Info Unit")]
  public class CVRFuryDSUInfoUnit : MonoBehaviour
  {
    // label to Say that the DIU has not correctly loaded and
    // to check the Tool Setup, this is a bit of a hack to let the user
    // know that something is wrong, without needing
    // unityEditor in the runtime
    [Header("CVRFury UI failed to load\n\nPlease check Tool Setup\n\n NVH>CVRFury>Tool Setup\n")]
    public bool somethingIsBroken = true;

    // data load failure bool
    public bool dataLoadFailed = true;

    // list of paths in the DSU
    [SerializeField]
    public List<DSUPath> DSUPaths = new List<DSUPath>();
  }

  [Serializable]
  public class DSUPath
  {
    public string originalPath;
    public string rewrittenPath;
    public pathType type;

    public enum pathType
    {
      ArmaturePath,
      ArmatureRootPath,
      ObjectPath
    }
  }
}
