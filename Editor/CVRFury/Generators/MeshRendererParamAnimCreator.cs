#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

using uk.novavoidhowl.dev.cvrfury.packagecore;
using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;
using static uk.novavoidhowl.dev.cvrfury.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.cvrfury.generator
{
  public class MeshRendererParamAnimCreator
  {
    public struct AnimationConfig
    {
      public GameObject rootGameObject;
      public Component renderer;
      public string paramName;
      public string paramType;
      public float minValue;
      public float maxValue;
      public string animationName;
      public string animationPath;
    }

    public static bool GenerateAnimations(AnimationConfig config)
    {
      // Create directory if needed
      if (!Directory.Exists(config.animationPath))
      {
        Directory.CreateDirectory(config.animationPath);
      }

      // Get relative path from root to renderer
      string rendererPath = GetGameObjectPath(config.renderer.gameObject);
      string rootPath = GetGameObjectPath(config.rootGameObject);
      string relativePath = rendererPath.Replace(rootPath, "").TrimStart('/');

      // Create animation clips
      AnimationClip minClip = new AnimationClip();
      AnimationClip maxClip = new AnimationClip();

      // Set clip names based on parameter type
      if (config.paramType == "Bool")
      {
        minClip.name = $"{config.animationName}_false";
        maxClip.name = $"{config.animationName}_true";
      }
      else
      {
        minClip.name = $"{config.animationName}_min";
        maxClip.name = $"{config.animationName}_max";
      }

      // Create curve binding with correct type
      EditorCurveBinding binding = new EditorCurveBinding
      {
        type = config.renderer is MeshRenderer ? typeof(MeshRenderer) : typeof(SkinnedMeshRenderer),
        path = relativePath,
        propertyName = config.paramName
      };

      // Create curves based on parameter type
      switch (config.paramType)
      {
        case "Bool":
          AnimationCurve minBoolCurve = AnimationCurve.Constant(0, 0, 0);
          AnimationCurve maxBoolCurve = AnimationCurve.Constant(0, 0, 1);
          AnimationUtility.SetEditorCurve(minClip, binding, minBoolCurve);
          AnimationUtility.SetEditorCurve(maxClip, binding, maxBoolCurve);
          break;

        case "Int":
        case "Float":
          AnimationCurve minCurve = AnimationCurve.Constant(0, 0, config.minValue);
          AnimationCurve maxCurve = AnimationCurve.Constant(0, 0, config.maxValue);
          AnimationUtility.SetEditorCurve(minClip, binding, minCurve);
          AnimationUtility.SetEditorCurve(maxClip, binding, maxCurve);
          break;
      }

      // Save animation clips
      string minPath = Path.Combine(config.animationPath, $"{minClip.name}.anim").Replace("\\", "/");
      string maxPath = Path.Combine(config.animationPath, $"{maxClip.name}.anim").Replace("\\", "/");

      try
      {
        AssetDatabase.CreateAsset(minClip, minPath);
        AssetDatabase.CreateAsset(maxClip, maxPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
      }
      catch (System.Exception e)
      {
        Debug.LogError($"Failed to create animation clips: {e.Message}");
        return false;
      }
    }
  }
}
#endif
