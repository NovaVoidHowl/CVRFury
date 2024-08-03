using System;
using UnityEngine.Scripting;

namespace uk.novavoidhowl.dev.cvrfury.packagecore
{
  /// <summary>
  ///   Use this attribute to rename a field without losing its serialized value.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
  public class WasSerializedAs : Attribute
  {
    // private copy of the old name of the field
    private string m_oldName;

    // public property to access the old name of the field
    public string oldName => m_oldName;

    public WasSerializedAs(string oldName)
    {
      m_oldName = oldName;
    }
  }
}