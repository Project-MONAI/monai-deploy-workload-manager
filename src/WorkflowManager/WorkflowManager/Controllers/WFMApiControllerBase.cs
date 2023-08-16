/*
 * Copyright 2023 MONAI Consortium
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

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Monai.Deploy.Shared.ControllersShared;
using Monai.Deploy.Common.Configuration;

namespace Monai.Deploy.Common.ControllersShared
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class WFMApiControllerBase : ApiControllerBase
    {
        private readonly IOptions<WorkflowManagerOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="WFMApiControllerBase"/> class.
        /// </summary>
        /// <param name="options">Workflow manager options.</param>
        public WFMApiControllerBase(IOptions<WorkflowManagerOptions> options)
            : base(options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
