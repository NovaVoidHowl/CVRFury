// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{

  [AddComponentMenu("")]
  public class VRCAimConstraintBase : VRCWorldUpConstraintBase
  {
    public bool AffectsRotationX = true;

    public bool AffectsRotationY = true;

    public bool AffectsRotationZ = true;

    public Vector3 AimAxis = Vector3.forward;

    public Vector3 UpAxis = Vector3.up;

    public WorldUpType WorldUp;

    public Vector3 WorldUpVector = Vector3.up;

    protected override VRCConstraintPositionMode PositionMode => VRCConstraintPositionMode.None;

    protected override VRCConstraintRotationMode RotationMode => VRCConstraintRotationMode.AimTowardsPosition;

    protected override VRCConstraintScaleMode ScaleMode => VRCConstraintScaleMode.None;

    protected override bool UsesWorldUpTransform
    {
      get
      {
        if (WorldUp != WorldUpType.ObjectUp)
        {
          return WorldUp == WorldUpType.ObjectRotationUp;
        }
        return true;
      }
    }
  }


}

#endif
