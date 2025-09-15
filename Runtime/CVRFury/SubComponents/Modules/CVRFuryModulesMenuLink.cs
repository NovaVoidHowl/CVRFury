using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;
using System.Reflection;

// following is provided by the VRC Stub dlls included in the CVRFury project
using VRC.SDK3.Avatars.ScriptableObjects;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [DoNotShowOnList] // needs to be reviewed, not sure if this is needed any more
  [Serializable]
  public class menuLink : CVRFuryModule
  {
    public menuLink()
      : base("MenuLink") { }

    public VRCExpressionsMenu menu;
  }
}
