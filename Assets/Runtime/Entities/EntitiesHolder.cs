using Chocolate4.Dialogue.Runtime.Saving;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chocolate4.Dialogue.Runtime.Entities
{
    public class EntitiesHolder : ScriptableObject
	{
        public const string DataBase = "Entities Database";

        public string associatedAssetName;

        [SerializeField]
        private List<DialogueEntity> dataBase = new List<DialogueEntity>();

        public IReadOnlyCollection<DialogueEntity> DialogueEntities => dataBase;

        public bool TryGetEntity(string id, out DialogueEntity entity)
        {
            entity = DialogueEntities.FirstOrDefault(entity => entity.Id == id);
            return entity != null;
        }

        [ContextMenu("Reload")]
		public void Reload()
		{
            dataBase.Clear();

            dataBase.AddRange(
                EntitiesUtilities.GetAllEntities(this)
            );
        }
	}
}