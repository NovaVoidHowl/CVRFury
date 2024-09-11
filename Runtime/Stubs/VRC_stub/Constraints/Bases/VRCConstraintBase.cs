// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  [ExecuteInEditMode]
  public abstract class VRCConstraintBase : MonoBehaviour
  {
    public enum WorldUpType
    {
      SceneUp,
      ObjectUp,
      ObjectRotationUp,
      Vector,
      None
    }

    [Flags]
    public enum Axis
    {
      None = 0,
      X = 1,
      Y = 2,
      Z = 4,
      All = -1
    }

    public bool IsActive;

    public float GlobalWeight = 1f;

    public Transform TargetTransform;

    public bool SolveInLocalSpace;

    public bool FreezeToWorld;

    public bool RebakeOffsetsWhenUnfrozen;

    public bool Locked;

    public VRCConstraintSourceKeyableList Sources;

    protected abstract VRCConstraintPositionMode PositionMode { get; }

    protected abstract VRCConstraintRotationMode RotationMode { get; }

    protected abstract VRCConstraintScaleMode ScaleMode { get; }

    #region Private Methods
    
    private void OnEnable()
    {
      // added to force unity to add the enabled/disabled checkbox and thus the Enabled/m_Enabled attribute
    }

    private void OnDisable()
    {
      // added to force unity to add the enabled/disabled checkbox and thus the Enabled/m_Enabled attribute
    }

    #endregion // Private Methods
  }
}
	

#endif
