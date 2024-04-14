using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_VRC = new ReadOnlyCollection<string>(
      new List<string>
      {
        "IsLocal",
        "Viseme",
        "Voice",
        "GestureLeft",
        "GestureRight",
        "GestureLeftWeight",
        "GestureRightWeight",
        "AngularY",
        "VelocityX",
        "VelocityY",
        "VelocityZ",
        "VelocityMagnitude",
        "Upright",
        "Grounded",
        "Seated",
        "AFK",
        "Expression1",
        "Expression2",
        "Expression3",
        "Expression4",
        "Expression5",
        "Expression6",
        "Expression7",
        "Expression8",
        "Expression9",
        "Expression10",
        "Expression11",
        "Expression12",
        "Expression13",
        "Expression14",
        "Expression15",
        "Expression16",
        "TrackingType",
        "VRMode",
        "MuteSelf",
        "InStation",
        "Earmuffs",
        "IsOnFriendsList"
      }
    );
    // ref https://creators.vrchat.com/avatars/animator-parameters/

  }
}
