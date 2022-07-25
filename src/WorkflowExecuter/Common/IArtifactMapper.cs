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

namespace Monai.Deploy.WorkflowManager.WorkfowExecuter.Common
{
    public interface IArtifactMapper
    {
        /// <summary>
        /// Converts an array of artifacts to a dictionary of artifact path variables.
        /// </summary>
        /// <param name="artifacts">Array of artifacts to convert.</param>
        /// <param name="payloadId">Payload id to check against.</param>
        /// <param name="workflowInstanceId">Workflow instance id to check against.</param>
        /// <param name="bucketId">Bucket id used to verify.</param>
        /// <param name="shouldExistYet">Checks if it should exist yet.</param>
        Task<Dictionary<string, string>> ConvertArtifactVariablesToPath(Artifact[] artifacts, string payloadId, string workflowInstanceId, string bucketId, bool shouldExistYet = true);
    }
}
