namespace uk.novavoidhowl.dev.nvhpmm
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
