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
  public class armatureLink : CVRFuryModuleLinker
  {
    public armatureLink()
      : base("ArmatureLink") { }

    public string boneSuffixToStrip; // if not empty, will strip this suffix from the bones of the addon object
    
    // added to allow for more targeting options
    public List<LinkTarget> linkTargets = new List<LinkTarget>();

    public AvatarLinkTargetingMode avatarTargetingMode = AvatarLinkTargetingMode.basic;

    // basic mode vars
    public string bonePathOnTargetArmature; // if not empty, will use this bone path on the avatar to link to instead of the armatureBoneToLinkTo
    public HumanBodyBones armatureBoneToLinkTo;
    public List<HumanBodyBones> fallbackBones = new List<HumanBodyBones>();
    
    // end basic mode vars
   
  }
}
