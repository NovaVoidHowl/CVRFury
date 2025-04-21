using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// this component is to allow setting of options based on the following:
  /// - DirectTreeOptimizer
  /// - BlendShapeOptimizer
  /// - Blinking
  /// - MMD Compatibility
  /// - Unlimited Parameters (don't think this will be needed as CVR already allows a lot of parameters)
  /// </summary>
  [DisallowMultipleComponent]
  [AddComponentMenu("CVRFury/System/Avatar Configuration")]
  public class CVRFuryAvatarConfiguration : MonoBehaviour
  {
    // label to Say that the AIU has not correctly loaded and
    // to check the Tool Setup, this is a bit of a hack to let the user
    // know that something is wrong, without needing
    // unityEditor in the runtime
    [Header("CVRFury UI failed to load\n\nPlease check Tool Setup\n\n NVH>CVRFury>Tool Setup\n")]
    public bool somethingIsBroken = true;

    // data load failure bool
    public bool dataLoadFailed = false;

    // bools to enable/disable VRCFury related per-avatar features
    [SerializeField]
    public bool enableDirectTreeOptimiser = false;

    [SerializeField]
    public bool enableBlendShapeOptimiser = false;

    [SerializeField]
    public bool enableBlinking = false;

    [SerializeField]
    public bool enableMMDCompatibility = false;

    [SerializeField]
    public bool enableUnlimitedParameters = false;

    // bools to enable/disable the use of collider adder
    [SerializeField]
    public bool enableMagica1Colliders = false;

    [SerializeField]
    public bool enableMagica2Colliders = false;

    [SerializeField]
    public bool enableDynamicBoneColliders = false;
  }
}
