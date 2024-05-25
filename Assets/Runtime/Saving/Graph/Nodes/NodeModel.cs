using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class NodeModel
    {
        public string NodeId { get; set; }
        public string NodeType { get; set; }
        public Vector2 Position { get; set; }
        public string GroupId { get; set; }
        public List<PortData> InputPortDataCollection { get; set; }
        public List<PortData> OutputPortDataCollection { get; set; }
    }
}
