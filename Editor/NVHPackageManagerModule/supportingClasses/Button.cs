namespace uk.novavoidhowl.dev.nvhpmm
{
  public sealed class Button
  {
    public string ButtonText { get; }
    public string ButtonLink { get; }

    public Button(string buttonText, string buttonLink)
    {
      ButtonText = buttonText;
      ButtonLink = buttonLink;
    }
  }
}
