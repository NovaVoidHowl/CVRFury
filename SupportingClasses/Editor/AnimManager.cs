//this whole file is editor only
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.supporting_classes.editor
{
  public static class Animation
  {
    public static AnimationClip rewriteAnimationClipCurvePaths(AnimationClip clip, string oldPathString, string newPathString)
    {
      //console print to say we are rewriting animation clip curve paths
      CoreLog("Rewriting animation clip curve paths");
      // console print the old and new path strings along with the clip's name
      CoreLog("clip.name: " + clip.name);

      // get the curve bindings from the clip
      EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
    

      // list of tuples to hold the old and new curve bindings
      List<System.Tuple<EditorCurveBinding, EditorCurveBinding>> curveBindingsPairs = new List<System.Tuple<EditorCurveBinding, EditorCurveBinding>>();
    
      //console print the curve bindings count
      CoreLogDebug("Number of curveBindings: " + curveBindings.Length);
      
      // loop through the curve bindings
      foreach (EditorCurveBinding curveBinding in curveBindings)
      {
        // Debug console print the curve binding's path
        CoreLogDebug("current curveBinding path: " + curveBinding.path);
        // Debug print the old path string
        CoreLogDebug("oldPathString: " + oldPathString);
        // Debug print the new path string
        CoreLogDebug("newPathString: " + newPathString);

        // check if the curve binding's path contains the old path string
        if (curveBinding.path.Contains(oldPathString))
        {
          // generate a new path string by replacing the old path string with the new path string
          // in the curve binding's path
          string replacementFullPath = curveBinding.path.Replace(oldPathString, newPathString);

          // console print to say we are replacing the path for the curve binding
          CoreLogDebug("Replacing path "+ curveBinding.path +" with "+ replacementFullPath +" for curve binding");

          // if it does, create a new curve binding with the replaced path
          EditorCurveBinding newCurveBinding = new EditorCurveBinding
          {
            path = replacementFullPath,
            type = curveBinding.type,
            propertyName = curveBinding.propertyName
          };

          // add the old and new curve bindings to the list
          curveBindingsPairs.Add(new System.Tuple<EditorCurveBinding, EditorCurveBinding>(curveBinding, newCurveBinding));
    
        }
        else
        {
          // if it doesn't, add the original curve binding to the list
          curveBindingsPairs.Add(new System.Tuple<EditorCurveBinding, EditorCurveBinding>(curveBinding, curveBinding));
        }
      }

      // get the object reference curve bindings from the clip
      EditorCurveBinding[] objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
      
      // loop through the object reference curve bindings
      foreach (EditorCurveBinding objectReferenceCurveBinding in objectReferenceCurveBindings)
      {
        // check if the object reference curve binding's path contains the old path string
        if (objectReferenceCurveBinding.path.Contains(oldPathString))
        {
          // generate a new path string by replacing the old path string with the new path string
          // in the object reference curve binding's path
          string replacementFullPath = objectReferenceCurveBinding.path.Replace(oldPathString, newPathString);
      
          // create a new object reference curve binding with the replaced path
          EditorCurveBinding newObjectReferenceCurveBinding = new EditorCurveBinding
          {
            path = replacementFullPath,
            type = objectReferenceCurveBinding.type,
            propertyName = objectReferenceCurveBinding.propertyName
          };
      
          // get the object reference curve from the clip
          ObjectReferenceKeyframe[] curve = AnimationUtility.GetObjectReferenceCurve(clip, objectReferenceCurveBinding);
      
          // remove the old object reference curve binding
          AnimationUtility.SetObjectReferenceCurve(clip, objectReferenceCurveBinding, null);
      
          // set the curve to the clip with the new object reference curve binding
          AnimationUtility.SetObjectReferenceCurve(clip, newObjectReferenceCurveBinding, curve);
        }
      }

    
      //console print that we are setting the new curve bindings to the clip
      CoreLog("Setting the new curve bindings to the clip: "  + clip.name);

      // loop through the curve bindings pairs and set the new curve bindings to the clip
      foreach (System.Tuple<EditorCurveBinding, EditorCurveBinding> curveBindingsPair in curveBindingsPairs)
      {
        // get the curve from the clip
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBindingsPair.Item1);

        // remove the old curve binding
        AnimationUtility.SetEditorCurve(clip, curveBindingsPair.Item1, null);

        // set the curve to the clip with the new curve binding
        AnimationUtility.SetEditorCurve(clip, curveBindingsPair.Item2, curve);
      }

      // list all the curve paths in the clip now (should be the new paths) (TODO: remove this debug print)
      EditorCurveBinding[] newCurveBindings = AnimationUtility.GetCurveBindings(clip);
      CoreLogDebug("Number of newCurveBindings: " + newCurveBindings.Length);
      foreach (EditorCurveBinding curveBinding in newCurveBindings)
      {
        CoreLogDebug("newCurveBinding.path: " + curveBinding.path);
      }


      // return the clip
      return clip;
    }
  } 
}
#endif