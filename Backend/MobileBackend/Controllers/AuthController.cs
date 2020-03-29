﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Contracts;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Mobile_Backend.Extensions;

namespace Mobile_Backend.Controllers
{
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;

        public AuthController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
        }

        [HttpPost, Route("login")]
       public IActionResult Login([FromBody]User user)
        {
            if(user == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid exercise object sent from client.");
                return BadRequest("Invalid model object");
            }

            bool valid = _repository.User.ValidateUser(user);

            if(valid)
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokenOptions = new JwtSecurityToken(
                    issuer: "http://localhost:5000",
                    audience: "http://localhost:5000",
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signinCredentials
                    );

                tokenOptions.Payload["email"] = user.Email;

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                return Ok(new { Token = tokenString });
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost, Route("register")]
        public IActionResult Register([FromBody]User user)
        {
            if(User == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            if (user.Email != null)
            {
                if (!AuthControllerExtensions.IsValidEmail(user.Email)) {
                    return BadRequest("Email format is not valid!");
                }

                if (_repository.User.CheckIfExisting(user.Email))
                {
                    return BadRequest("User is already existing!");
                }
            }

            try
            {
                var userPwd = AuthControllerExtensions.GenerateFirstPassword();
                user.Password = userPwd;
    
                _repository.User.RegisterUser(user);
                _repository.Save();

                _emailSender.SendEmailAsync(user.Email, "Your password for Studi App", $"<b>Your password: {userPwd}</b>");

                return NoContent();

            } catch (Exception e)

            {   _logger.LogError($"Something went wrong inside Register action: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpPut, Route("change_user")]
        public IActionResult ChangeUser([FromBody]User user) {
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            try {
                var userMail = JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                dbUser.Map(user);
                _repository.User.Update(dbUser);
                _repository.Save();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside ChangeUser action: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpPost, Route("change_password")]
        public IActionResult ChangePassword([FromBody]User user)
        {
            string token = Request.Headers["Authorization"];
            string email = AuthController.JwtNameExtractor(token);

            var dbUser = _repository.User.GetUserByEmail(email);
            dbUser.Password = user.Password;

            if (dbUser == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            try
            {
                _repository.User.ChangePassword(dbUser);
                _repository.Save();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside ChangePassword action: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpDelete]
        public IActionResult DeleteUser()
        {
            string token = Request.Headers["Authorization"];
            string email = AuthController.JwtNameExtractor(token);

            var dbUser = _repository.User.GetUserByEmail(email);

            try
            {            
                    _repository.User.DeleteUser(dbUser);
                    _repository.Save();

                    return NoContent();            
            }
            catch(Exception e)
            {
                _logger.LogError($"Something went wrong inside DeleteUser action: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, Route("reset_password")]
        public IActionResult ResetPassword([FromBody]User user) {
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            var dbUser = _repository.User.GetUserByEmail(user.Email);

            if (dbUser == null) {
                _logger.LogError($"User with email {user.Email} was not found");
                return BadRequest($"User with email {user.Email} was not found");
            }

            var newPassword = AuthControllerExtensions.GenerateFirstPassword();
            dbUser.Password = newPassword;

            _repository.User.ChangePassword(dbUser);
            _repository.Save();

            _emailSender.SendEmailAsync(user.Email, "Your password for Studi App has been resetted.", $"<b>Your password: {newPassword}</b>");

            return NoContent();

        }

        public static string JwtNameExtractor(string token)
        {
            token = token.Split(' ')[1];

            var jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(token))
            {
                var readToken = jwtHandler.ReadJwtToken(token);
                var payload = readToken.Claims.FirstOrDefault(e => e.Type.Equals("email"));
                var email = payload.Value;

                return email;
            }
            else
            {
                return "";
            }
        }
    }
}