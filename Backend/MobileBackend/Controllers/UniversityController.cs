﻿using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mobile_Backend.Extensions;
using System;

namespace Mobile_Backend.Controllers
{
    [Route("api/uni")]
    public class UniversityController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IEmailSender _emailSender;

        public UniversityController(ILoggerManager logger, IRepositoryWrapper repository, IEmailSender emailSender)
        {
            _logger = logger;
            _repository = repository;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult GetUniversities()
        {
            try
            {
                var listUniversities = _repository.University.GetUniversities();
                return Ok(listUniversities);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside GetUniversities: {e.Message}");
                return StatusCode(500, "Something went wrong while getting the universities");
            }
        }

        [Authorize]
        [HttpGet, Route("get_all_students")]
        public IActionResult GetAllStudents()
        {
            var userMail = AuthControllerExtensions.JwtNameExtractor(Request.Headers["Authorization"]);
            var dbUser = _repository.User.GetUserByEmail(userMail);

            var allStudents = _repository.User.FindByCondition(u => u.University_Id.Equals(dbUser.University_Id));

            return Ok(allStudents);
        }
    }
}