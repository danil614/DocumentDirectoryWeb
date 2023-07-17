using System.Security.Claims;

namespace DocumentDirectoryWeb.Helpers;

public static class UserTabManager
{
    // Создание словаря вкладок
    private static readonly Dictionary<string, TabItem> Tabs = new()
    {
        { "DocumentView", new TabItem("Просмотр документов", "DocumentView", "Index") },
        { "DocumentManagement", new TabItem("Управление документами", "DocumentManagement", "Index") },
        { "Categories", new TabItem("Разделы", "Categories", "Index") },
        { "Users", new TabItem("Пользователи", "Users", "Index") },
        { "Departments", new TabItem("Подразделения", "Departments", "Index") }
    };

    public static string? GetUserType(IEnumerable<Claim> claims)
    {
        var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType);
        return roleClaim?.Value;
    }

    /// <summary>
    ///     Метод для формирования списка вкладок на основе UserType.
    /// </summary>
    public static List<TabItem> GetTabsForUserType(string? userType)
    {
        var userTabs = new List<TabItem>();

        switch (userType)
        {
            case "Admin":
                userTabs.AddRange(Tabs.Values);
                break;
            case "Editor":
                userTabs.AddRange(new[]
                {
                    Tabs["DocumentView"],
                    Tabs["DocumentManagement"],
                    Tabs["Categories"]
                });
                break;
            case "User":
                userTabs.AddRange(new[]
                {
                    Tabs["DocumentView"]
                });
                break;
        }

        return userTabs;
    }
}