using Chocolate4.Dialogue.Runtime.Saving;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Chocolate4.Dialogue.Runtime.Asset
{
    [System.Serializable]
    public class DialogueData
    {
        [JsonProperty]
        public string FileName { get; set; }
        [JsonProperty]
        public BlackboardSaveData BlackboardData { get; set; }
        //[JsonProperty]
        //public EntitiesData EntitiesData { get; set; }

        [JsonIgnore]
        public List<SituationSaveData> SituationData { get; set; }
    }
}