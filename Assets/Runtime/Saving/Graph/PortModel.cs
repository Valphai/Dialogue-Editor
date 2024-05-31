using System;

namespace Chocolate4.Dialogue.Runtime.Saving
{
    [Serializable]
    public class PortModel
    {
        public string otherNodeID;
        public string otherPortName;
        public string thisPortName;
        public string thisPortType;

        public PortModel()
        {
            
        }

        public PortModel(PortModel portData)
        {
            otherNodeID = portData.otherNodeID;
            otherPortName = portData.otherPortName;
            thisPortName = portData.thisPortName;
            thisPortType = portData.thisPortType;
        }
    }
}
