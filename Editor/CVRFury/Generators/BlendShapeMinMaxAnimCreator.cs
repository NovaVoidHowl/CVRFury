#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class BlendShapeMinMaxAnimCreator
  {
    public static bool CreateAnimations(
      GameObject rootGameObject,
      SkinnedMeshRenderer meshRenderer,
      string blendShapeName,
      string animationName,
      string animationPath,
      out string errorMessage
    )
    {
      errorMessage = string.Empty;

      // Validate inputs
      if (rootGameObject == null)
      {
        errorMessage = "Root GameObject is null";
        return false;
      }

      if (meshRenderer == null)
      {
        errorMessage = "Mesh Renderer is null";
        return false;
      }

      if (string.IsNullOrEmpty(blendShapeName))
      {
        errorMessage = "BlendShape Name is empty";
        return false;
      }

      if (string.IsNullOrEmpty(animationName))
      {
        errorMessage = "Animation Name is empty";
        return false;
      }

      if (string.IsNullOrEmpty(animationPath))
      {
        errorMessage = "Animation Path is empty";
        return false;
      }

      // Create directory if needed
      if (!Directory.Exists(animationPath))
      {
        try
        {
          Directory.CreateDirectory(animationPath);
        }
        catch (System.Exception ex)
        {
          errorMessage = $"Failed to create directory: {ex.Message}";
          return false;
        }
      }

      // Get the blendshape index
      int blendShapeIndex = meshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
      if (blendShapeIndex == -1)
      {
        errorMessage = "BlendShape not found";
        return false;
      }

      // Create animation clips
      AnimationClip animationClipMin = new AnimationClip();
      AnimationClip animationClipMax = new AnimationClip();
      animationClipMin.name = animationName + "_min";
      animationClipMax.name = animationName + "_max";

      // Setup paths
      string meshRendererGameObjectPath = GetGameObjectPath(meshRenderer.gameObject);
      string rootGameObjectPath = GetGameObjectPath(rootGameObject);
      string relativePath = meshRendererGameObjectPath.Replace(rootGameObjectPath, "");
      if (relativePath.StartsWith("/"))
      {
        relativePath = relativePath.Substring(1);
      }

      // Create curve bindings
      EditorCurveBinding curveBinding = new EditorCurveBinding
      {
        type = typeof(SkinnedMeshRenderer),
        path = relativePath,
        propertyName = "blendShape." + blendShapeName
      };

      // Create keyframes and curves
      AnimationCurve curveMin = new AnimationCurve(new Keyframe(0, 0));
      AnimationCurve curveMax = new AnimationCurve(new Keyframe(0, 100));

      // Set curves
      AnimationUtility.SetEditorCurve(animationClipMin, curveBinding, curveMin);
      AnimationUtility.SetEditorCurve(animationClipMax, curveBinding, curveMax);

      // Save animation clips
      string minPath = Path.Combine(animationPath, $"{animationClipMin.name}.anim").Replace("\\", "/");
      string maxPath = Path.Combine(animationPath, $"{animationClipMax.name}.anim").Replace("\\", "/");

      AssetDatabase.CreateAsset(animationClipMin, minPath);
      AssetDatabase.CreateAsset(animationClipMax, maxPath);
      AssetDatabase.Refresh();

      return true;
    }
  }
}
#endif
