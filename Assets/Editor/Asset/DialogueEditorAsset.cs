using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocolate4.Dialogue.Edit.Asset
{
    public class DialogueEditorAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string serializedData;

        private DialogueAssetManager assetManager;

        private DialogueDataOwner originalReference;
        private DialogueDataOwner deserializedRefrence;

        public DialogueDataOwner OriginalReference
        {
            get => originalReference;
            set
            {
                if (originalReference != null)
                    originalReference.Owner = null;

                originalReference = value;
                if (originalReference != null)
                    originalReference.Owner = this;
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private void OnDisable()
        {
            Undo.ClearUndo(this);
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        internal void Initialize(DialogueAssetManager assetManager)
        {
            this.assetManager = assetManager;
        }

        public void OnBeforeSerialize()
        {
            if (originalReference != null)
                serializedData = JsonConvert.SerializeObject(OriginalReference);
        }

        public void OnAfterDeserialize()
        {
            var deserializedObject = JsonConvert.DeserializeObject<DialogueDataOwner>(serializedData);
            if (originalReference == null)
                originalReference = deserializedObject;
            else
                deserializedRefrence = deserializedObject;
        }

        public void RegisterCompleteObjectUndo(string undoOperation)
        {
            Undo.RegisterCompleteObjectUndo(this, undoOperation);
        }

        private void HandleUndoRedo()
        {
            if (deserializedRefrence == null)
            {
                return;
            }

            originalReference.ReplaceWith(deserializedRefrence);
            deserializedRefrence = null;
            assetManager.Rebuild();
        }
    }
}