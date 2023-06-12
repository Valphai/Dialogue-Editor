﻿using System;
using UnityEngine;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class NodeSaveData
    {
        public string nodeID;
        public string nodeType;
        public string text;
        public Vector2 position;
        public string groupID;
        public string nextNodeId;
    }
}
