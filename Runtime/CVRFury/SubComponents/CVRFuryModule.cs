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
  public class fullControllerModule : CVRFuryModule
  {
    public fullControllerModule()
      : base("fullControllerModule") { }

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

  [Serializable]
  public class parametersLink : CVRFuryModule
  {
    public parametersLink()
      : base("ParametersLink") { }

    public VRCExpressionParameters parameters;
  }

  [Serializable]
  public class menuLink : CVRFuryModule
  {
    public menuLink()
      : base("MenuLink") { }

    public VRCExpressionsMenu menu;
  }

  [Serializable]
  public class showInFirstPerson : CVRFuryModule
  {
    public showInFirstPerson()
      : base("ShowInFirstPerson") { }

    public GameObject target;
    public bool show;
    public bool bindToHead = false; // this is so that when imported from VRCFury it will trigger binding to the head
                                    // by default (normal add via CVRFury will not bind to the head)
  }

  [Serializable]
  public class deleteOnUpload : CVRFuryModule
  {
    public deleteOnUpload()
      : base("DeleteOnUpload") { }

    public GameObject target;
  }

  [Serializable]
  public class parameterStreamLink : CVRFuryModule
  {
    public parameterStreamLink()
      : base("parameterStreamLink") { }

    public List<CVRFuryParameterStreamEntry> entries = new();
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
  public class customLink : CVRFuryModuleLinker
  {
    public customLink()
      : base("CustomLink") { }

    public GameObject targetArmatureObject;
  }

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

  [Serializable]
  public abstract class StateSetter : CVRFuryModule
  {
    public StateSetter(string moduleType)
      : base(moduleType) { }
  }

  [Serializable]
  public class objectStateSetter : StateSetter
  {
    public objectStateSetter()
      : base("objectStateSetter") { }

    public objectStatePair[] objectStatePairs;
  }

  [Serializable]
  public class objectDefaultMaterialSetter : StateSetter
  {
    public objectDefaultMaterialSetter()
      : base("objectDefaultMaterialSetter") { }

    // material to set as default
    public Material defaultMaterial;

    // renderer to set the default material on
    public Renderer renderer;

    // renderer index to set the default material on
    public int rendererIndex;
  }

  [Serializable]
  public class objectDefaultBlendShapeSetter : StateSetter
  {
    public objectDefaultBlendShapeSetter()
      : base("objectDefaultBlendShapeSetter") { }

    public string blendShape;
    public float blendShapeValue = 100;
    public Renderer renderer;
    public bool allRenderers = true;
  }

  [Serializable]
  public class objectFxFloatSetter : StateSetter
  {
    public objectFxFloatSetter()
      : base("objectFxFloatSetter") { }

    public string fxFloatName;
    public float fxFloatValue;
  }

  [Serializable]
  public class objectScaleSetter : StateSetter
  {
    public objectScaleSetter()
      : base("objectScaleSetter") { }

    public GameObject objectToSetScaleOn;
    public float scaleToBeSet;
  }

  [Serializable]
  public class blockBlinkSetter : StateSetter
  {
    public blockBlinkSetter()
      : base("blockBlinkSetter") { }

    public bool blockBlink;
  }

  [Serializable]
  public class blockVisemesSetter : StateSetter
  {
    public blockVisemesSetter()
      : base("blockVisemesSetter") { }

    public bool blockVisemes;
  }

  [Serializable]
  public class CVRFuryPrefabDependency : CVRFuryModule
  {
    public CVRFuryPrefabDependency()
      : base("PrefabDependency") { }

    public string version;
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
