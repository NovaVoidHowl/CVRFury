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
  public class fullControllerModule : CVRFuryModule
  {
    public fullControllerModule()
      : base("fullControllerModule") { }

    public bool verbatimMode = false;

    // if true all animation controllers in this module will have their layers copied as is no rewrites etc
    // note variable names will not be rewritten in this mode
    public List<RuntimeAnimatorController> controllers;
    public List<MenuEntry> menus;
    public List<ParamsEntry> parameters;
    public List<SmoothParamEntry> smoothedPrms;
    public List<string> globalParams;

    public string toggleParam;

    public List<BindingRewrite> rewriteBindings;

    [Serializable]
    public class MenuEntry
    {
      public CVRFuryMenuStore menu;
      public string prefix;

      // var to allow overriding the string format of the prefix ( \ to / )
      // only use this if you know what you are doing
      public bool forcePrefixStringFormat = false;
    }

    [Serializable]
    public class ParamsEntry
    {
      public CVRFuryParametersStore parameters;
    }

    [Serializable]
    public class BindingRewrite
    {
      public string from;
      public string to;
      public bool delete = false;
    }

    [Serializable]
    public class SmoothParamEntry
    {
      public string name;
      public float smoothingDuration = 0.2f;
    }
  }
}
