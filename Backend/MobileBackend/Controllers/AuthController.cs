using System;
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
                    expires: DateTime.Now.AddMinutes(20),
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

            if (!_repository.University.CheckIfExisting(user.University_Id)) {

                return BadRequest("Choosen UniversityId was not found!");

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
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
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
            string email = AuthControllerExtensions.JwtNameExtractor(token);

            var dbUser = _repository.User.GetUserByEmail(email);

            if (dbUser == null)
            {
                return BadRequest("Invalid client request, User was not found");
            }

            dbUser.Password = user.Password;

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
            string email = AuthControllerExtensions.JwtNameExtractor(token);

            var dbUser = _repository.User.GetUserByEmail(email);

            try
            {
                // Delete all memberships of user
                var removeUserToGroup = _repository.UserToGroup.GetMembershipsForUser(dbUser);

                foreach (var mem in removeUserToGroup)
                {
                        _repository.UserToGroup.DeleteMembership(mem);         
                }

                _repository.Save();

                // Delete all groups of user
                var groupsFromUser = _repository.Group.FindByCondition(gr => gr.AdminUserId == dbUser.Id).ToList();

                var membershipsInGroupsFromUser = new List<UserToGroup>();

                foreach (var group in groupsFromUser) {
                    var tempMemberships = _repository.UserToGroup.GetMembershipsForGroup(group).ToList();
                    foreach (var mem in tempMemberships) {
                        membershipsInGroupsFromUser.Add(mem);
                    }
                }

                foreach (var mem in membershipsInGroupsFromUser) {
                    _repository.UserToGroup.DeleteMembership(mem);
                }
                _repository.Save();

                foreach (var group in groupsFromUser) {
                    _repository.Group.DeleteGroup(group);
                }
                _repository.Save();

                _repository.User.DeleteUser(dbUser);
                _repository.Save();

                return NoContent();            
            }
            catch(Exception e)
            {
                _logger.LogError($"Something went wrong inside DeleteUser action: {e.InnerException}");
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

        [Authorize]
        [HttpGet]
        public IActionResult GetUser()
        {
            string token = Request.Headers["Authorization"];
            string email = AuthControllerExtensions.JwtNameExtractor(token);

            var user = _repository.User.GetUserByEmail(email);

            if (user == null)
            {
                _logger.LogError($"User with email {user.Email} was not found");
                return BadRequest($"User with email {user.Email} was not found");
            }

            return Ok(user);

        }
    }
}