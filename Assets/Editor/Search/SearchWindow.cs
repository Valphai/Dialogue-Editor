using Chocolate4.Dialogue.Edit.Graph.Nodes;
using Chocolate4.Dialogue.Runtime.Utilities;
using Chocolate4.Dialogue.Edit.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocolate4.Dialogue.Edit.Search
{
    internal class SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView dialogueGraphView;
        private Texture2D indentIcon;

        public void Initialize(DialogueGraphView dialogueGraphView)
        {
            this.dialogueGraphView = dialogueGraphView;

            indentIcon = new Texture2D(1, 1);
            indentIcon.SetPixel(0, 0, Color.clear);
            indentIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<Type> nodeTypes = TypeExtensions.GetTypes<BaseNode>(FilePathConstants.Chocolate4)
                .Except(new Type[] { typeof(StartNode), typeof(EventPropertyNode) }).ToList();

            string contextElementTitle;
            List<SearchTreeEntry> dialogueTreeEntries = new List<SearchTreeEntry>();
            foreach (Type nodeType in nodeTypes)
            {

                contextElementTitle = nodeType.Name;
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(contextElementTitle.ToString(), indentIcon))
                {
                    level = 2,
                    userData = nodeType
                };

                dialogueTreeEntries.Add(entry);
            }

            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
                new SearchTreeEntry(new GUIContent("Group", indentIcon))
                {
                    level = 2,
                    userData = new CustomGroup()
                },
            };

            searchTreeEntries.AddRange(dialogueTreeEntries);

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 windowMousePosition = 
                VisualElementExtensions.ChangeCoordinatesTo(DialogueEditorWindow.Window.rootVisualElement, 
                    DialogueEditorWindow.Window.rootVisualElement.parent, 
                    context.screenMousePosition - DialogueEditorWindow.Window.position.position
                );

            Vector2 spawnPosition = dialogueGraphView.GetLocalMousePosition(windowMousePosition);
            if (SearchTreeEntry.userData is CustomGroup)
            {
                dialogueGraphView.CreateGroup(spawnPosition);
                return true;
            }

            dialogueGraphView.CreateNode(
                spawnPosition, (Type)SearchTreeEntry.userData
            );

            return true;
        }
    }
}
