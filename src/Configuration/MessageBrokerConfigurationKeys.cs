﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.WorkflowManager.Configuration
{
    public class MessageBrokerConfigurationKeys
    {
        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md_workflow_request`.
        /// </summary>
        [ConfigurationKeyName("workflowRequest")]
        public string WorkflowRequest { get; set; } = "md.workflow.request";

        /// <summary>
        /// Gets or sets the topic for publishing task dispatch requests.
        /// Defaults to `md.workflow.task_dispatch`.
        /// </summary>
        [ConfigurationKeyName("taskDispatch")]
        public string TaskDispatchRequest { get; set; } = "md.workflow.task_dispatch";

        /// <summary>
        /// Gets or sets the topic for publishing workflow requests.
        /// Defaults to `md_workflow_request`.
        /// </summary>
        [ConfigurationKeyName("exportRequestPrefix")]
        public string ExportRequestPrefix { get; set; } = "md.export.request";
    }
}
