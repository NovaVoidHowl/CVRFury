// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{
  /// <summary>
  /// Stub for VRCExpressionParameters
  /// This is a minimal implementation of the VRCExpressionParameters class only concerned with the parameters
  ///
  /// named the same as the original class so that it can be used as a drop in replacement with VRCFury stubs
  /// in order to keep the code as similar as possible with the original VRCFury code base
  /// </summary>
  public class VRCExpressionParameters : ScriptableObject
  {
    [Serializable]
    public class Parameter
    {
      public string name;

      public ValueType valueType;

      public bool saved = true;

      public float defaultValue;

      public bool networkSynced = true;
    }

    public enum ValueType
    {
      Int,
      Float,
      Bool
    }

    public Parameter[] parameters;
  }
}

#endif
