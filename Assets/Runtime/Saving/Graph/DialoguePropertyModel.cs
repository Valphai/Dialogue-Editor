using Chocolate4.Dialogue.Edit.Graph.Utilities;
using Chocolate4.Dialogue.Runtime.Utilities;
using Newtonsoft.Json;
using System;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class DialoguePropertyModel : IHaveId
    {
        [JsonProperty] 
        public string Id { get; set; }
        [JsonProperty] 
        public string DisplayName { get; set; }
        [JsonProperty] 
        public string Value { get; set; }
        [JsonProperty] 
        public PropertyType PropertyType { get; set; }
    }
}
