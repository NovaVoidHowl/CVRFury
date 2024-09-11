// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  [AddComponentMenu("")]
  public class VRCPositionConstraintBase : VRCConstraintBase
  {
    public Vector3 PositionAtRest = Vector3.zero;

    public Vector3 PositionOffset = Vector3.zero;

    public bool AffectsPositionX = true;

    public bool AffectsPositionY = true;

    public bool AffectsPositionZ = true;

    protected override VRCConstraintPositionMode PositionMode => VRCConstraintPositionMode.MatchPosition;

    protected override VRCConstraintRotationMode RotationMode => VRCConstraintRotationMode.None;

    protected override VRCConstraintScaleMode ScaleMode => VRCConstraintScaleMode.None;

  }
}
#endif