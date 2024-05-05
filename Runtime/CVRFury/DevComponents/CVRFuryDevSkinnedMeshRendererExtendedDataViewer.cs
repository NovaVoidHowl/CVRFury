using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// this component is used to, show the extended data of a SkinnedMeshRenderer in the inspector
  /// as the SkinnedMeshRenderer component does not show the extended data in the inspector (even in debug mode)
  /// </summary>
  [AddComponentMenu("CVRFury/Dev/SkinnedMeshRenderer Extended View")]
  public class CVRFuryDevSkinnedMeshRendererExtendedDataViewer : MonoBehaviour
  {
    public bool foldoutState = false;
    
  }
}
