﻿using Monai.Deploy.WorkflowManager.Models;

namespace Monai.Deploy.WorkflowManager.WorkflowExecutor.IntegrationTests.TestData
{
    public class TaskRequestTestData
    {
        public string? Name { get; set; }

        public TasksRequest? TaskRequest { get; set; }
    }

    public static class TaskRequestsTestData
    {
        public static List<TaskRequestTestData> TestData = new List<TaskRequestTestData>()
        {
            new TaskRequestTestData
            {
                Name = "Valid_Task_Details_1",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "8ff3ea90-0113-4071-9b92-5068956daeff",
                    TaskId = "7b8ea05b-8abe-4848-928d-d55f5eef1bc3",
                }
            },
            new TaskRequestTestData
            {
                Name = "Valid_Task_Details_2",
                TaskRequest = new TasksRequest()
                {
                    WorkflowInstanceId = "44a63094-9e36-4ba4-9fea-8e9b76aa875b",
                    ExecutionId = "a1cd5b89-85e8-4d32-b9aa-bdbc0f4bbba5",
                    TaskId = "953c0236-5292-4186-80ee-ef7d4073220b",
                }
            }
        };
    }
}
