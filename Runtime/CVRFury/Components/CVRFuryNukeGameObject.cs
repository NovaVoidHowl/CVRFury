using System;
using System.Collections.Generic;
using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  ///
  /// </summary>
  [DisallowMultipleComponent]
  [AddComponentMenu("")]
  /// <summary>
  /// This class is used to tag gameObjects for purge during the cleanup phase
  /// </summary>
  public class CVRFuryNukeGameObject : MonoBehaviour
  {
    // bool to enable the nuke (default is false, safeguard incase user adds this component to a gameobject by mistake)
    [HideInInspector]
    public bool nukeEnabled = false;
    [HideInInspector]
    public int DSUNumber = 0;
  }
}
