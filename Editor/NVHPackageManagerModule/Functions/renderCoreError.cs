// editor only script to manage the dependencies
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Only need to change the following line, in the following files:
//
// DepManager.cs
// render1stPartyDeps.cs
// render3rdPartyDeps.cs
// renderAppComponents.cs
// renderCoreError.cs
// Validation.cs
// AppInternalPackages.cs
// DepManagerConfig.cs
// PrimaryDependenciesPackages.cs
// ThirdPartyDependenciesPackages.cs
//
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.cvrfury.packagecore.Constants;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public partial class ToolSetup : EditorWindow
  {
    private static VisualElement renderCoreError(string errorMessageUI, String errorMessageConsole)
    {
      Debug.LogError(errorMessageConsole);

      //add error message container
      var errorContainer = new VisualElement();

      // make error container flex layout with vertical direction
      errorContainer.style.flexDirection = FlexDirection.Column;
      // set sub element alignment to center
      errorContainer.style.alignItems = Align.Center;
      // set justify content to center
      errorContainer.style.justifyContent = Justify.Center;
      // set the height to 100%
      errorContainer.style.height = new Length(100, LengthUnit.Percent);
      // set the width to 100%
      errorContainer.style.width = new Length(100, LengthUnit.Percent);
      // set the padding
      errorContainer.style.paddingTop = new StyleLength(20f);
      errorContainer.style.paddingBottom = new StyleLength(20f);
      errorContainer.style.paddingLeft = new StyleLength(20f);
      errorContainer.style.paddingRight = new StyleLength(20f);

      // load the VectorImage from the Resources folder
      VectorImage errorIcon = Resources.Load<VectorImage>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/IconsAndImages/error"
      );

      // create a StyleBackground from the VectorImage
      StyleBackground errorBackground = new StyleBackground(errorIcon);

      // add icon element to the errorContainer
      var icon = new VisualElement();
      // add the icon class to the icon
      icon.AddToClassList("icon");
      // set the StyleBackground as the background image for the 'icon' UI element
      icon.style.backgroundImage = errorBackground;
      // set the width and height of the icon to 40px
      icon.style.width = new Length(40, LengthUnit.Pixel);
      icon.style.height = new Length(40, LengthUnit.Pixel);
      // add margin to the icon
      icon.style.marginBottom = new StyleLength(20f);

      // add the icon to the errorContainer
      errorContainer.Add(icon);

      // add a label to say that the config file is missing
      var infoMessage = new Label(errorMessageUI);
      // add the info message class
      infoMessage.AddToClassList("infoMessage");
      errorContainer.Add(infoMessage);

      // return the errorContainer
      return errorContainer;
    }
  }
}
#endif
