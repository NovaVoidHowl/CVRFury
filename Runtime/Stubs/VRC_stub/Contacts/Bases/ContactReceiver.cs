// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using UnityEngine;
using System.Collections.Generic;

namespace uk.novavoidhowl.dev.vrcstub
{

  [AddComponentMenu("")]
  public class ContactReceiver : ContactBase
  {
    public enum ReceiverType
    {
      Constant,
      OnEnter,
      Proximity
    }

    public bool allowSelf = true;

    public bool allowOthers = true;

    public bool localOnly;

    public ReceiverType receiverType;

    public string parameter;

    public float minVelocity = 0.05f;

    public override bool IsReceiver()
    {
      return true;
    }

  }

}

#endif
