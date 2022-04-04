﻿using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class TaskObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "args")]
        public object Args { get; set; }

        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        [JsonProperty(PropertyName = "task_destinations")]
        public TaskDestination[] TaskDestinations { get; set; }

        [JsonProperty(PropertyName = "export_destinations")]
        public TaskDestination[] ExportDestinations { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public ArtifactMap Artifacts { get; set; }
    }
}
