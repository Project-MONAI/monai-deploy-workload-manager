﻿using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.PayloadListener.Extensions;

namespace Monai.Deploy.WorkflowManager.PayloadListener.Validators
{
    public class EventPayloadValidator : IEventPayloadValidator
    {
        private ILogger<EventPayloadValidator> Logger { get; }

        public EventPayloadValidator(ILogger<EventPayloadValidator> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateWorkflowRequest(WorkflowRequestEvent payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            var valid = true;
            var payloadValid = payload.IsValid(out var validationErrors);

            if (!payloadValid)
            {
                Logger.ValidationErrors(string.Join(Environment.NewLine, validationErrors));
            }

            valid &= payloadValid;

            foreach (var workflow in payload.Workflows)
            {
                Guard.Against.Null(workflow, nameof(workflow));

                var workflowValid = !string.IsNullOrEmpty(workflow);

                if (!workflowValid)
                {
                    Logger.ValidationErrors("Workflow is null or empty");
                }

                valid &= workflowValid;
            }

            return valid;
        }
    }
}
