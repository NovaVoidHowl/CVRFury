///
/// Retained for Data Loss Prevention
///

using System;
using System.Collections.Generic;
using UnityEngine;
using VF.Model;

namespace VF.Component
{
  [AddComponentMenu("")] // hide from unity components menu
  public class VRCFuryHapticPlug : VRCFuryComponent
  {
    public bool autoRenderer = true;
    public bool autoPosition = true;
    public bool autoLength = true;
    public bool useBoneMask = true;
    public GuidTexture2d textureMask = null;
    public float length;
    public bool autoRadius = true;
    public float radius;
    public new string name;
    public bool unitsInMeters = false;
    public bool configureTps = false;
    public bool enableSps = true;

    [NonSerialized]
    public bool sendersOnly = false;
    public bool spsAutorig = true;
    public List<string> spsBlendshapes = new List<string>();
    public List<Renderer> configureTpsMesh = new List<Renderer>();
    public float spsAnimatedEnabled = 1;
    public bool useLegacyRendererFinder = false;
    public bool addDpsTipLight = false;
    public bool spsKeepImports = false;

    public State postBakeActions;
    public bool spsOverrun = true;
    public bool enableDepthAnimations = false;
    public List<PlugDepthAction> depthActions = new List<PlugDepthAction>();
    public bool useHipAvoidance = true;

    [Obsolete]
    public bool configureSps = false;

    [Obsolete]
    public bool spsBoneMask = true;

    [Obsolete]
    public GuidTexture2d spsTextureMask = null;

    [Obsolete]
    public GuidTexture2d configureTpsMask = null;

    [Serializable]
    public class PlugDepthAction
    {
      public State state;
      public float startDistance = 1;
      public float endDistance;
      public bool enableSelf;
      public float smoothingSeconds = 0.25f;

      [Obsolete]
      public float smoothing;
      public bool ResetMePlease2;
    }

    public override int GetLatestVersion()
    {
      return 9;
    }
  }
}
