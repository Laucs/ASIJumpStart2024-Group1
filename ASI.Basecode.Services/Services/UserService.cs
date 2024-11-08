using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = repository;
        }

        public IEnumerable<UserViewModel> RetrieveAll(int? id = null, string firstName = null)
        {
            var data = _userRepository.GetUsers()
                .Where(x => x.Deleted != true
                        && (!id.HasValue || x.UserId == id)
                        && (string.IsNullOrEmpty(firstName) || x.FirstName.Contains(firstName)))
                .Select(s => new UserViewModel
                {
                    Id = s.UserId,
                    Name = string.Concat(s.FirstName, " ", s.LastName),
                    Description = s.Remarks,
                });
            return data;
        }

        public UserViewModel RetrieveUser(int id)
        {
            var data = _userRepository.GetUsers().FirstOrDefault(x => x.Deleted != true && x.UserId == id);
            var model = new UserViewModel
            {
                Id = data.UserId,
                UserCode = data.UserCode,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Password = PasswordManager.DecryptPassword(data.Password)
            };
            return model;
        }

        public UserViewModel RetrieveUserByUsername(string userCode)
        {
            var data = _userRepository.GetUsers().FirstOrDefault(x => x.Deleted != true && x.UserCode.ToLower().Trim() == userCode.ToLower().Trim());
            var user = new UserViewModel()
            {
                Id = data.UserId,
                UserCode = data.UserCode,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Password = PasswordManager.DecryptPassword(data.Password),
                ProfilePic = data.ProfileImg,
                Mail = data.Mail
            };

            return user;
        }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(UserViewModel model)
        {
            var newModel = new MUser
            {
                UserCode = model.UserCode,
                FirstName = model.FirstName ?? null,
                LastName = model.LastName ?? null,
                Password = PasswordManager.EncryptPassword(model.Password),
                Mail = model.Mail,
                UserRole = 1,
                EmailVerificationToken = model.EmailVerificationToken,
                VerificationTokenExpiration = model.VerificationTokenExpiration,
                IsEmailVerified = false
            };

            _userRepository.AddUser(newModel);
        }


        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(UserViewModel model)
        {
            var existingData = _userRepository.GetUsers().Where(s => s.Deleted != true && s.UserId == model.Id).FirstOrDefault();
            existingData.UserCode = model.UserCode;
            existingData.FirstName = model.FirstName;
            existingData.LastName = model.LastName;
            existingData.Mail = model.Mail;
            existingData.Password = PasswordManager.EncryptPassword(model.Password);
            existingData.EmailVerificationToken = model.EmailVerificationToken;
            existingData.VerificationTokenExpiration = model.VerificationTokenExpiration;
            existingData.PasswordResetToken = model.PasswordResetToken;
            existingData.ResetTokenExpiration = model.ResetTokenExpiration;
            existingData.IsEmailVerified = true;
            _userRepository.UpdateUser(existingData);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(int id)
        {
            _userRepository.DeleteUser(id);
        }

        public LoginResult AuthenticateUser(string userCode, string password, ref MUser user)
        {

            user = new MUser();
            var passwordKey = PasswordManager.EncryptPassword(password.Trim());
            user = _userRepository.GetUsers().Where(x => x.UserCode.Trim() == userCode.Trim() &&
                                                          x.Password == passwordKey &&
                                                          x.IsEmailVerified).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public bool ValidatePassword(string username, string password)
        {
            var currentUser = RetrieveUserByUsername(username);
            return currentUser.Password.Equals(password);
        }

        public void UpdateProfile(ProfileViewModel model)
        {
            var existingData = _userRepository.GetUsers().Where(s => s.Deleted != true && s.UserCode.Trim().ToLower() == model.UserCode.Trim().ToLower()).FirstOrDefault();
            existingData.ProfileImg = model.ProfilePicture;
            _userRepository.UpdateUser(existingData);
        }

        public bool UsernameAvailability(string username, int id)
        {
            return _userRepository.GetUsers()
                .Any(x => x.UserCode.ToUpper() == username.ToUpper() &&
                            x.UserId != id &&
                            x.Deleted != true);
        }

        public bool EmailAvailability(string email, int id)
        {
            return _userRepository.GetUsers()
                .Any(x => x.Mail.ToLower() == email.ToLower() &&
                            x.UserId != id &&
                            x.Deleted != true);
        }

        public bool IsUsernameTaken(string username)
        {
            return _userRepository.GetUsers()
                .Any(x => x.UserCode.ToUpper() == username.ToUpper() && x.Deleted != true);
        }

        public bool IsEmailTaken(string email)
        {
            return _userRepository.GetUsers()
                .Any(x => x.Mail.ToLower() == email.ToLower() && x.Deleted != true);
        }

        public MUser GetUserByVerificationToken(string token)
        {
            return _userRepository.GetUsers().SingleOrDefault(u => u.EmailVerificationToken == token && u.VerificationTokenExpiration > DateTime.Now && !u.Deleted);
        }

        public MUser GetUserByEmail(string email)
        {
            return _userRepository.GetUsers().FirstOrDefault(u => u.Mail == email && !u.Deleted);
        }

        public MUser GetUserByPasswordResetToken(string token)
        {
            return _userRepository.GetUsers().FirstOrDefault(u => u.PasswordResetToken == token && u.ResetTokenExpiration > DateTime.Now && !u.Deleted);
        }

        public void UpdateEmail(string newEmail, string currentUser)
        {
            var existingUser = _userRepository.GetUsers().FirstOrDefault(u => u.UserCode == currentUser);
            if (existingUser != null)
            {
                existingUser.Mail = newEmail;
                _userRepository.UpdateUser(existingUser);
            }
        }

        public void UpdatePassword(ChangePasswordViewModel model, string userCode)
        {
            var existingData = _userRepository.GetUsers().Where(s => s.Deleted != true && s.UserCode == userCode).FirstOrDefault();
            if (existingData != null)
            {
                var currentPassword = PasswordManager.DecryptPassword(existingData.Password);
                existingData.Password = PasswordManager.EncryptPassword(model.NewPassword);
                _userRepository.UpdateUser(existingData);
            }
        }


    }
}
