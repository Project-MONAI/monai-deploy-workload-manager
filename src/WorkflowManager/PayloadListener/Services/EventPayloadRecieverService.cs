/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.Extensions.Logging;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Interfaces;
using Monai.Deploy.WorkflowManager.Common.Logging;
using Monai.Deploy.WorkflowManager.Common.PayloadListener.Validators;
using Monai.Deploy.WorkflowManager.Common.WorkfowExecuter.Services;
using Monai.Deploy.WorkflowManager.Logging;
using Log = Monai.Deploy.WorkflowManager.Logging.Log;

namespace Monai.Deploy.WorkflowManager.Common.PayloadListener.Services
{
    public class EventPayloadReceiverService : IEventPayloadReceiverService
    {
        public EventPayloadReceiverService(
            ILogger<EventPayloadReceiverService> logger,
            IEventPayloadValidator payloadValidator,
            IMessageBrokerSubscriberService messageBrokerSubscriberService,
            IWorkflowExecuterService workflowExecuterService,
            IPayloadService payloadService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PayloadValidator = payloadValidator ?? throw new ArgumentNullException(nameof(payloadValidator));
            _messageSubscriber = messageBrokerSubscriberService ?? throw new ArgumentNullException(nameof(messageBrokerSubscriberService));
            WorkflowExecuterService = workflowExecuterService ?? throw new ArgumentNullException(nameof(workflowExecuterService));
            PayloadService = payloadService ?? throw new ArgumentNullException(nameof(payloadService));
        }

        private IEventPayloadValidator PayloadValidator { get; }

        private IWorkflowExecuterService WorkflowExecuterService { get; }

        private IPayloadService PayloadService { get; }

        private ILogger<EventPayloadReceiverService> Logger { get; }

        private readonly IMessageBrokerSubscriberService _messageSubscriber;

        public async Task ReceiveWorkflowPayload(MessageReceivedEventArgs message)
        {
            try
            {
                var requestEvent = message.Message.ConvertTo<WorkflowRequestEvent>();

                using var loggingScope = (Logger.BeginScope(new LoggingDataDictionary<string, object>
                {
                    ["correlationId"] = requestEvent.CorrelationId,
                    ["workflowId"] = requestEvent.Workflows.FirstOrDefault()
                }));

                var validation = PayloadValidator.ValidateWorkflowRequest(requestEvent);

                if (!validation)
                {
                    Log.WorkflowRequestRejectValidationError(Logger, message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                var payload = await PayloadService.CreateAsync(requestEvent);
                if (payload is null)
                {
                    Log.WorkflowRequestRequeuePayloadCreateError(Logger, message.Message.MessageId);
                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                if (!await WorkflowExecuterService.ProcessPayload(requestEvent, payload))
                {
                    Log.WorkflowRequestRequeuePayloadProcessError(Logger, message.Message.MessageId);
                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Log.WorkflowRequestRequeueUnknownError(Logger, message.Message.MessageId, e);
                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }

        public async Task TaskUpdatePayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<TaskUpdateEvent>();

                using var loggerScope = Logger.BeginScope(new LoggingDataDictionary<string, object>
                {
                    ["correlationId"] = payload.CorrelationId,
                    ["workflowInstanceId"] = payload.WorkflowInstanceId,
                    ["taskId"] = payload.TaskId
                });

                if (!PayloadValidator.ValidateTaskUpdate(payload))
                {
                    Log.TaskUpdateRejectValiationError(Logger, message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                var processTaskUpdateResult = await WorkflowExecuterService.ProcessTaskUpdate(payload);
                if (!processTaskUpdateResult && payload.Reason != FailureReason.TimedOut)
                {
                    Log.TaskUpdateRequeueProcessingError(Logger, message.Message.MessageId);
                    await _messageSubscriber.RequeueWithDelay(message.Message);
                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Log.TaskUpdateRequeueUnknownError(Logger, message.Message.MessageId, e);
                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }

        public async Task ExportCompletePayload(MessageReceivedEventArgs message)
        {
            try
            {
                var payload = message.Message.ConvertTo<ExportCompleteEvent>();

                using var loggerScope = Logger.BeginScope(new LoggingDataDictionary<string, object> { ["workflowInstanceId"] = payload.WorkflowInstanceId });

                if (!PayloadValidator.ValidateExportComplete(payload))
                {
                    Log.ExportCompleteRejectValiationError(Logger, message.Message.MessageId);
                    _messageSubscriber.Reject(message.Message, false);

                    return;
                }

                if (!await WorkflowExecuterService.ProcessExportComplete(payload, message.Message.CorrelationId))
                {
                    Log.ExportCompleteRequeueProcessingError(Logger, message.Message.MessageId);

                    await _messageSubscriber.RequeueWithDelay(message.Message);

                    return;
                }

                _messageSubscriber.Acknowledge(message.Message);
            }
            catch (Exception e)
            {
                Log.ExportCompleteRequeueUnknownError(Logger, message.Message.MessageId, e);
                await _messageSubscriber.RequeueWithDelay(message.Message);
            }
        }
    }
}
