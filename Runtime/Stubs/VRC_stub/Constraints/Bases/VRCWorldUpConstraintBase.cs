// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  public abstract class VRCWorldUpConstraintBase : VRCConstraintBase
  {
    public Vector3 RotationAtRest = Vector3.zero;

    public Vector3 RotationOffset = Vector3.zero;

    public Transform WorldUpTransform;

    protected override VRCConstraintPositionMode PositionMode => VRCConstraintPositionMode.None;

    protected override VRCConstraintScaleMode ScaleMode => VRCConstraintScaleMode.None;

    protected virtual bool UsesWorldUpTransform => false;
  }
	
}
#endif
