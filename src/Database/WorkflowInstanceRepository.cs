﻿
// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;
        private readonly ILogger<WorkflowInstanceRepository> _logger;

        public WorkflowInstanceRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
            ILogger<WorkflowInstanceRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
        }

        public async Task<IList<WorkflowInstance>> GetByWorkflowsIdsAsync(List<string> workflowIds)
        {
            Guard.Against.NullOrEmpty(workflowIds, nameof(workflowIds));

            try
            {
                var filterDef = new FilterDefinitionBuilder<WorkflowInstance>();

                var filter = filterDef.In(x => x.WorkflowId, workflowIds);
                var workflowIstances = await _workflowInstanceCollection.Find(filter).ToListAsync();

                return workflowIstances ?? new List<WorkflowInstance>();
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(GetByWorkflowsIdsAsync), e);
                return new List<WorkflowInstance>();
            }
        }

        public async Task<bool> CreateAsync(IList<WorkflowInstance> workflowInstances)
        {
            Guard.Against.NullOrEmpty(workflowInstances, nameof(workflowInstances));

            try
            {
                await _workflowInstanceCollection.InsertManyAsync(workflowInstances);
                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(CreateAsync), e);
                return false;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(string workflowInstanceId, string taskId, Status status)
        {
            Guard.Against.NullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            Guard.Against.NullOrWhiteSpace(taskId, nameof(taskId));
            Guard.Against.Null(status, nameof(status));

            try
            {
                var result = await _workflowInstanceCollection.FindOneAndUpdateAsync(
                    i => i.Id == workflowInstanceId && i.Tasks.Any(t => t.TaskId == taskId),
                    Builders<WorkflowInstance>.Update.Set(w => w.Tasks[-1].Status, status));
                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(CreateAsync), e);
                return false;
            }
        }
    }
}
