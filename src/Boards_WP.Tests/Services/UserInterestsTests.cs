using System;
using System.Security.Cryptography;
using System.Text;

using Moq;

using Xunit;

using Boards_WP.Data.Models;
using Boards_WP.Data.Services;
using Boards_WP.Data.Repositories.Interfaces;

namespace Boards_WP.Tests.Services
{
    public class UsersServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepoMock;
        private readonly UsersService _service;

        public UsersServiceTests()
        {
            _usersRepoMock = new Mock<IUsersRepository>();
            _service = new UsersService(_usersRepoMock.Object);
        }

        private static string HashPasswordForTest(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        [Fact]
        public void Login_UserNull_ThrowsWrappedException()
        {
            // Arrange
            _usersRepoMock
                .Setup(repo => repo.GetUserByEmail("test@mail.com"))
                .Returns((User)null!);

            // Act
            var exception = Assert.Throws<Exception>(() => _service.Login("test@mail.com", "password123"));

            // Assert
            Assert.Equal("An error occurred during login.", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Invalid email or password.", exception.InnerException!.Message);
        }

        [Fact]
        public void Login_WrongPassword_ThrowsWrappedException()
        {
            // Arrange
            var user = new User
            {
                UserID = 1,
                Email = "test@mail.com",
                PasswordHash = HashPasswordForTest("correct-password")
            };

            _usersRepoMock
                .Setup(repo => repo.GetUserByEmail("test@mail.com"))
                .Returns(user);

            // Act
            var exception = Assert.Throws<Exception>(() => _service.Login("test@mail.com", "wrong-password"));

            // Assert
            Assert.Equal("An error occurred during login.", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Invalid email or password.", exception.InnerException!.Message);
        }

        [Fact]
        public void Login_CorrectPassword_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                UserID = 1,
                Email = "test@mail.com",
                PasswordHash = HashPasswordForTest("correct-password")
            };

            _usersRepoMock
                .Setup(repo => repo.GetUserByEmail("test@mail.com"))
                .Returns(user);

            // Act
            var result = _service.Login("test@mail.com", "correct-password");

            // Assert
            Assert.Equal(user, result);
        }
    }
}