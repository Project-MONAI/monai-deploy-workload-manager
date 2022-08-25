/*
 * Copyright 2021-2022 MONAI Consortium
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

using Monai.Deploy.WorkflowManager.Contracts.Models;
using Monai.Deploy.WorkflowManager.TaskManager.API.Models;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace Monai.Deploy.WorkflowManager.IntegrationTests
{
    public class MongoClientUtil
    {
        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }
        private IMongoCollection<WorkflowRevision> WorkflowRevisionCollection { get; set; }
        private IMongoCollection<WorkflowInstance> WorkflowInstanceCollection { get; set; }
        private IMongoCollection<Payload> PayloadCollection { get; set; }
        private IMongoCollection<TaskDispatchEventInfo> TaskDispatchEventInfoCollection { get; set; }
        private RetryPolicy RetryMongo { get; set; }
        private RetryPolicy<List<Payload>> RetryPayload { get; set; }
        private RetryPolicy<List<TaskDispatchEventInfo>> RetryTaskDispatchEventInfo { get; set; }

        public MongoClientUtil(string connectionString, string database, string workflowCollection = null, string workflowInstanceCollection = null, string payloadCollection = null, string taskDispatchEventInfo = null)
        {
            workflowCollection ??= "Workflows";
            workflowInstanceCollection ??= "WorkflowInstances";
            payloadCollection ??= "Payloads";
            taskDispatchEventInfo ??= "TaskDispatchEvents";

            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase($"{database}");
            WorkflowRevisionCollection = Database.GetCollection<WorkflowRevision>($"{workflowCollection}");
            WorkflowInstanceCollection = Database.GetCollection<WorkflowInstance>($"{workflowInstanceCollection}");
            PayloadCollection = Database.GetCollection<Payload>($"{payloadCollection}");
            TaskDispatchEventInfoCollection = Database.GetCollection<TaskDispatchEventInfo>($"{taskDispatchEventInfo}");
            RetryMongo = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            RetryPayload = Policy<List<Payload>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
            RetryTaskDispatchEventInfo = Policy<List<TaskDispatchEventInfo>>.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));
        }

        #region WorkflowRevision
        public void CreateWorkflowRevisionDocument(WorkflowRevision workflowRevision)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.InsertOne(workflowRevision);
            });
        }

        public void DeleteWorkflowRevisionDocument(string id)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DeleteWorkflowRevisionDocumentByWorkflowId(string workflowId)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteMany(x => x.WorkflowId.Equals(workflowId));
            });
        }

        public void DeleteWorkflowRevisions(string workflowId)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteMany(x => x.WorkflowId.Equals(workflowId));
            });
        }

        public void DeleteAllWorkflowRevisionDocuments()
        {
            RetryMongo.Execute(() =>
            {
                WorkflowRevisionCollection.DeleteMany("{ }");

                var workflows = WorkflowRevisionCollection.Find("{ }").ToList();

                if (workflows.Count > 0)
                {
                    throw new Exception("All workflows are not deleted!");
                }
            });
        }

        public List<WorkflowRevision> GetWorkflowRevisionsByWorkflowId(string workflowId)
        {
            return WorkflowRevisionCollection.Find(x => x.WorkflowId == workflowId).ToList();
        }
        #endregion

        #region WorkflowInstances
        public void CreateWorkflowInstanceDocument(WorkflowInstance workflowInstance)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowInstanceCollection.InsertOne(workflowInstance);
            });
        }

        public WorkflowInstance GetWorkflowInstance(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).FirstOrDefault();
        }

        public WorkflowInstance GetWorkflowInstanceById(string Id)
        {
            return WorkflowInstanceCollection.Find(x => x.Id == Id).FirstOrDefault();
        }

        public List<WorkflowInstance> GetWorkflowInstancesByPayloadId(string payloadId)
        {
            return WorkflowInstanceCollection.Find(x => x.PayloadId == payloadId).ToList();
        }

        public void DeleteAllWorkflowInstances()
        {
            RetryMongo.Execute(() =>
            {
                WorkflowInstanceCollection.DeleteMany("{ }");

                var workflowInstances = WorkflowInstanceCollection.Find("{ }").ToList();

                if (workflowInstances.Count > 0)
                {
                    throw new Exception("All workflows instances are not deleted!");
                }
            });
        }

        public void DeleteWorkflowInstance(string id)
        {
            RetryMongo.Execute(() =>
            {
                WorkflowInstanceCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }
        #endregion

        #region Payload
        public void CreatePayloadDocument(Payload payload)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.InsertOne(payload);
            });
        }
        public List<Payload> GetPayloadCollectionByPayloadId(string payloadId)
        {
            var res = RetryPayload.Execute(() =>
            {
                var payloadCollection = PayloadCollection.Find(x => x.PayloadId == payloadId).ToList();
                if (payloadCollection.Count != 0)
                {
                    return payloadCollection;
                }
                else
                {
                    throw new Exception("Payload not found");
                }
            });
            return res;
        }

        public void DeletePayloadDocument(string id)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.DeleteOne(x => x.Id.Equals(id));
            });
        }

        public void DeletePayloadDocumentByPayloadId(string payloadId)
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.DeleteMany(x => x.PayloadId.Equals(payloadId));
            });
        }

        public void DeleteAllPayloadDocuments()
        {
            RetryMongo.Execute(() =>
            {
                PayloadCollection.DeleteMany("{ }");

                var payloads = PayloadCollection.Find("{ }").ToList();

                if (payloads.Count > 0)
                {
                    throw new Exception("All payloads are not deleted!");
                }
            });
        }
        #endregion

        #region TaskDispatchEventInfo

        public List<TaskDispatchEventInfo> GetTaskDispatchEventInfoByExecutionId(string executionId)
        {
            var res = RetryTaskDispatchEventInfo.Execute(() =>
            {
                return TaskDispatchEventInfoCollection.Find(x => x.Event.ExecutionId == executionId).ToList();
            });
            return res;
        }

        public void DeleteAllTaskDispatch()
        {
            RetryMongo.Execute(() =>
            {
                TaskDispatchEventInfoCollection.DeleteMany("{ }");

                var taskDispatch = TaskDispatchEventInfoCollection.Find("{ }").ToList();

                if (taskDispatch.Count > 0)
                {
                    throw new Exception("All task Dispatch Events were not deleted!");
                }
            });
        }

        #endregion TaskDispatchEventInfo

        public void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}