using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace EPAD_Backend_Core.Provider
{
    public class TokenProvider
    {
        public static string CreateJsonWebToken(IMemoryCache cache, UserInfo user,ref string guid)
        {
            var key = Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
            guid = Guid.NewGuid().ToString();
            var claims = new[] {
                new Claim(ClaimTypes.Name, guid),
                new Claim("Guid", guid),
                new Claim("UserName", user.UserName),
                new Claim("LoginType", user.UserName.StartsWith("Service_") ? "Service" : "User")
            };

            //Generate Token for user 
            var JWToken = new JwtSecurityToken(
                issuer: "",
                audience: "",
                claims: claims,
                notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddDays(1)).DateTime,
                //Using HS256 Algorithm to encrypt Token  
                signingCredentials: new SigningCredentials
                (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
            
            user.AddToCache(cache,guid);
            return token;

        }

        public static string CreateJsonWebTokenForClient(IMemoryCache cache, UserInfo user, ref string guid)
        {
            var key = Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
            guid = Guid.NewGuid().ToString();
            var claims = new[] {
                new Claim(ClaimTypes.Name, guid),
                new Claim("Guid", guid),
                new Claim("UserName", user.UserName),
                new Claim("LoginType", user.UserName.StartsWith("Service_") ? "Service" : "User")
            };

            //Generate Token for user 
            var JWToken = new JwtSecurityToken(
                issuer: "",
                audience: "",
                claims: claims,
                notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddMinutes(5)).DateTime,
                //Using HS256 Algorithm to encrypt Token  
                signingCredentials: new SigningCredentials
                (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(JWToken);

            user.AddToCacheWithMinutes(cache, guid, 5);
            return token;

        }

        private IEnumerable<Claim> GetUserClaims(User user)
        {
            
            IEnumerable<Claim> claims = new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.FIRST_NAME + " " + user.LAST_NAME),
                    new Claim("USERID", user.USERID),
                    new Claim("EMAILID", user.EMAILID),
                    new Claim("PHONE", user.PHONE),
                    new Claim("ACCESS_LEVEL", user.ACCESS_LEVEL.ToUpper()),
                    new Claim("READ_ONLY", user.READ_ONLY.ToUpper())
                    };
            return claims;
        }
        public class User
        {
            public string USERID { get; set; }
            public string PASSWORD { get; set; }
            public string FIRST_NAME { get; set; }
            public string LAST_NAME { get; set; }
            public string EMAILID { get; set; }
            public string PHONE { get; set; }
            public string ACCESS_LEVEL { get; set; }
            public string READ_ONLY { get; set; }
        }
    }
}
