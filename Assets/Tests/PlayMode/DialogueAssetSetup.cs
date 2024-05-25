using B83.LogicExpressionParser;
using Chocolate4.Dialogue.Runtime;
using Chocolate4.Dialogue.Runtime.Asset;
using Chocolate4.Dialogue.Runtime.Entities;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Runtime.Utilities.Parsing;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chocolate4.Dialogue.Tests.Utilities
{
    internal static class DialogueAssetSetup
    {
        private const string DialogueAsset = "dialogueAsset";
        private const string ParserAdapter = "parseAdapter";
        private const string Parser = "parser";

        internal static DialogueMaster LoadMaster()
        {
            GameObject go = new GameObject("Dialogue Master");
            DialogueMaster dialogueMaster = go.AddComponent<DialogueMaster>();

            string path = 
                FilePathConstants.Chocolate4 + FilePathConstants.DirSep + FilePathConstants.testCasesAsset;
            
            DialogueAsset asset =
                Resources.Load<DialogueAsset>(path);

            string fullAssetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);

            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(fullAssetPath);
            EntitiesHolder entitiesDatabase = assets.Last() as EntitiesHolder;

            FieldInfo dialogueAssetField =
                typeof(DialogueMaster).GetField(DialogueAsset, BindingFlags.NonPublic | BindingFlags.Instance);

            PropertyInfo entitiesField =
                typeof(DialogueMaster).GetProperty("EntitiesDatabase", BindingFlags.Public | BindingFlags.Instance);

            dialogueAssetField.SetValue(dialogueMaster, asset);
            entitiesField.SetValue(dialogueMaster, entitiesDatabase);

            dialogueMaster.Initialize();

            return dialogueMaster;
        }

        internal static Parser GetParser(DialogueMaster dialogueMaster, out ParseAdapter parseAdapter)
        {
            parseAdapter = (ParseAdapter)typeof(DialogueMaster)
                            .GetField(ParserAdapter, BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(dialogueMaster);

            Parser parser = (Parser)typeof(ParseAdapter)
                .GetField(Parser, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(parseAdapter);
            return parser;
        }
    }
}
