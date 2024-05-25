using Chocolate4.Dialogue.Runtime.Utilities;
using System;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class PropertyNodeSaveData : NodeModel
    {
        public string propertyID;
        public PropertyType propertyType;
        public string value;
    }
}
