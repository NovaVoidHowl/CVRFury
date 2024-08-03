using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public static readonly ReadOnlyCollection<string> ALWAYS_GLOBAL_PARAMETERS_FROM_CVR = new ReadOnlyCollection<string>(
      new List<string>
      {
        "MovementX",
        "MovementY",
        "Grounded",
        "Emote",
        "CancelEmote",
        "GestureLeft",
        "GestureRight",
        "Toggle",
        "Sitting",
        "Crouching",
        "Prone",
        "Flying",
        "Swimming"
      }
    );

    public static readonly ReadOnlyCollection<string> CVR_PROTECTED_LAYERS = new ReadOnlyCollection<string>(
      new List<string>
      {
        "Locomotion/Emotes",
        "LeftHand",
        "RightHand",
        "Toggles"
      }
    );

  }
}
