namespace DocumentDirectoryWeb.Helpers;

public class TabItem
{
    public string DisplayName { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }

    public TabItem(string displayName, string controller, string action)
    {
        DisplayName = displayName;
        Controller = controller;
        Action = action;
    }
}
