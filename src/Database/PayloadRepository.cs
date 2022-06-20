﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.Database.Interfaces;
using Monai.Deploy.WorkflowManager.Database.Options;
using Monai.Deploy.WorkflowManager.Logging.Logging;
using MongoDB.Driver;

namespace Monai.Deploy.WorkflowManager.Database
{
    public class PayloadRepository : IPayloadRepsitory
    {
        private readonly IMongoCollection<Payload> _payloadCollection;
        private readonly ILogger<PayloadRepository> _logger;

        public PayloadRepository(
            IMongoClient client,
            IOptions<WorkloadManagerDatabaseSettings> databaseSettings,
            ILogger<PayloadRepository> logger)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var mongoDatabase = client.GetDatabase(databaseSettings.Value.DatabaseName);
            _payloadCollection = mongoDatabase.GetCollection<Payload>(databaseSettings.Value.PayloadCollectionName);
        }

        public async Task<bool> CreateAsync(Payload payload)
        {
            Guard.Against.Null(payload, nameof(payload));

            try
            {
                await _payloadCollection.InsertOneAsync(payload);

                return true;
            }
            catch (Exception e)
            {
                _logger.DbCallFailed(nameof(CreateAsync), e);

                return false;
            }
        }

        public async Task<IList<Payload>> GetAllAsync()
        {
            var payloads = await _payloadCollection
                .Find(Builders<Payload>.Filter.Empty)
                .Sort(Builders<Payload>.Sort.Descending("Timestamp"))
                .ToListAsync();

            return payloads;
        }

        public async Task<Payload> GetByIdAsync(string payloadId)
        {
            Guard.Against.NullOrWhiteSpace(payloadId);

            var payload = await _payloadCollection
                .Find(x => x.PayloadId == payloadId)
                .FirstOrDefaultAsync();

            return payload;
        }
    }
}
