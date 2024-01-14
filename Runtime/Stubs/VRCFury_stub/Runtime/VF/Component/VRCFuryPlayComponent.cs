///
/// this is an empty stub file for VRCFuryPlayComponent.cs
/// it's used to prevent class missing errors
///

using System;
using UnityEngine;

namespace VF.Component
{
  [AddComponentMenu("")] // hide from unity components menu
  public class VRCFuryPlayComponent : MonoBehaviour { }

  public class VRCFurySocketGizmo : VRCFuryPlayComponent { }

  public class VRCFuryNoUpdateWhenOffscreen : VRCFuryPlayComponent { }
}
