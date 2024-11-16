using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserViewModel> RetrieveAll(int? id = null, string firstName = null);
        UserViewModel RetrieveUser(int id);
        UserViewModel RetrieveUserByUsername(string userCode);
        void Add(UserViewModel model);
        void Update(UserViewModel model);
        void Delete(int id);
        LoginResult AuthenticateUser(string userCode, string password, ref MUser user);
        public bool UsernameAvailability(string username, int id);
        public bool EmailAvailability(string email, int id);
        bool IsUsernameTaken(string username);
        bool IsEmailTaken(string email);
        MUser GetUserByVerificationToken(string token);
        MUser GetUserByEmail(string email);
        MUser GetUserByPasswordResetToken(string token);
        string GetUserProfilePic(string userCode);
        public void UpdatePassword(UserViewModel model);
        public void UpdateEmail(UserViewModel model);
        public void UpdateUsername(UserViewModel model);
        public void UpdateProfile(UserViewModel model);
    }
}
