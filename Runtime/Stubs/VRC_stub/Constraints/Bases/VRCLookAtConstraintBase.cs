// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  [AddComponentMenu("")]
  public class VRCLookAtConstraintBase : VRCWorldUpConstraintBase
  {
    public float Roll;

    public bool UseUpTransform;

    protected override VRCConstraintPositionMode PositionMode => VRCConstraintPositionMode.None;

    protected override VRCConstraintRotationMode RotationMode => VRCConstraintRotationMode.LookAtPosition;

    protected override VRCConstraintScaleMode ScaleMode => VRCConstraintScaleMode.None;

    protected override bool UsesWorldUpTransform => UseUpTransform;

 }
}
#endif
