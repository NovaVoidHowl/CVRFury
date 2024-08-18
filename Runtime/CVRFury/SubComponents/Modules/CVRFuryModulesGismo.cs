using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public class CVRFuryGismo : CVRFuryModule
  {
    public CVRFuryGismo()
      : base("CVRFuryGismo") { }

    public enum typeOfIndicator
    {
      sphere = 0,
      cube = 1,
      cylinder = 2,
      cone = 3,
      pyramid = 4
    }

    public GameObject target;

    public Vector3 rotation;
    public string descriptionText;
    public float indicatorScale;
    public Color indicatorColor;
    public float arrowLength;
    public typeOfIndicator indicatorType;
  }
}
