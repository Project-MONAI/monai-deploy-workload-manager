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

using System.Runtime.Serialization;

namespace Monai.Deploy.TaskManager.Argo
{
    [Serializable]
    public class ArtifactMappingNotFoundException : Exception
    {
        public ArtifactMappingNotFoundException()
        {
        }

        public ArtifactMappingNotFoundException(string? artifactName) : base($"Storage information cannot be found for artifact '{artifactName}'.")
        {
        }

        public ArtifactMappingNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ArtifactMappingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
