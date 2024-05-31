using Newtonsoft.Json;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Saving
{
    [System.Serializable]
    public class GroupModel
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string DisplayName { get; set; }
        [JsonProperty]
        public Vector2 Position { get; set; }
    }
}