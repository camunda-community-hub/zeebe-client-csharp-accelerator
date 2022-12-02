using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe_Client_Accelerator_Showcase.Worker
{
    [JobType("accountService")]
    [FetchVariables("applicantName")] // fetches only the variable 'applicantName' - not the 'businessKey'
    public class AccountServiceWorker : IAsyncZeebeWorker
    {
        private readonly ILogger<AccountServiceWorker> _logger;

        public AccountServiceWorker(ILogger<AccountServiceWorker> logger)
        {
            _logger = logger;
        }

        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            // get process variables
            ProcessVariables variables = job.getVariables<ProcessVariables>();
            // get custom headers
            AccountServiceHeaders headers = job.getCustomHeaders<AccountServiceHeaders>();

            // call the account service adapter
            _logger.LogInformation("Do {action} Account for {applicantName}", headers.Action, variables.ApplicantName);

            // done
            return Task.CompletedTask;
        }
    }

    class AccountServiceHeaders
    {
        public string Action { get; set; }
    }

}