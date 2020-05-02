﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Contracts;
using Entities.Extensions;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mobile_Backend.Extensions;
using Mobile_Backend.Helper;
using File = Entities.Models.File;
using NLog;

namespace Mobile_Backend.Controllers
{
    [Route("api/discussion")]
    public class DiscussionController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;
        private IConfiguration _config;

        public DiscussionController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender, IConfiguration config)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
            _config = config;
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
                entry.TimeStamp = DateTime.Now;
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);
                entry.UserId = dbUser.Id;

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
        public IActionResult GetGroupDiscussionEntries([FromBody]DiscussionEntryRequest discussionEntryRequest)
        {

            if (discussionEntryRequest == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                DateTime? since = null;
                LastGroupFetch lastFetch = null;
                if (discussionEntryRequest.SinceLastFetch)
                {
                    lastFetch = _repository.LastGroupFetch.Get(dbUser.Id, discussionEntryRequest.GroupId);
                    if (lastFetch != null)
                    {
                        since = lastFetch.TimeStamp;
                    }
                }
                
                var entries = _repository.DiscussionEntry.GetGroupDiscussionEntries(discussionEntryRequest.GroupId, since).ToList();
                MakeParseable(entries);

                if (lastFetch == null)
                {
                    lastFetch = new LastGroupFetch();
                    lastFetch.UserId = dbUser.Id;
                    lastFetch.GroupId = discussionEntryRequest.GroupId;
                }

                lastFetch.TimeStamp = DateTime.Now;
                _repository.LastGroupFetch.PostOrUpdate(lastFetch);
                _repository.Save();

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
        public IActionResult GetSubGroupDiscussionEntries([FromBody]DiscussionEntryRequest discussionEntryRequest)
        {

            if (discussionEntryRequest == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
                var dbUser = _repository.User.GetUserByEmail(userMail);

                DateTime? since = null;
                LastSubGroupFetch lastFetch = null;
                if (discussionEntryRequest.SinceLastFetch)
                {
                    lastFetch = _repository.LastSubGroupFetch.Get(dbUser.Id, discussionEntryRequest.GroupId);
                    if (lastFetch != null)
                    {
                        since = lastFetch.TimeStamp;
                    }
                }

                var entries = _repository.DiscussionEntry.GetSubgroupDiscussionEntries(discussionEntryRequest.GroupId, since).ToList();

                if (lastFetch == null)
                {
                    lastFetch = new LastSubGroupFetch();
                    lastFetch.UserId = dbUser.Id;
                    lastFetch.SubGroupId = discussionEntryRequest.GroupId;
                }

                lastFetch.TimeStamp = DateTime.Now;
                _repository.LastSubGroupFetch.PostOrUpdate(lastFetch);
                _repository.Save();

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

        [Authorize]
        [HttpPost, Route("file")]
        public async System.Threading.Tasks.Task<IActionResult> UploadFileAsync(List<IFormFile> files)
        {
            if (files == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }

            if (files.Count > 1)
            {
                _logger.LogError($"Invalid client request: sent multiple files");
                return BadRequest("Invalid client request: sent multiple files");
            }
            try
            {
                var file = files.First();
                var fileName = Path.GetRandomFileName();
                var filePath = Path.Combine(_config["StoredFilesPath"], fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var f = new Entities.Models.File
                {
                    Path = fileName
                };
                _repository.File.PostFile(f);

                return Ok(f);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while UploadFileAsync: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting UploadFileAsync");
            }
        }

        [Authorize]
        [HttpPatch, Route("file")]
        public IActionResult UpdateFile(File file)
        {
            if (file == null)
            {
                _logger.LogError($"Invalid client request: object was null");
                return BadRequest("Invalid client request: object was null");
            }
            try
            {
                _repository.File.UpdateFile(file);
                return Ok(file);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while UploadFileAsync: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting UploadFileAsync");
            }
        }

        [Authorize]
        [HttpGet, Route("file/{id}")]
        public ActionResult Download(string id)
        {
            try
            {
                var stream = System.IO.File.OpenRead(_config["StoredFilesPath"] + '/' + id);
                return new FileStreamResult(stream, "application/octet-stream");
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while fetching file: {e.Message}");

                return NotFound();
            }
        }

        private void MakeParseable(IEnumerable<DiscussionEntry> discussionEntries)
        {
            foreach(DiscussionEntry entry in discussionEntries)
            {
                _logger.LogInfo($"answers {entry.Answers.Count} {entry.Answers.ToString()}");
                //entry.ParentDiscussionEntry = null;
                if (entry.Answers != null)
                {
                    MakeParseable(entry.Answers);
                }
            }
        }
    }
}