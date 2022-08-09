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

using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.WorkflowManager.Common.Interfaces;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkflowManager.WorkfowExecuter.Common;

namespace Monai.Deploy.WorkflowManager.MonaiBackgroundService
{
    public class Worker : BackgroundService
    {
        private const string IdentityKey = "IdentityKey";
        private readonly ILogger<Worker> _logger;
        private readonly ITasksService _tasksService;
        private readonly IMessageBrokerPublisherService _publisherService;
        private readonly IOptions<WorkflowManagerOptions> _options;
        public bool IsRunning { get; set; } = false;

        public Worker(
            ILogger<Worker> logger,
            ITasksService tasksService,
            IMessageBrokerPublisherService publisherService,
            IOptions<WorkflowManagerOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tasksService = tasksService ?? throw new ArgumentNullException(nameof(tasksService));
            _publisherService = publisherService ?? throw new ArgumentNullException(nameof(publisherService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static string ServiceName => "Monai Background Service";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IsRunning = true;
                var time = DateTimeOffset.Now;
                _logger.ServiceStarted(ServiceName);
                _logger.LogInformation("Worker running at: {time}", time);
                await DoWork(stoppingToken);
                _logger.LogInformation("Worker completed in: {time} milliseconds", (int)(DateTimeOffset.Now - time).TotalMilliseconds);
                await Task.Delay(_options.Value.BackgroundServiceSettings.BackgroundServiceDelay, stoppingToken);
            }
            _logger.ServiceStopping(ServiceName);
            IsRunning = false;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            // New Message Type cancel pluggin listener

            // Get Running Tasks
            var runningTasks = await _tasksService.GetAllAsync();
            foreach (var workflow in runningTasks.Where(t => t.Tasks.Timeout > DateTime.UtcNow))
            {
                var task = workflow.Tasks;
                // task update event -> timed out (update other events to ignore timedout tasks)

                // Timeout current task
                // Send Task Update
                task.ExecutionStats.TryGetValue(IdentityKey, out var identity);
                var workflowInstanceId = workflow.WorkflowId;
                var correlationId = Guid.NewGuid().ToString();

                await PublishTimeoutUpdateEvent(task, correlationId, workflowInstanceId).ConfigureAwait(false); // -> task manager

                await PublishCancellationEvent(task, correlationId, identity, workflowInstanceId).ConfigureAwait(false); // -> workflow executor
            }
            // submit new rabit message run handleTimeout.cancel any running argo workflows and submit argo request to run it again.

            // if task has not completed after all retries counts fail task
            // When we fails(max retries) TaskUpdate workflow manager workflow
            // -> poke argo to cancel it. (Argo terminate vs stop - choose terminate)
            // (implent genericly plugin forceCanceTask, retryTaskCode)
        }

        private async Task PublishCancellationEvent(TaskExecution task, string correlationId, string identity, string workflowInstanceId)
        {
            //issue cancelation event -> workflow executor?
            var cancellationEvent = EventMapper.GenerateTaskCancellationEvent(
                identity,
                task.ExecutionId,
                workflowInstanceId,
                task.TaskId,
                FailureReason.TimedOut,
                $"Task ({task.TaskId.Substring(0, 15)}) timed out @ {DateTime.UtcNow}");

            cancellationEvent.Validate();

            var message = EventMapper.ToJsonMessage(cancellationEvent, ServiceName, correlationId);

            await _publisherService!.Publish(_options.Value.Messaging.Topics.TaskCancellationRequest, message.ToMessage()).ConfigureAwait(false);
        }

        private async Task PublishTimeoutUpdateEvent(TaskExecution task, string correlationId, string workflowInstanceId)
        {
            var updateEvent = EventMapper.GenerateTaskUpdateEvent(new GenerateTaskUpdateEventParams
            {
                CorrelationId = correlationId,
                ExecutionId = task.ExecutionId,
                WorkflowInstanceId = workflowInstanceId,
                TaskId = task.TaskId,
                TaskExecutionStatus = TaskExecutionStatus.Failed,
                FailureReason = FailureReason.TimedOut,
                Stats = task.ExecutionStats
            });
            updateEvent.Validate();
            var message = EventMapper.ToJsonMessage(updateEvent, ServiceName, updateEvent.CorrelationId);

            await _publisherService!.Publish(_options.Value.Messaging.Topics.TaskUpdateRequest, message.ToMessage()).ConfigureAwait(false); // to task manager
        }
    }
}