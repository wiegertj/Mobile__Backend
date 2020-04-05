using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;

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
            var listUniversities = _repository.University.GetUniversities();

            return Ok(listUniversities);
        }
    }
}