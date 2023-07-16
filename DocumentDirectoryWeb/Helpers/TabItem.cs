namespace DocumentDirectoryWeb.Helpers;

public class TabItem
{
    public TabItem(string displayName, string controller, string action)
    {
        DisplayName = displayName;
        Controller = controller;
        Action = action;
    }

    public string DisplayName { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }
}