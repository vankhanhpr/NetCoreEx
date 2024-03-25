using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AuthService.services.auth;
using FuBonServices.data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelClass.respond;
using ModelClass.user;
using ModelClassLibrary.area.auth;
using ModelClassLibrary.interfaces;
using WebApi.reponsitory.imlp;
namespace AuthServer.service.impl
{
    public class AuthImpl : Reponsitory<User>, IAuth
    {
        private IOptions<Audience> m_audience;
        private IHashPass m_hashPass;

        public AuthImpl(DataContext context, IHashPass _hashPass, IOptions<Audience> _audience) : base(context)
        {
            m_hashPass = _hashPass;
            m_audience = _audience;
        }
        /*
         * login 
         */
        public DataRespond login(User user, string lang)
        {
            DataRespond data = new DataRespond();

            //User x = new User();
            //x.username = "admin";
            //x.password = m_hashPass.hashPass("Vnpt@123");
            ////x.status = true;
            //insert(x);

            var us = getAll().FirstOrDefault(m=>m.username == user.username);

            if (us!= null || us.status == 1)
            {
                if (m_hashPass.checkPass(us.password, user.password) == false)
                {
                    data.success = false;
                    data.message = "Login success";
                    //data.message = m_translate.loadJsonLogin(lang).passwordnotcorrect;
                    return data;
                }
                data.success = true;
                data.data = new { token = genToken(us), user = us };
                data.message = "success";// m_translate.loadJsonLogin(lang).loginsuccess;
            }
            else
            {
                data.success = false;
                data.message = "User or password is not correct" ;// m_translate.loadJsonLogin(lang).passwordnotcorrect;
            }
            return data;
        }
        public dynamic genToken(User user)
        {
            var now = DateTime.UtcNow;
            var claims = new Claim[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
            //new Claim(ClaimTypes.Role,user.role.ToString())//check quyen
            };

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(m_audience.Value.Secret));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = m_audience.Value.Iss,
                ValidateAudience = true,
                ValidAudience = m_audience.Value.Aud,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,

            };

            var jwt = new JwtSecurityToken(
                issuer: m_audience.Value.Iss,
                audience: m_audience.Value.Aud,
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromHours(8)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var responseJson = new
            {
                access_token = encodedJwt,
                expires_in = (int)TimeSpan.FromDays(1).TotalSeconds
            };

            return responseJson;
        }
    }
}
