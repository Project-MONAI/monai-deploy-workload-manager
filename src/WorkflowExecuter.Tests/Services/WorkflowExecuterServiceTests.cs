﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Events;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Storage;
using Monai.Deploy.Storage.Configuration;
using Monai.Deploy.WorkflowManager.Configuration;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using Monai.Deploy.WorkloadManager.WorkfowExecuter.Services;
using Moq;
using Xunit;

namespace Monai.Deploy.WorkflowManager.WorkflowExecuter.Tests.Services
{
    public class WorkflowExecuterServiceTests
    {
        private IWorkflowExecuterService WorkflowExecuterService { get; set; }

        private readonly Mock<IWorkflowRepository> _workflowRepository;
        private readonly Mock<IWorkflowInstanceRepository> _workflowInstanceRepository;
        private readonly Mock<IMessageBrokerPublisherService> _messageBrokerPublisherService;
        private readonly Mock<IStorageService> _storageService;
        private readonly IOptions<WorkflowManagerOptions> _configuration;
        private readonly IOptions<StorageServiceConfiguration> _storageConfiguration;

        public WorkflowExecuterServiceTests()
        {
            _workflowRepository = new Mock<IWorkflowRepository>();
            _workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            _messageBrokerPublisherService = new Mock<IMessageBrokerPublisherService>();
            _storageService = new Mock<IStorageService>();
            _configuration = Options.Create(new WorkflowManagerOptions() { Messaging = new MessageBrokerConfiguration { Topics = new MessageBrokerConfigurationKeys { TaskDispatchRequest = "md.task.dispatch" } } });
            _storageConfiguration = Options.Create(new StorageServiceConfiguration());

            WorkflowExecuterService = new WorkflowExecuterService(_configuration, _storageConfiguration, _workflowRepository.Object, _workflowInstanceRepository.Object, _messageBrokerPublisherService.Object, _storageService.Object);
        }

        [Fact]
        public async Task ProcessPayload_ValidAeTitleWorkflowRequest_ReturnesTrue()
        {
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var workflows = new List<Workflow>
            {
                new Workflow
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid(),
                    Revision = 1,
                    WorkflowSpec = new WorkflowSpec
                    {
                        Name = "Workflowname",
                        Description = "Workflowdesc",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetWorkflowsByAeTitleAsync(workflowRequest.CalledAeTitle)).ReturnsAsync(workflows);

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Once());

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayload_ValidWorkflowIdRequest_ReturnesTrue()
        {
            var workflowId1 = Guid.NewGuid();
            var workflowId2 = Guid.NewGuid();
            var workflowRequest = new WorkflowRequestEvent
            {
                Bucket = "testbucket",
                CalledAeTitle = "aetitle",
                CallingAeTitle = "aetitle",
                CorrelationId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Workflows = new List<string>
                {
                    workflowId1.ToString(),
                    workflowId2.ToString()
                }
            };

            var workflows = new List<Workflow>
            {
                new Workflow
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId1,
                    Revision = 1,
                    WorkflowSpec = new WorkflowSpec
                    {
                        Name = "Workflowname1",
                        Description = "Workflowdesc1",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                },
                new Workflow
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId2,
                    Revision = 1,
                    WorkflowSpec = new WorkflowSpec
                    {
                        Name = "Workflowname2",
                        Description = "Workflowdesc2",
                        Version = "1",
                        InformaticsGateway = new InformaticsGateway
                        {
                            AeTitle = "aetitle"
                        },
                        Tasks = new TaskObject[]
                        {
                            new TaskObject {
                                Id = Guid.NewGuid().ToString(),
                                Type = "type",
                                Description = "taskdesc"
                            }
                        }
                    }
                }
            };

            _workflowRepository.Setup(w => w.GetByWorkflowsIdsAsync(new List<string> { workflowId1.ToString(), workflowId2.ToString() })).ReturnsAsync(workflows);

            _workflowInstanceRepository.Setup(w => w.CreateAsync(It.IsAny<List<WorkflowInstance>>())).ReturnsAsync(true);

            var result = await WorkflowExecuterService.ProcessPayload(workflowRequest);

            _messageBrokerPublisherService.Verify(w => w.Publish(_configuration.Value.Messaging.Topics.TaskDispatchRequest, It.IsAny<Message>()), Times.Exactly(2));

            Assert.True(result);
        }
    }
}
