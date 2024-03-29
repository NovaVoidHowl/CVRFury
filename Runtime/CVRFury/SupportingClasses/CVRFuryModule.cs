using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.vrcstub;

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
  }

  [Serializable]
  public class fullControllerModule : CVRFuryModule
  {
    public fullControllerModule()
      : base("fullControllerModule") { }

    public List<RuntimeAnimatorController> controllers = new List<RuntimeAnimatorController>();
    public List<MenuEntry> menus = new List<MenuEntry>();
    public List<ParamsEntry> parameters = new List<ParamsEntry>();
    public List<SmoothParamEntry> smoothedPrms = new List<SmoothParamEntry>();
    public List<string> globalParams = new List<string>();

    public string toggleParam;

    public List<BindingRewrite> rewriteBindings = new List<BindingRewrite>();

    [Serializable]
    public class MenuEntry
    {
      public CVRFuryMenuStore menu;
      public string prefix;
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

    public string bonePathOnTargetArmature; // if not empty, will use this bone path on the avatar to link to instead of the armatureBoneToLinkTo
    public HumanBodyBones armatureBoneToLinkTo;
    public List<HumanBodyBones> fallbackBones = new List<HumanBodyBones>();

    public string boneSuffixToStrip; // if not empty, will strip this suffix from the bones of the addon object
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

  //------------------------------------------------- Supporting structs -------------------------------------------------

  /// <summary>
  /// A struct to hold a pair of an object and a state to set it to.
  /// </summary>
  [Serializable]
  public struct objectStatePair
  {
    public enum objectState
    {
      disabled,
      enabled,
    }

    public objectState stateToSet;
    public GameObject objectToSetStateOn;
  }
}
