using System;
using System.Collections.Generic;
using System.Linq;
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
                return BadRequest("Invalid client request: group object was null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user: model state not valid");
                return BadRequest("Invalid user: model state not valid");
            }

            if (!group.ValidateCreateGroup())
            {
                return BadRequest("Sent object was not valid!");
            }

            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                group.AdminUserId = dbUser.Id;

                _repository.Group.CreateGroup(group);
                _repository.Save();

                var adminMembership = new UserToGroup()
                {
                    GroupId = group.Id,
                    UserId = dbUser.Id
                };

                _repository.UserToGroup.AddMembership(adminMembership);
                _repository.Save();

                return Ok(_repository.Group.GetGroupById(group.Id));
            }
            catch (Exception e) {
                _logger.LogError($"Something went wrong inside CreateGroup: {e.Message}");
                return BadRequest("Something went wrong while creating group, the group was not saved!");
            }
        }

        [Authorize]
        [HttpDelete, Route("delete/{id}")]
        public IActionResult DeleteGroup(long id)
        {

            var group = _repository.Group.GetGroupById(id);

            if (group == null)
            {
                _logger.LogError("Group to delete was not found!");
                return BadRequest("Invalid client request, group was not found!");
            }

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var dbUser = _repository.User.GetUserByEmail(userMail);

            if (!(group.AdminUserId == dbUser.Id))
            {
                _logger.LogError("Only admin can delte his groups!");
                return BadRequest("Only group admin can delete this group!");
            }

            try
            {
                var listOfMembers = _repository.UserToGroup.GetMembershipsForGroup(group).ToList();
                var subgroups = _repository.Subgroup.GetSubgroupsForGroup(group.Id).ToList();

                foreach (var subgroup in subgroups)
                {
                    var memberships = _repository.UserToSubgroup.GetMembershipsForSubgroup(subgroup).ToList();

                    foreach (var mem in memberships)
                    {
                        _repository.UserToSubgroup.DeleteMembership(mem);
                        _repository.Save();
                    }

                    _repository.Subgroup.DeleteGroup(subgroup);
                    _repository.Save();
                }

                foreach (var mem in listOfMembers)
                {
                    _repository.UserToGroup.DeleteMembership(mem);
                }

                _repository.Save();
                _repository.Group.DeleteGroup(group);
                _repository.Save();

                return NoContent();
            }
            catch(Exception e)
            {
                _logger.LogError($"Something went wrong inside DeleteGroup: {e.Message}");
                return StatusCode(500, "Something went wrong during deleting the group");
            }
        }

        [Authorize]
        [HttpPut, Route("update")]
        public IActionResult UpdateGroup([FromBody]Group group) {

            if (group == null)
            {
                return BadRequest("Invalid client request: Group object was null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid group object sent from client.");
                return BadRequest("Invalid group object sent from client.");
            }

            if (!group.ValidateChangeGroup())
            {
                return BadRequest("Sent object was not valid!");
            }

            try
            {
                var dbGroup = _repository.Group.GetGroupById(group.Id);

                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                if (dbUser.Id == dbGroup.AdminUserId)
                {
                    dbGroup.Map(group);
                    _repository.Group.Update(dbGroup);
                    _repository.Save();
                    return NoContent();
                }

                _logger.LogError("Only admin can change group");
                return BadRequest("Only admin can change group");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside UpdateGroup: {e.Message}");
                return StatusCode(500, "Internal Server Error while updating the group");
            }
        }

        [Authorize]
        [HttpGet, Route("members/{id}")]
        public IActionResult GetMembers(long id)
        {
            var group = _repository.Group.GetGroupById(id);

            if (group == null)
            {
                _logger.LogError($"Group was not found!");
                return BadRequest("Group was not found!");
            }

            try
            {
                var listMembers = _repository.UserToGroup.GetMembersForGroup(group).ToList();
                return Ok(listMembers);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetMembers: {e.Message}");
                return StatusCode(500, "Internal Server Error while getting the members for the group");
            }
        }

        [Authorize]
        [HttpPost, Route("add_member")]
        public IActionResult AddMember([FromBody] UserToGroup userToGroupParam) {
            var group = _repository.Group.GetGroupById(userToGroupParam.GroupId);

            if (group == null)
            {
                _logger.LogError("Group was not found!");
                return BadRequest("Invalid client request: Group was not found");
            }

            var userToAdd = _repository.User.FindByCondition(us => us.Id == userToGroupParam.UserId).FirstOrDefault();    

            if (userToAdd == null) {
                _logger.LogError("User with this id was not found!");
                return BadRequest("User with this id was not found!");
            }

            var currentGroupMemberships = _repository.UserToGroup.GetMembershipsForGroup(group).ToList();

            foreach (var mem in currentGroupMemberships) {
                if (mem.UserId == userToAdd.Id) {
                    _logger.LogError("User is already member of this group!");
                    return BadRequest("User is already member of this group!");
                }
            }

            try
            {
                if (group.IsPublic.Equals("true"))
                {
                    var userToGroupPublic = new UserToGroup()
                    {
                        UserId = userToAdd.Id,
                        GroupId = group.Id
                    };
                    _repository.UserToGroup.AddMembership(userToGroupPublic);
                    _repository.Save();
                    return NoContent();
                }

                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var loggedInUser = _repository.User.GetUserByEmail(userMail);

                if (loggedInUser.Id != group.AdminUserId)
                {
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
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside AddMember: {e.Message}");
                return StatusCode(500, "Something went wrong while adding a new member");
            }
        }

        [Authorize]
        [HttpDelete, Route("remove_member/{userId}/{groupId}")]
        public IActionResult RemoveMember(long userId, long groupId) {

            var group = _repository.Group.GetGroupById(groupId);
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            if (group == null)
            {
                _logger.LogError("Group with was not found!");
                return BadRequest("Group with was not found!");
            }

            var userToRemove = _repository.User.FindByCondition(us => us.Id == userId).FirstOrDefault();

            if (userToRemove == null)
            {
                _logger.LogError("User with was not found!");
                return BadRequest("User with was not found!");
            }

            // Admin cant be deleted from its group
            if(userToRemove.Id == group.AdminUserId)
            {
                _logger.LogError($"User with id {userToRemove.Id} is admin and cant be removed!");
                return BadRequest($"User with id {userToRemove.Id} is admin and cant be removed!");
            }

            try
            {
                // Delete all subgroups if existing
                if ((group.AdminUserId == loggedInUser.Id) || (loggedInUser.Id == userToRemove.Id))
                {
                    var listSubgroups = _repository.Subgroup.GetSubgroupsForGroup(groupId).ToList();

                    foreach (var sgr in listSubgroups)
                    {
                        var memberships = _repository.UserToSubgroup.GetMembershipsForSubgroup(sgr).ToList();
                        foreach (var mem in memberships)
                        {
                            if (mem.UserId.Equals(userToRemove.Id))
                            {
                                _repository.UserToSubgroup.DeleteMembership(mem);
                                _repository.Save();
                            }
                        }
                    }

                    var removeUserToGroup = _repository.UserToGroup.GetMembershipsForUser(userToRemove);
                    bool deleted = false;

                    foreach (var mem in removeUserToGroup)
                    {
                        if (mem.GroupId == groupId)
                        {
                            _repository.UserToGroup.DeleteMembership(mem);
                            deleted = true;
                            break;
                        }
                    }

                    if (deleted)
                    {
                        _repository.Save();
                        return NoContent();
                    }

                    _logger.LogError($"User is no member of this group!");
                    return BadRequest("User is no member of this group!");
                }

                _logger.LogError("Only admins or members themself can delete their membership!");
                return BadRequest("Only admins or members themself can delete their membership!");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside RemoveMember: {e.Message}");
                return StatusCode(500, "Something went wrong during removing the membership");
            }
        }

        [Authorize]
        [HttpGet, Route("public")]
        public IActionResult GetPublicGroups() {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            try
            {
                var allPublicGroups = _repository.Group.GetPublicGroups().ToList();
                var groupsOfUser = _repository.UserToGroup.GetGroupsForUser(loggedInUser).ToList();
                var resultGroups = new List<Group>();

                foreach (var availableGroup in allPublicGroups)
                {
                    var isExisting = false;

                    foreach (var currentUserGroup in groupsOfUser)
                    {
                        if (currentUserGroup.Id == availableGroup.Id)
                        {
                            isExisting = true;
                            break;
                        }
                    }
                    if (!isExisting)
                    {
                        resultGroups.Add(availableGroup);
                    }
                }

                return Ok(resultGroups);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetPublicGroups: {e.Message}");
                return StatusCode(500, "Something went wrong during getting public groups");
            }
        }

        [Authorize]
        [HttpGet, Route("admin_groups")]
        public IActionResult GetAdminGroups() {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);

            try
            {
                var adminGroups = _repository.Group.FindByCondition(gr => (gr.AdminUserId == loggedInUser.Id)).ToList();

                if (adminGroups.Count >= 1)
                {
                    return Ok(adminGroups);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetAdminGroups: {e.Message}");
                return StatusCode(500, "Something went wrong during getting all admin groups");
            }
        }

        [Authorize]
        [HttpGet, Route("get_user_groups")]
        public IActionResult GetAllGroupsForUser()
        {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var loggedInUser = _repository.User.GetUserByEmail(userMail);
            
            try
            {
                
                var myGroups = _repository.UserToGroup.GetGroupsForUser(loggedInUser).ToList();

                foreach (var group in myGroups)
                {
                    if (group.AdminUserId.Equals(loggedInUser.Id))
                    {
                        group.IsAdmin = true;
                    }
                    else
                    {
                        group.IsAdmin = false;
                    }
                }

                var mySubgroups = _repository.UserToSubgroup.GetSubgroupsForUser(loggedInUser).ToList();

                foreach (var subgroup in mySubgroups)
                {
                    foreach(var group in myGroups)
                    {
                        if (subgroup.Main_group.Equals(group.Id))
                        {
                            group.Subgroups.Add(subgroup);
                            break;
                        }
                    }
                }

                return Ok(myGroups);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetAllGroupsForUser: {e.Message}");
                return StatusCode(500, "Something went wrong during getting all groups for logged in user");
            }
        }
    }
}