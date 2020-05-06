using System;
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
        [HttpGet]
        [Route("group/{groupId}")]
        [Route("group/{groupId}/{since?}")]
        public IActionResult GetGroupDiscussionEntries(int groupId, DateTime? since)
        {
            try
            {
                var entries = _repository.DiscussionEntry.GetGroupDiscussionEntries(groupId, since).ToList();
                return Ok(entries);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetGroupDiscussionEntries: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetGroupDiscussionEntries");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("sub_group/{groupId}")]
        [Route("sub_group/{groupId}/{since?}")]
        public IActionResult GetSubGroupDiscussionEntries(int groupId, DateTime? since)
        {
            try
            {
                var entries = _repository.DiscussionEntry.GetSubgroupDiscussionEntries(groupId, since).ToList();
                return Ok(entries);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong while GetSubGroupDiscussionEntries: {e.Message}");
                return StatusCode(500, $"Something went wrong while getting GetSubGroupDiscussionEntries");
            }
        }

        [Authorize]
        [HttpGet, Route("new/group/{groupId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetNewGroupDiscussionEntryAsync(int groupId)
        {
            try
            {
                var lp = new SimpleLongPolling($"group{groupId}");
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
        [HttpGet, Route("new/sub_group/{groupId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetNewSubgroupDiscussionEntryAsync(int groupId)
        {
            try
            {
                var lp = new SimpleLongPolling($"subgroup{groupId}");
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
        [HttpPost]
        [Route("group/file/{groupId?}")]
        [Route("sub_group/file/{subGroupId?}")]
        public async System.Threading.Tasks.Task<IActionResult> UploadFileAsync(List<IFormFile> files, int? groupId, int? subGroupId)
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

            if (!groupId.HasValue && !subGroupId.HasValue)
            {
                _logger.LogError($"No Id in URL");
                return BadRequest("No Id in URL");
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

                if (groupId.HasValue)
                {
                    f.NormalGroup = groupId.Value;
                }
                else if (subGroupId.HasValue)
                {
                    f.Subgroup = subGroupId.Value;
                }

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
    }
}