﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PeiuPlatform.Model
{
    public class JasonWebTokenManager
    {
        public readonly static string SECRET;
        static JasonWebTokenManager()
        {
            //HMACSHA256 hmac = new HMACSHA256();
            //string secret = Convert.ToBase64String(hmac.Key);
            //Secret = secret;
            SECRET = Environment.GetEnvironmentVariable("JWT_SECRET");
        }

        public static ClaimsPrincipal GetPrincipal(string userid, string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;
                byte[] key = Encoding.ASCII.GetBytes(SECRET);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    NameClaimType = userid,
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch (Exception ex)/* (Exception ex)*/
            {
                return null;
            }
        }

        public static string ValidateToken(string userid, string token, string ClaimType)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(userid, token);

            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
            Claim usernameClaim = identity.FindFirst(ClaimType);
            username = usernameClaim.Value;
            return username;
        }

        public static ClaimsIdentity ValidateToken(string userid, string token)
        {
            ClaimsPrincipal principal = GetPrincipal(userid, token);

            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch /*(Exception ex)*/
            {
                return null;
            }
            return identity;
        }

        public static string GenerateToken(string userid, IList<Claim> claims)
        {
            //    var claims = new[]
            //{
            //    new Claim(ClaimTypes.Name, username),
            //    new Claim(ClaimTypes.sc)
            //};
            try

            {
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SECRET));

                //var security_key = new SymmetricSecurityKey(Convert.FromBase64String(Secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(

                    issuer: "https://www.peiu.co.kr",
                    audience: userid,
                    claims: claims,
                    expires: DateTime.Now.AddYears(1),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }


        }
        // public static string Secret => 
        // {
        //     HMACSHA256 hmac = new HMACSHA256();

        // }
    }
}
