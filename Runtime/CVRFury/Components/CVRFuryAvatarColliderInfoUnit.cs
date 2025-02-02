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
    public ColliderConfig collider_fingerLittleR;
    public ColliderConfig collider_fingerRingR;
    public ColliderConfig collider_fingerMiddleR;
    public ColliderConfig collider_fingerIndexR;
    public ColliderConfig collider_fingerLittleL;
    public ColliderConfig collider_fingerRingL;
    public ColliderConfig collider_fingerMiddleL;
    public ColliderConfig collider_fingerIndexL;
    public ColliderConfig collider_handL;
    public ColliderConfig collider_handR;
    public ColliderConfig collider_footL;
    public ColliderConfig collider_footR;
    public ColliderConfig collider_torso;
    public ColliderConfig collider_head;

    // extra colliders for the avatar (supported in CVRFury data sets)
    public ColliderConfig collider_hips;
    public ColliderConfig collider_upperLegL;
    public ColliderConfig collider_upperLegR;
    public ColliderConfig collider_lowerLegL;
    public ColliderConfig collider_lowerLegR;
    public ColliderConfig collider_upperArmL;
    public ColliderConfig collider_upperArmR;
    public ColliderConfig collider_lowerArmL;
    public ColliderConfig collider_lowerArmR;

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

      public Vector3 axis { get; }

      public enum State
      {
        Automatic = 0,
        Custom = 1,
        Disabled = 2
      }
    }
  }
}
