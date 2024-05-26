using Newtonsoft.Json;
using UnityEngine;

namespace Chocolate4.Dialogue.Runtime.Asset
{
    [System.Serializable]
    public class DialogueAsset : ScriptableObject
    {
        [field:SerializeField]
        public DialogueData DialogueData { get; private set; }

        public void FromJson(string json) => DialogueData = JsonConvert.DeserializeObject<DialogueData>(json);

        public string ToJson() => JsonConvert.SerializeObject(DialogueData);
    }
}