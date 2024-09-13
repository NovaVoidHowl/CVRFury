// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.vrcstub
{
  [Serializable]
  public struct VRCConstraintSourceKeyableList
  {
    public const int MaxFlatLength = 16;

    [SerializeField]
    public VRCConstraintSource source0;

    [SerializeField]
    public VRCConstraintSource source1;

    [SerializeField]
    public VRCConstraintSource source2;

    [SerializeField]
    public VRCConstraintSource source3;

    [SerializeField]
    public VRCConstraintSource source4;

    [SerializeField]
    public VRCConstraintSource source5;

    [SerializeField]
    public VRCConstraintSource source6;

    [SerializeField]
    public VRCConstraintSource source7;

    [SerializeField]
    public VRCConstraintSource source8;

    [SerializeField]
    public VRCConstraintSource source9;

    [SerializeField]
    public VRCConstraintSource source10;

    [SerializeField]
    public VRCConstraintSource source11;

    [SerializeField]
    public VRCConstraintSource source12;

    [SerializeField]
    public VRCConstraintSource source13;

    [SerializeField]
    public VRCConstraintSource source14;

    [SerializeField]
    public VRCConstraintSource source15;

    [SerializeField]
    [HideInInspector]
    [NotKeyable]
    public int totalLength;

    [SerializeField]
    [NotKeyable]
    public List<VRCConstraintSource> overflowList;

    public VRCConstraintSourceKeyableList(
      VRCConstraintSource source0,
      VRCConstraintSource source1,
      VRCConstraintSource source2,
      VRCConstraintSource source3,
      VRCConstraintSource source4,
      VRCConstraintSource source5,
      VRCConstraintSource source6,
      VRCConstraintSource source7,
      VRCConstraintSource source8,
      VRCConstraintSource source9,
      VRCConstraintSource source10,
      VRCConstraintSource source11,
      VRCConstraintSource source12,
      VRCConstraintSource source13,
      VRCConstraintSource source14,
      VRCConstraintSource source15,
      int totalLength,
      List<VRCConstraintSource> overflowList)
    {
      this.source0 = source0;
      this.source1 = source1;
      this.source2 = source2;
      this.source3 = source3;
      this.source4 = source4;
      this.source5 = source5;
      this.source6 = source6;
      this.source7 = source7;
      this.source8 = source8;
      this.source9 = source9;
      this.source10 = source10;
      this.source11 = source11;
      this.source12 = source12;
      this.source13 = source13;
      this.source14 = source14;
      this.source15 = source15;
      this.totalLength = totalLength;
      this.overflowList = overflowList;
    }
  }
}

#endif
