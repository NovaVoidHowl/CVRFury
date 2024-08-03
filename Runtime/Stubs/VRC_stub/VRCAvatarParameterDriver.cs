// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{

  /// <summary>
  /// Stub for VRCAvatarParameterDriver
  /// This is a minimized implementation of the VRCAvatarParameterDriver class only concerned with data extraction
  /// use by the VRCFury code base.
  ///
  /// named the same as the original class so that it can be used as a drop in replacement
  /// </summary>
  public class VRCAvatarParameterDriver : StateMachineBehaviour
  {
    public enum ChangeType
    {
      Set,
      Add,
      Random,
      Copy
    }

    [Serializable]
    public class Parameter
    {
      public ChangeType type;
      public string name;
      public string source;
      public float value;
      public float valueMin;
      public float valueMax = 1f;
      [Range(0f, 1f)]
      public float chance = 1f;
      public bool convertRange;
      public float sourceMin;
      public float sourceMax;
      public float destMin;
      public float destMax;
      public object sourceParam;
      public object destParam;
    }
    public List<Parameter> parameters = new List<Parameter>();
    public bool localOnly;
    public string debugString;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      // dummy override code to force unity to allow this script to be added to an animator state
      // we only need this for the data it holds, we don't need it to actually do anything
    }

  }

}
#endif