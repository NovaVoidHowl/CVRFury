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
  public abstract class CVRFuryModule
  {
    [SerializeField]
    private string moduleType;

    public string ModuleType
    {
      get { return moduleType; }
    }

    protected CVRFuryModule(string moduleType)
    {
      this.moduleType = moduleType;
    }

    public CVRFurySemVer CVRFuryVersion;
    public CVRFurySemVer unityVersion;
    public CVRFurySemVer CVRCCKVersion;

    public string name;
    public string description;

    public bool importedFromVRCFury;
    public bool justImported;

    public GameObject moduleFoundOnObject;
  }

  [Serializable]
  public abstract class CVRFuryModuleLinker : CVRFuryModule
  {
    protected CVRFuryModuleLinker(string moduleType)
      : base(moduleType) { }

    public enum linkMode
    {
      MeshRebind,
      MergeAsChildren,
      ParentConstraint,
      ReparentRoot,
      Auto,
    }

    public enum KeepBoneOffsets
    {
      Auto,
      Yes,
      No
    }

    public linkMode addonLinkMode;
    public GameObject addonObjectToLink;
    public KeepBoneOffsets keepBoneOffsets;
    public float meshRewriteScalingFactor = 0;
    public bool scalingFactorPowersOf10Only = true;
  }

  [Serializable]
  public abstract class StateSetter : CVRFuryModule
  {
    public StateSetter(string moduleType)
      : base(moduleType) { }
  }

  #region supporting classes/enums
  [Serializable]
  public class LinkTarget
  {
    // options of what to link to
    public LinkTargetType linkType = LinkTargetType.useHumanBodyBones;

    // targets
    public HumanBodyBones humanBodyBone = HumanBodyBones.Hips;
    
    public GameObject targetGameObject = null;
    
    // path addons for after the target
    public string offset = "";

  }

  [Serializable]
  public enum LinkTargetType
  {
    useHumanBodyBones = 0,
    useGameObject = 1,
    useAvatarRoot = 2
  }

  [Serializable]
  public enum AvatarLinkTargetingMode
  {
    basic = 0, // basic mode, binds to human body bones only and does not use the LinkTarget list
    order = 1, // uses the LinkTarget list first valid target will be used
    proximity = 2 // uses the LinkTarget list and will use the closest target to the addon object
  }


  #endregion // supporting classes/enums

}
