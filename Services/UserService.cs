using JWT.Entities;
using JWT.Helpers;
using JWT.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest req);
        IEnumerable<User> GetAll();

        User GetById(int id);

    }
    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;

        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Peter", LastName = "Anderson", UserName = "Peter", Password = "123" },
            new User { Id = 2, FirstName = "Rick", LastName = "Thomas", UserName = "Rick", Password = "456" },
            new User { Id = 3, FirstName = "Joe", LastName = "Johnson", UserName = "Joe", Password = "789" },
            new User { Id = 4, FirstName = "Angel", LastName = "Gomez", UserName = "Angel", Password = "098" }
        };

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User GetById(int id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest req)
        {
            User user = _users.SingleOrDefault(x => x.UserName == req.UserName && x.Password == req.Password);

            if (user == null) return null;

            AuthenticateResponse res = new AuthenticateResponse();
            res.Token = GetJWTToken(user);

            return res;
        }

        private string GetJWTToken(User user)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
