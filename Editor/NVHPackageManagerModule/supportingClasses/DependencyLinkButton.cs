// editor only script to manage the dependencies
#if UNITY_EDITOR

namespace uk.novavoidhowl.dev.cvrfury.nvhpmm
{
  public sealed class DependencyLinkButton
  {
    public string ButtonText { get; }
    public string ButtonLink { get; }

    public DependencyLinkButton(string buttonText, string buttonLink)
    {
      ButtonText = buttonText;
      ButtonLink = buttonLink;
    }
  }
}
#endif
