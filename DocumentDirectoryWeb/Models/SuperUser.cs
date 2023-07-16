namespace DocumentDirectoryWeb.Models;

public class SuperUser
{
    public SuperUser(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; set; }
    public string Password { get; set; }
}