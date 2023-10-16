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

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Wrappers;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Filter;
using Monai.Deploy.WorkflowManager.Common.Miscellaneous.Services;

namespace Monai.Deploy.WorkflowManager.Common.ControllersShared
{
    /// <summary>
    /// Base Api Controller.
    /// </summary>
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiControllerBase"/> class.
        /// </summary>
        public ApiControllerBase()
        {
        }

        /// <summary>
        /// Gets internal Server Error 500.
        /// </summary>
        protected static int InternalServerError => (int)HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets bad Request 400.
        /// </summary>
        protected static new int BadRequest => (int)HttpStatusCode.BadRequest;

        /// <summary>
        /// Gets notFound 404.
        /// </summary>
        protected static new int NotFound => (int)HttpStatusCode.NotFound;
    }
}
