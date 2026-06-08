using UnityEngine;

namespace Infrastructure.Services.SaveLoadSystem.AuthService
{
    public class AuthService : IAuthService
    {
        private const string IsNewUserKey = "isNewUser";
        
        public void SetIsNewUserValue(bool isNewUser)
        {
            PlayerPrefs.SetInt(IsNewUserKey, isNewUser ? 1 : 0);

            PlayerPrefs.Save();
        }
        
        public bool IsNewUser() =>
            PlayerPrefs.HasKey(IsNewUserKey) == false || PlayerPrefs.GetInt(IsNewUserKey) == 1;
    }
}