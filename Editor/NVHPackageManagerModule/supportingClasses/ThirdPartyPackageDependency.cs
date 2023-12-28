using System.Collections.Generic;

namespace uk.novavoidhowl.dev.nvhpmm
{
  public sealed class ThirdPartyPackageDependency
  {
    public string Name { get; }
    public string Description { get; }
    public string DependencyType { get; }
    public string InstallCheckMode { get; }
    public string InstallCheckValue { get; }
    public List<DependencyLinkButton> Buttons { get; set; }

    public ThirdPartyPackageDependency(
      string name,
      string description,
      string dependencyType,
      string installCheckMode,
      string installCheckValue,
      List<DependencyLinkButton> buttons
    )
    {
      Name = name;
      Description = description;
      DependencyType = dependencyType;
      InstallCheckMode = installCheckMode;
      InstallCheckValue = installCheckValue;
      Buttons = buttons;
    }
  }
}
