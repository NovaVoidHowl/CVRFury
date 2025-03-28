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
  [RequireComponent(typeof(CVRFuryAvatarConfiguration))]
  [AddComponentMenu("CVRFury/System/Avatar Collider Info Unit")]
  public class CVRFuryAvatarColliderInfoUnit : MonoBehaviour
  {
    // label to Say that the AIU has not correctly loaded and
    // to check the Tool Setup, this is a bit of a hack to let the user
    // know that something is wrong, without needing
    // unityEditor in the runtime
    [Header("CVRFury UI failed to load\n\nPlease check Tool Setup\n\n NVH>CVRFury>Tool Setup\n")]
    public bool somethingIsBroken = true;

    // data load failure bool
    public bool dataLoadFailed = false;

    // list of colliders for the avatar bones (supported in VRC data sets)
    public ColliderConfig collider_fingerLittleR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerRingR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerMiddleR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerIndexR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerLittleL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerRingL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerMiddleL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_fingerIndexL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_handL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_handR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_footL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_footR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_torso = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_head = new ColliderConfig { state = ColliderConfig.State.Disabled };

    // extra colliders for the avatar (supported in CVRFury data sets)
    public ColliderConfig collider_hips = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_upperLegL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_upperLegR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_lowerLegL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_lowerLegR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_upperArmL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_upperArmR = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_lowerArmL = new ColliderConfig { state = ColliderConfig.State.Disabled };
    public ColliderConfig collider_lowerArmR = new ColliderConfig { state = ColliderConfig.State.Disabled };

    [System.Serializable]
    public struct ColliderConfig
    {
      public bool isMirrored;
      public State state;
      public Transform transform;
      public float radius;
      public float height;
      public Vector3 position;
      public Quaternion rotation;
      public Vector3 axis;

      public enum State
      {
        Automatic = 0,
        Custom = 1,
        Disabled = 2
      }
    }
  }
}
