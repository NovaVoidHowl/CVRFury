using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [Serializable]
  public abstract class CVRFuryModule { }

  [Serializable]
  public class parametersLink : CVRFuryModule
  {
    public string name;
    public string description;
    public VRCExpressionParameters parameters;
  }

  [Serializable]
  public class menuLink : CVRFuryModule
  {
    public string name;
    public string description;
    public VRCExpressionsMenu menu;
  }
}
