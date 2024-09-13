// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  [Serializable]
  public struct VRCConstraintSource
  {
    public Transform SourceTransform;
    public float Weight;
    public Vector3 ParentPositionOffset;
    public Vector3 ParentRotationOffset;

    public VRCConstraintSource(Transform transform, float weight, Vector3 parentPositionOffset, Vector3 parentRotationOffset)
    {
      SourceTransform = transform;
      Weight = weight;
      ParentPositionOffset = parentPositionOffset;
      ParentRotationOffset = parentRotationOffset;
    }
  }
}

#endif
