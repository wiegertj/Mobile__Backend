using System;
using System.Linq;
using Contracts;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mobile_Backend.Extensions;

namespace Mobile_Backend.Controllers
{
    [Route("api/subgroup")]
    public class SubgroupController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;

        public SubgroupController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
        }

        [Authorize]
        [HttpPost, Route("create")]
        public IActionResult CreateSubgroup([FromBody]Subgroup subgroup)
        {

            if (subgroup == null)
            {
                _logger.LogError("Invalid object: subgroup was null");
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid subgroup object sent from client.");
                return BadRequest("Invalid subgroup object sent from client.");
            }

            if (!subgroup.ValidateCreateSubgroup())
            {
                return BadRequest("Sent object was not valid!");
            }

            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                var mainGroup = _repository.Group.GetGroupById(subgroup.Main_group);

                if (mainGroup.AdminUserId.Equals(dbUser.Id))
                {
                    _repository.Subgroup.Create(subgroup);
                    _repository.Save();

                    return Ok(_repository.Subgroup.GetSubgroupById(subgroup.Id));
                }

                return BadRequest("Only admin can add a subgroup to a group!");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside CreateSubgroup: {e.Message}");
                return StatusCode(500, "Something went wrong during creating subgroup");
            }
        }

        [Authorize]
        [HttpDelete, Route("delete/{id}")]
        public IActionResult DeleteSubgroup(long id)
        {
            var subgroup = _repository.Subgroup.GetSubgroupById(id);

            if (subgroup == null)
            {
                _logger.LogError("Invalid object: subgroup was null");
                return BadRequest("Subgroup with this id is not existing!");
            }

            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                var mainGroup = _repository.Group.GetGroupById(subgroup.Main_group);

                if (mainGroup.AdminUserId.Equals(dbUser.Id))
                {
                    var listOfMembers = _repository.UserToSubgroup.GetMembershipsForSubgroup(subgroup);

                    foreach (var mem in listOfMembers)
                    {
                        _repository.UserToSubgroup.DeleteMembership(mem);
                    }

                    _repository.Save();
                    _repository.Subgroup.Delete(subgroup);
                    _repository.Save();

                    return NoContent();
                }

                return BadRequest("Only admin of main group can delete subgroup");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside DelteSubgroup {e.Message}");
                return StatusCode(500, $"Something went wrong during deleting the subgroup");
            }
        }

        [Authorize]
        [HttpPut, Route("update")]
        public IActionResult UpdateSubgroup([FromBody]Subgroup subgroup)
        {
            if (subgroup == null)
            {
                _logger.LogError("Invalid request: Subgroup object was null");
                return BadRequest("Invalid request: Subgroup object was null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client");
                return BadRequest("Invalid user object sent from client");
            }

            if (!subgroup.ValidateUpdateSubgroup())
            {
                return BadRequest("Sent object was not valid!");
            }

            try
            {
                var dbSubgroup = _repository.Subgroup.GetSubgroupById(subgroup.Id);
                var mainGroup = _repository.Group.GetGroupById(subgroup.Main_group);

                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                if (dbUser.Id.Equals(mainGroup.AdminUserId))
                {
                    dbSubgroup.Map(subgroup);
                    _repository.Subgroup.Update(dbSubgroup);
                    _repository.Save();
                    return NoContent();
                }

                _logger.LogError("Only admin can change subgroup");
                return BadRequest("Only admin can change subgroup");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside UpdateSubgroup: {e.Message}");
                return StatusCode(500, "Something went wrong during update subgroup");
            }
        }

        [Authorize]
        [HttpPost, Route("add_member")]
        public IActionResult AddMembers([FromBody]UserToSubgroup userToSubgroup)
        {

            if (userToSubgroup == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid client request: object was invalid");
                return BadRequest("Invalid client request: object was invalid");
            }

            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                var subgroup = _repository.Subgroup.GetSubgroupById(userToSubgroup.SubgroupId);

                if (subgroup == null)
                {
                    _logger.LogError("The subgroup was not found!");
                    return BadRequest("The subgroup was not found!");
                }

                var mainGroup = _repository.Group.GetGroupById(subgroup.Main_group);
                var allMainGroupMembers = _repository.UserToGroup.GetMembersForGroup(mainGroup);

                var userIsMemberOfGroup = false;

                foreach (var user in allMainGroupMembers)
                {
                    if (user.Id.Equals(dbUser.Id))
                    {
                        userIsMemberOfGroup = true;
                        break;
                    }
                }

                if (!userIsMemberOfGroup)
                {
                    return BadRequest("User is not member of group so user cant be member of subgroup");
                }

                var allSubgroupMembers = _repository.UserToSubgroup.GetMembersForSubgroup(subgroup);

                var userIsMemberOfSubgroup = false;

                foreach (var user in allSubgroupMembers)
                {
                    if (user.Id.Equals(dbUser.Id))
                    {
                        userIsMemberOfSubgroup = true;
                        break;
                    }
                }

                if (userIsMemberOfSubgroup)
                {
                    return BadRequest("User is already member of subgroup");
                }

                UserToSubgroup _userToSubgroup = new UserToSubgroup()
                {
                    SubgroupId = subgroup.Id,
                    UserId = dbUser.Id
                };

                _repository.UserToSubgroup.Create(_userToSubgroup);
                _repository.Save();
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside AddMembers {e.Message}");
                return StatusCode(500, $"Something went wrong while saving new subgroup member");
            }
        }

        [Authorize]
        [HttpDelete, Route("remove_member/{id}")]
        public IActionResult RemoveSubgroupMember(long id)
        {
            var userToSubgroup = new UserToSubgroup
            {
                SubgroupId = id
            };

            if (userToSubgroup == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }

            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var dbUser = _repository.User.GetUserByEmail(userMail);

            try
            {
                var subgroup = _repository.Subgroup.GetSubgroupById(userToSubgroup.SubgroupId);
                var allSubgroupMembers = _repository.UserToSubgroup.GetMembersForSubgroup(subgroup).ToList();
                var isMember = false;

                foreach (var mem in allSubgroupMembers)
                {
                    if (mem.Id.Equals(dbUser.Id))
                    {
                        isMember = true;
                        break;
                    }
                }

                if (isMember)
                {
                    var allUserToSubgroups = _repository.UserToSubgroup.GetMembershipsForSubgroup(subgroup).ToList();

                    foreach (var ms in allUserToSubgroups)
                    {
                        if (ms.UserId.Equals(dbUser.Id))
                        {
                            _repository.UserToSubgroup.DeleteMembership(ms);
                            _repository.Save();

                            return NoContent();
                        }
                    }
                }

                return BadRequest("User is not member of this subgroup");
            }

            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside RemoveSubgroupMember: {e.Message}");
                return StatusCode(500, $"Something went wrong while deleting subgroup member");
            }
        }

        [Authorize]
        [HttpGet, Route("members/{id}")]
        public IActionResult GetMembersSubgroup(long id)
        {
            var subgroup = _repository.Subgroup.GetSubgroupById(id);

            if(subgroup == null)
            {
                return BadRequest("Subgroup with this id was not found!");
            }

            try
            {
                var listMembers = _repository.UserToSubgroup.GetMembersForSubgroup(subgroup).ToList();
                return Ok(listMembers);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetMembersSubgroup: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting subgroup members");
            }
        }

        [Authorize]
        [HttpGet, Route("all_subgroups/{id}")]
        public IActionResult GetAllSubgroups(long id)
        {

            var group = _repository.Group.GetGroupById(id);

            if (group == null)
            {
                _logger.LogError($"Group was not found!");
                return BadRequest($"Group was not found!");
            }

            try
            {
                var allSubgroups = _repository.Subgroup.GetSubgroupsForGroup(group.Id);
                return Ok(allSubgroups);
            }

            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetAllSubgroups: {e.Message}");
                return StatusCode(500, "Something went wrong during getting all subgroups!");
            }
        }
    }
}