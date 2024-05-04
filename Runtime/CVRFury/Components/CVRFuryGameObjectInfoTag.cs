using System;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  /// <summary>
  /// this component is used to
  /// to store info about GameObjects that have been added to the base avatar via CVRFury
  /// </summary>
  [AddComponentMenu("")] // hide from add component menu
  public class CVRFuryGameObjectInfoTag : MonoBehaviour
  {
    public string sourcePrefabName;
    public int sourceDSUNumber;
  }
}
