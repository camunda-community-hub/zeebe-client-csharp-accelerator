using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client;

namespace Zeebe_Client_Accelerator_Showcase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ShowcaseController : ControllerBase
    {
        private readonly ILogger<ShowcaseController> _logger;
        private readonly IZeebeClient _zeebeClient;
        private readonly IZeebeVariablesSerializer _variablesSerializer;

        public ShowcaseController(ILogger<ShowcaseController> logger, IZeebeClient zeebeClient, IZeebeVariablesSerializer variablesSerializer)
        {
            _logger = logger;
            _zeebeClient = zeebeClient;
            _variablesSerializer = variablesSerializer;
        }

        /// <summary>
        /// Creates a new application request and thus starts a new process instance.
        /// </summary>
        /// <param name="applicationRequest">the application request</param>
        /// <returns>the business key of the process instance</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /application
        ///     {
        ///        "applicantName": "John Doe",
        ///     }
        ///
        /// </remarks>
        [HttpPost("/application")]
        public string? SubmitApplication([FromBody] ApplicationRequest applicationRequest)
        {
            var variables = new ProcessVariables()
            {
                ApplicantName = applicationRequest.ApplicantName,
                BusinessKey = "A-" + DateTime.Today.DayOfYear + "." + new Random().Next(0, 9999)
            };

            _zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId(ProcessConstants.PROCESS_DEFINITION_KEY)
                .LatestVersion()
                .Variables(_variablesSerializer.Serialize(variables))
                .Send();

            return variables.BusinessKey;
        }

    }

    public class ApplicationRequest
    {
        /// <summary>
        /// The name of the new applicant.
        /// </summary>
        [Required] public string ApplicantName { get; set; }
    }
}
