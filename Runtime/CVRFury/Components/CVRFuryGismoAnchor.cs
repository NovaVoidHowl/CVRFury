using UnityEngine;
using uk.novavoidhowl.dev.cvrfury.supporting_classes.runtime;
using uk.novavoidhowl.dev.cvrfury.packagecore;

namespace uk.novavoidhowl.dev.cvrfury.runtime
{
  [AddComponentMenu("CVRFury/Gismo Anchor")]
  [DisallowMultipleComponent]
  public class CVRFuryGismoAnchor : MonoBehaviour
  {
    public GameObject sourceDSUGameObject;
    public CVRFuryGismo moduleData;
  }
}
