using JWT.Helpers;
using JWT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT.Middlewares
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        public JWTMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
            
    }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            string[] arr;
            string token = ""; 
            var auth = context.Request.Headers["Authorization"].FirstOrDefault();

            if(auth != null)
            {
                arr = auth.Split(" ");
                token = arr.Length == 2 ? arr[1] : "";
            }

            if (token != "")
                AddUserToContext(token, context, userService);

            await _next(context);
        }

        private void AddUserToContext(string token, HttpContext context, IUserService userService)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var temp = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(temp.Claims.First(x => x.Type == "Id").Value);

            context.Items["User"] = userService.GetById(userId);
        }
    }
}
