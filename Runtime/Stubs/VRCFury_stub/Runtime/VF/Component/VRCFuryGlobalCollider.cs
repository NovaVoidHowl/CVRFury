///
/// Retained for Data Loss Prevention
///

using UnityEngine;

namespace VF.Component
{
  [AddComponentMenu("")] // hide from unity components menu
  public class VRCFuryGlobalCollider : VRCFuryComponent
  {
    public float radius = 0.1f;
    public float height = 0;
    public Transform rootTransform;

    public Transform GetTransform()
    {
      return rootTransform != null ? rootTransform : transform;
    }
  }
}
