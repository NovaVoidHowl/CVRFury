// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{

  [AddComponentMenu("")]
  public class VRCScaleConstraintBase : VRCConstraintBase
  {
    public Vector3 ScaleAtRest = Vector3.one;

    public Vector3 ScaleOffset = Vector3.one;

    public bool AffectsScaleX = true;

    public bool AffectsScaleY = true;

    public bool AffectsScaleZ = true;

    protected override VRCConstraintPositionMode PositionMode => VRCConstraintPositionMode.None;

    protected override VRCConstraintRotationMode RotationMode => VRCConstraintRotationMode.None;

    protected override VRCConstraintScaleMode ScaleMode => VRCConstraintScaleMode.MatchScale;

  }
}
#endif
