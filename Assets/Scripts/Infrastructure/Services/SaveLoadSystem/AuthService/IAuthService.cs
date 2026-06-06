namespace Infrastructure.Services.SaveLoadSystem.AuthService
{
    public interface IAuthService
    {
        void SetIsNewUserValue(bool isNewUser);
        bool IsNewUser();
    }
}