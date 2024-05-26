using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Chocolate4.Dialogue.Edit.CodeGeneration
{
    public static class DialogueMasterCollectionGenerator
	{
        public static void TryRegenerate(
            string fileName, string oldFileName,
            BlackboardSaveData blackboardSaveData, List<TreeItemModel> treeItemsSaveData
        )
        {
            string collectionName = FilePathConstants.GetCollectionName(fileName);
            string oldCollectionName = FilePathConstants.GetCollectionName(oldFileName);

            string oldPath = FilePathConstants.GetCollectionPath(oldCollectionName);
            if (!string.IsNullOrEmpty(oldPath) && !collectionName.Equals(oldCollectionName))
            {
                File.Delete(oldPath);
                AssetDatabase.Refresh();
            }

            string path = FilePathConstants.GetCollectionPath(collectionName);

            string pathRelativeToProjectFolder =
                FilePathConstants.GetPathRelativeTo(FilePathConstants.Assets, path);


            Writer writer = new Writer(new StringBuilder());

            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine("namespace Chocolate4.Dialogue.Runtime.Master.Collections");
            writer.BeginBlockWithIndent();
            writer.WriteLine($"public class {collectionName} : IDialogueMasterCollection");
            writer.BeginBlockWithIndent();
            writer.WriteLine($"public Type CollectionType => typeof({collectionName});");
            writer.WriteLine();

            writer.WriteLine("#region Situation Names");

            writer.WriteLine("public enum SituationName");
            writer.BeginBlockWithIndent();

            foreach (TreeItemModel treeItemData in treeItemsSaveData)
            {
                string displayName = treeItemData.RootItem.DisplayName;
                string sanitizedDisplayName = displayName.ToPascalCase().Sanitize();

                writer.WriteLine($"{sanitizedDisplayName},");
            }

            writer.EndBlockWithIndent();
            writer.WriteLine();

            foreach (TreeItemModel treeItemData in treeItemsSaveData)
            {
                string displayName = treeItemData.RootItem.DisplayName;
                string sanitizedDisplayName = displayName.ToPascalCase().Sanitize();

                writer.WriteLine($"public const string {sanitizedDisplayName} = \"{displayName}\";");
            }

            writer.WriteLine();
            writer.WriteLine("private Dictionary<SituationName, string> situations = new Dictionary<SituationName, string>");
            writer.BeginBlockWithIndent();

            foreach (TreeItemModel treeItemData in treeItemsSaveData)
            {
                string displayName = treeItemData.RootItem.DisplayName;
                string sanitizedDisplayName = displayName.ToPascalCase().Sanitize();

                writer.WriteLine($"{{ SituationName.{sanitizedDisplayName}, {sanitizedDisplayName} }},");
            }

            writer.EndBlockWithIndent(";");

            writer.WriteLine();
            writer.WriteLine("public string GetSituationName(SituationName situationName) => situations[situationName];");
            writer.WriteLine();
            writer.WriteLine("#endregion");
            writer.WriteLine();

            writer.WriteLine("#region Variables");

            List<DialoguePropertyModel> dialoguePropertiesSaveData =
                blackboardSaveData.dialoguePropertiesSaveData;

            List<DialoguePropertyModel> valuesData =
                dialoguePropertiesSaveData.Where(prop => prop.PropertyType != PropertyType.Event).ToList();
            foreach (DialoguePropertyModel property in valuesData)
            {
                writer.WriteLine($"public {property.PropertyType.GetPropertyString()} {property.DisplayName} {{ get; set; }} = {property.Value.ToLower()};");
            }

            writer.WriteLine("#endregion");
            writer.WriteLine();
            writer.WriteLine("#region Events");

            List<DialoguePropertyModel> eventsData = dialoguePropertiesSaveData.Except(valuesData).ToList();
            foreach (DialoguePropertyModel property in eventsData)
            {
                writer.WriteLine($"public {property.PropertyType.GetPropertyString()} {property.DisplayName};");
            }

            foreach (DialoguePropertyModel property in eventsData)
            {
                writer.WriteLine();
                writer.WriteLine($"private void Invoke{property.DisplayName}() => {property.DisplayName}?.Invoke();");
            }

            writer.WriteLine("#endregion");

            writer.EndBlockWithIndent();
            writer.EndBlockWithIndent();

            string generatedFile = writer.buffer.ToString();
            if (FilePathConstants.FileIsDuplicate(pathRelativeToProjectFolder, generatedFile))
            {
                return;
            }

            File.WriteAllText(pathRelativeToProjectFolder, generatedFile);
            AssetDatabase.ImportAsset(pathRelativeToProjectFolder);
        }
    } 
}
