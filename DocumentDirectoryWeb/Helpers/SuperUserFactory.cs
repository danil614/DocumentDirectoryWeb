using DocumentDirectoryWeb.Models;

namespace DocumentDirectoryWeb.Helpers;

public class SuperUserFactory
{
    private readonly IConfiguration _configuration;

    public SuperUserFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SuperUser CreateSuperUser()
    {
        var username = _configuration.GetValue<string>("SuperUser:Username");
        var password = _configuration.GetValue<string>("SuperUser:Password");

        if (password != null && username != null)
            return new SuperUser(username, password);

        return new SuperUser("yUwKNeCKWf", "VkTPO7LlGE");
    }
}