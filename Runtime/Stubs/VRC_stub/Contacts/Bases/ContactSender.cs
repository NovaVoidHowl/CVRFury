#if !VRC_SDK_VRCSDK3

using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{

  [AddComponentMenu("")]
  public class ContactSender : ContactBase
  {
    public override bool IsReceiver()
    {
      return false;
    }
  }
}

#endif
