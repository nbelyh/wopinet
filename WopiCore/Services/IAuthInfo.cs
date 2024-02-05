namespace WopiCore.Services
{
    public interface IAuthInfo
    {
        string UserId { get; set; }
        string UserFriendlyName { get; set; }
    }
}
