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
  [AddComponentMenu("CVRFury/Avatar Info Unit")]
  public class CVRFuryAvatarInfoUnit : MonoBehaviour
  {
    // label to Say that the AIU has not correctly loaded and
    // to check the Tool Setup, this is a bit of a hack to let the user
    // know that something is wrong, without needing
    // unityEditor in the runtime
    [Header("CVRFury UI failed to load\n\nPlease check Tool Setup\n\n NVH>CVRFury>Tool Setup\n")]
    public bool somethingIsBroken = true;

    // data load failure bool
    public bool dataLoadFailed = false;

    // list of paths in the avatar armature
    public List<string> avatarArmaturePaths = new List<string>();

    // list of other paths in the avatar (ie mesh, etc)
    public List<string> avatarOtherPaths = new List<string>();
  }
}
