using System;
using System.Security.Cryptography;
using System.Text;

using Boards_WP.Data.Models;
using Boards_WP.Data.Repositories.Interfaces;
using Boards_WP.Data.Services.Interfaces;

namespace Boards_WP.Data.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepo;

        public UsersService(IUsersRepository usersRepo)
        {
            _usersRepo = usersRepo;
        }

        public User GetUserByID(int userID)
        {
            try
            {
                var user = _usersRepo.GetUserByID(userID);
                if (user == null)
                {
                    throw new Exception($"User with ID {userID} was not found.");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the user.", ex);
            }
        }

        public User Login(string email, string password)
        {
            try
            {
                var user = _usersRepo.GetUserByEmail(email);

                if (user == null)
                {
                    throw new Exception("Invalid email or password.");
                }


                string hashedPasswordInput = HashPassword(password);

                if (user.PasswordHash != hashedPasswordInput)
                {
                    throw new Exception("Invalid email or password.");
                }   

                return user; 
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during login.", ex);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}