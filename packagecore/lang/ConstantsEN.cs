using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  public static partial class Constants
  {
    public const string ERR_ARMATURELINK_NOLINKTARGETFOUND = "No linkTarget found";
    public const string ERR_ARMATURELINK_LINKTARGETS_EMPTY_NULL = "linkTargets is null or empty, cannot find any targets";
    public const string ERR_ARMATURELINK_BONETOLINKTOGAMEOBJECTTRANSFORM_NULL = "boneToLinkToGameObjectTransform is null, skipping to next target";
    public const string ERR_ARMATURELINK_TARGETGAMEOBJECT_NULL = "targetGameObject is null, skipping to next target";
    public const string ERR_ARMATURELINK_TARGETGAMEOBJECT_OFFSET_NULL = "Error finding targetGameObject with offset";
    public const string ERR_ARMATURELINK_TARGETAVATAR_NULL = "targetAvatar is null, cannot use Avatar Root";
    public const string ERR_ARMATURELINK_ANIMATOR_NULL = "No animator found on avatar, this should not happen. Please check your base avatar setup";
    public const string ERR_ARMATURELINK_ARMATURELINKMODULE_NULL = "No armature link module provided";
  }
}
