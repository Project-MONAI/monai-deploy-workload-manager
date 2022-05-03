﻿
// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using Monai.Deploy.WorkloadManager.Contracts.Models;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly IMongoClient _client;
        private readonly IMongoCollection<WorkflowInstance> _workflowInstanceCollection;
        private readonly ILogger<WorkflowInstanceRepository> _logger;

        public WorkflowInstanceRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> bookStoreDatabaseSettings,
            ILogger<WorkflowInstanceRepository> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
            _workflowInstanceCollection = mongoDatabase.GetCollection<WorkflowInstance>(bookStoreDatabaseSettings.Value.WorkflowInstanceCollectionName);
        }

        public async Task<bool> CreateAsync(IList<WorkflowInstance> workflowInstances)
        {
            using var session = await _client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                await _workflowInstanceCollection.InsertManyAsync(workflowInstances);
                await session.CommitTransactionAsync();

                return true;
            }
            catch (Exception e)
            {
                _logger.TransactionFailed(nameof(CreateAsync), e);
                await session.AbortTransactionAsync();
                return false;
            }
        }
    }
}
