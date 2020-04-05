using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mobile_Backend.Extensions;

namespace Mobile_Backend.Controllers
{
    [Route("api/group")]
    public class GroupController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;

        public GroupController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
        }

        [Authorize]
        [HttpPost, Route("create")]
        public IActionResult CreateGroup([FromBody]Group group)
        {
            if (group == null)
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

                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                group.AdminUserId = dbUser.Id;

                _repository.Group.CreateGroup(group);
                _repository.Save();

                return Ok(_repository.Group.GetGroupById(group.Id));
            }
            catch (Exception e) {
                _logger.LogError($"Group object is not valid");
                return BadRequest($"Group object is not valid");
            }
        }

        [Authorize]
        [HttpDelete, Route("delete")]
        public IActionResult DeleteGroup([FromBody]Group group)
        {

            group = _repository.Group.GetGroupById(group.Id);

            if (group == null)
            {
                _logger.LogError("Group to delete was not found!");
                return BadRequest("Invalid client request, group was not found!");
            }

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var dbUser = _repository.User.GetUserByEmail(userMail);

            if (!(group.AdminUserId == dbUser.Id))
            {
                _logger.LogError("Only admin can delte groups!");
                return BadRequest("Only group admin can delete this group!");
            }

            var listOfMembers = _repository.UserToGroup.GetMembershipsForGroup(group);

            foreach (var mem in listOfMembers)
            {
                _repository.UserToGroup.DeleteMembership(mem);
            }

            _repository.Save();

            _repository.Group.DeleteGroup(group);

            _repository.Save();

            return NoContent();
        }

        [Authorize]
        [HttpPut, Route("update")]
        public IActionResult UpdateGroup([FromBody]Group group) {

            if (group == null)
            {
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            var dbGroup = _repository.Group.GetGroupById(group.Id);

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var dbUser = _repository.User.GetUserByEmail(userMail);

            if (dbUser.Id == dbGroup.AdminUserId) {
                dbGroup.Map(group);
                _repository.Group.Update(dbGroup);
                _repository.Save();
                return NoContent();
            }

            _logger.LogError("Only admin can change group");
            return BadRequest("Only admin can change group");
        }

        [Authorize]
        [HttpGet, Route("members")]
        public IActionResult GetMembers([FromBody] Group group)
        {

            group = _repository.Group.GetGroupById(group.Id);

            if (group == null)
            {
                _logger.LogError($"Group with id {group.Id} was not found!");
                return BadRequest("Invalid client request");
            }

            var listMembers = _repository.UserToGroup.GetMembersForGroup(group);

            return Ok(listMembers);
        }

        [Authorize]
        [HttpPost, Route("add_member")]
        public IActionResult AddMember([FromBody] UserToGroup userToGroupParam) {
            var group = _repository.Group.GetGroupById(userToGroupParam.GroupId);

            if (group == null)
            {
                _logger.LogError($"Group with id {userToGroupParam.GroupId} was not found!");
                return BadRequest("Invalid client request");
            }

            var userToAdd = _repository.User.FindByCondition(us => us.Id == userToGroupParam.UserId).FirstOrDefault();    

            if (userToAdd == null) {
                _logger.LogError($"User with id {userToAdd.Id} was not found!");
                return BadRequest("Invalid client request");
            }

            var currentGroupMemberships = _repository.UserToGroup.GetMembershipsForGroup(group);

            foreach (var mem in currentGroupMemberships) {
                if (mem.UserId == userToAdd.Id) {
                    _logger.LogError("User is already member of this group!");
                    return BadRequest("User is already member of this group!");
                }
            }

            if (group.IsPublic.Equals("true")) {
                var userToGroupPublic = new UserToGroup() {
                    UserId = userToAdd.Id,
                    GroupId = group.Id
                };
                _repository.UserToGroup.AddMembership(userToGroupPublic);
                _repository.Save();
                return NoContent();
            }

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            if (loggedInUser.Id != group.AdminUserId) {
                _logger.LogError("Logged in user is no admin therefore he cant add members");
                return BadRequest("Logged in user is no admin therefore he cant add members");
            }

            var userToGroup = new UserToGroup()
            {
                UserId = userToAdd.Id,
                GroupId = group.Id
            };

            _repository.UserToGroup.AddMembership(userToGroup);
            _repository.Save();
            return NoContent();
        }

        [Authorize]
        [HttpDelete, Route("remove_member")]
        public IActionResult RemoveMember([FromBody] UserToGroup userToGroup) {
            var group = _repository.Group.GetGroupById(userToGroup.GroupId);

            if (group == null)
            {
                _logger.LogError($"Group with id {userToGroup.GroupId} was not found!");
                return BadRequest("Invalid client request");
            }

            var userToRemove = _repository.User.FindByCondition(us => us.Id == userToGroup.UserId).FirstOrDefault();

            if (userToRemove == null)
            {
                _logger.LogError($"User with id {userToRemove.Id} was not found!");
                return BadRequest("Invalid client request");
            }

            bool deleted = false;

            if (group.IsPublic.Equals("true"))
            {
                var removeUserToGroup = _repository.UserToGroup.GetMembershipsForUser(userToRemove);

                foreach (var mem in removeUserToGroup) {
                    if (mem.GroupId == userToGroup.GroupId) {
                        _repository.UserToGroup.DeleteMembership(mem);
                        deleted = true;
                        break;
                    }
                }

                if (deleted) {
                    _repository.Save();
                    return NoContent();
                }

                _logger.LogError($"User is no member of this group!");
                return BadRequest("User is no member of this group!");
            }

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            if (group.AdminUserId == loggedInUser.Id) {

                var removeUserToGroup = _repository.UserToGroup.GetMembershipsForUser(userToRemove);

                foreach (var mem in removeUserToGroup)
                {
                    if (mem.GroupId == userToGroup.GroupId)
                    {
                        _repository.UserToGroup.DeleteMembership(mem);
                        deleted = true;
                    }
                }

                if (deleted) {
                    _repository.Save();
                    return NoContent();
                }
                
            }

            _logger.LogError($"Something went wrong");
            return BadRequest("Something went wrong");
        }

        [Authorize]
        [HttpGet, Route("public")]
        public IActionResult GetPublicGroups() {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            var allPublicGroups = _repository.Group.GetPublicGroups().ToList();

            var groupsOfUser = _repository.UserToGroup.GetGroupsForUser(loggedInUser).ToList();
            var resultGroups = new List<Group>();

            foreach (var availableGroup in allPublicGroups) {
                var isExisting = false;
               
                foreach (var currentUserGroup in groupsOfUser) {
                    if (currentUserGroup.Id == availableGroup.Id) {
                        isExisting = true;
                        break;
                    }
                }
                if (!isExisting) {
                    resultGroups.Add(availableGroup);
                }
            }

            return Ok(resultGroups);
        }

        [Authorize]
        [HttpGet, Route("admin_groups")]
        public IActionResult GetAdminGroups() {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            var adminGroups = _repository.Group.FindByCondition(gr => (gr.AdminUserId == loggedInUser.Id)).ToList();
            _logger.LogInfo("Hier:" + adminGroups.Count);

            if (adminGroups.Count >= 1) {
                return Ok(adminGroups);
            }

            return NoContent();      
        }
    }
}