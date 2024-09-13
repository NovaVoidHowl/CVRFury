// if for some reason the VRC SDK is in the project then disable this stub
#if !VRC_SDK_VRCSDK3

using System;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.vrcstub
{

  public abstract class ContactBase : MonoBehaviour
  {
    public enum ShapeType
    {
      Sphere,
      Capsule
    }

    public Transform rootTransform;

    public ShapeType shapeType;

    public float radius = 0.5f;

    public float height = 2f;

    public Vector3 position = Vector3.zero;

    public Quaternion rotation = Quaternion.identity;

    public List<string> collisionTags = new List<string>();

    public Vector3 axis => rotation * Vector3.up;

    #region Private Methods

    private void OnEnable()
    {
      // added to force unity to add the enabled/disabled checkbox and thus the Enabled/m_Enabled attribute
    }

    private void OnDisable()
    {
      // added to force unity to add the enabled/disabled checkbox and thus the Enabled/m_Enabled attribute
    }

    #endregion // Private Methods

    public abstract bool IsReceiver();

  }
}

#endif
