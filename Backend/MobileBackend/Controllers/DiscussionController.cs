﻿using System;
using System.Linq;
using Contracts;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mobile_Backend.Extensions;
using Mobile_Backend.Helper;

namespace Mobile_Backend.Controllers
{
    [Route("api/discussion")]
    public class DiscussionController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;

        public DiscussionController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateDiscussionEntry([FromBody]DiscussionEntry entry)
        {

            if (entry == null)
            {
                _logger.LogError("Invalid object: entry was null");
                return BadRequest("Invalid client request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid DiscussionEntry object sent from client.");
                return BadRequest("Invalid DiscussionEntry object sent from client.");
            }

            try
            {
                _repository.DiscussionEntry.PostDiscussion(entry);
                _repository.Save();

                if (entry.Subgroup != null)
                {
                    SimpleLongPolling.Publish($"subgroup{entry.Subgroup.Value}", entry.Id);
                }
                else if (entry.NormalGroup != null)
                {
                    SimpleLongPolling.Publish($"group{entry.NormalGroup.Value}", entry.Id);
                }

                return Ok(_repository.DiscussionEntry.GetDiscussionEntryById(entry.Id));
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside CreateDiscussionEntry: {e.Message}");
                return StatusCode(500, "Something went wrong during creating discussion entry");
            }
        }

        [Authorize]
        [HttpGet, Route("group")]
        public IActionResult GetGroupDiscussionEntries([FromBody]int? groupId)
        {

            if (groupId == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var entries = _repository.DiscussionEntry.GetGroupDiscussionEntries(groupId.Value).ToList();

                return Ok(entries);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetGroupDiscussionEntries: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetGroupDiscussionEntries");
            }
        }

        [Authorize]
        [HttpGet, Route("sub_group")]
        public IActionResult GetSubGroupDiscussionEntries([FromBody]int? groupId)
        {

            if (groupId == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var entries = _repository.DiscussionEntry.GetSubgroupDiscussionEntries(groupId.Value).ToList();

                return Ok(entries);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetSubGroupDiscussionEntries: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetSubGroupDiscussionEntries");
            }
        }

        [Authorize]
        [HttpGet, Route("new/group")]
        public async System.Threading.Tasks.Task<IActionResult> GetNewGroupDiscussionEntryAsync([FromBody]int? groupId)
        {

            if (groupId == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var lp = new SimpleLongPolling($"group{groupId.Value}");
                var id = await lp.WaitAsync();

                var entry = _repository.DiscussionEntry.GetDiscussionEntryById(id);

                return Ok(entry);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetNewGroupDiscussionEntryAsync: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetNewGroupDiscussionEntryAsync");
            }
        }

        [Authorize]
        [HttpGet, Route("new/sub_group")]
        public async System.Threading.Tasks.Task<IActionResult> GetNewSubgroupDiscussionEntryAsync([FromBody]int? groupId)
        {

            if (groupId == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var lp = new SimpleLongPolling($"subgroup{groupId.Value}");
                var id = await lp.WaitAsync();

                var entry = _repository.DiscussionEntry.GetDiscussionEntryById(id);

                return Ok(entry);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetNewSubgroupDiscussionEntryAsync: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetNewSubgroupDiscussionEntryAsync");
            }
        }
    }
}