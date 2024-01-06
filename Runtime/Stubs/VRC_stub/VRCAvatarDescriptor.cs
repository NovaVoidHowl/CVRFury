// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{
  /// <summary>
  /// Stub for VRCAvatarDescriptor
  /// This is a minimized implementation of the VRCAvatarDescriptor class only concerned with a restricted set of
  /// properties used by the VRCFury code base.
  ///
  /// named the same as the original class so that it can be used as a drop in replacement with VRCFury stubs
  /// in order to keep the code as similar as possible with the original VRCFury code base
  /// </summary>
  [AddComponentMenu("")] // hide from add component menu
  public class VRCAvatarDescriptor : MonoBehaviour
  {
    [Serializable]
    public enum AnimLayerType
    {
      Base,
      Deprecated0,
      Additive,
      Gesture,
      Action,
      FX,
      Sitting,
      TPose,
      IKPose
    }
  }
}

#endif
