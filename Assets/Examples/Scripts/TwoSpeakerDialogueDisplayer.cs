using Chocolate4.Dialogue.Runtime;
using Chocolate4.Dialogue.Runtime.Saving;
using Chocolate4.Dialogue.Runtime.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chocolate4.Dialogue.Examples
{
    public enum SpeakerSide
    {
        Left,
        Right
    }

    public class TwoSpeakerDialogueDisplayer : MonoBehaviour
    {
        [SerializeField]
        private DialogueMaster dialogueMaster;
        [SerializeField]
        private float letterDelay = 0.01f;
        [SerializeField]
        private Image[] speakerImage;
        [SerializeField]
        private TMP_Text[] speakerNameText;
        [SerializeField]
        private TMP_Text displayText;
        [SerializeField]
        private AdvancedDialogueProgressor dialogueProgressor;
        [SerializeField]
        private ChoiceOption choicePrefab;
        [SerializeField]
        private Transform choicesParent;
        [SerializeField]
        private Canvas choicesCanvas;

        private List<ChoiceOption> choiceButtons = new List<ChoiceOption>();
        private SpeakerSide speakerSide = SpeakerSide.Right;
        private string fullText;
        private bool speakerNamesDefined;
        private bool speakerImagesDefined;
        private string[] activeSpeakerIdentifiers = new string[2];

        private void OnEnable()
        {
            dialogueProgressor.OnReceivedDialogueInfo += DialogueProgressor_OnReceivedDialogueInfo;
            dialogueProgressor.OnReceivedChoiceInfo += DialogueProgressor_OnReceivedChoiceInfo;
            dialogueProgressor.OnReceivedSituationEndedInfo += DialogueProgressor_OnReceivedSituationEndedInfo;
        }

        private void OnDisable()
        {
            dialogueProgressor.OnReceivedDialogueInfo -= DialogueProgressor_OnReceivedDialogueInfo;
            dialogueProgressor.OnReceivedChoiceInfo -= DialogueProgressor_OnReceivedChoiceInfo;
            dialogueProgressor.OnReceivedSituationEndedInfo -= DialogueProgressor_OnReceivedSituationEndedInfo;
        }

        private void Start()
        {
            choicesCanvas.gameObject.SetActive(false);

            displayText.SetText(string.Empty);

            speakerNamesDefined = !speakerNameText.IsNullOrEmpty();
            speakerImagesDefined = !speakerImage.IsNullOrEmpty();
            if (speakerNamesDefined)
            {
                for (int i = 0; i < speakerNameText.Length; i++)
                {
                    speakerNameText[i].SetText(string.Empty);
                } 
            }
        }

        private void DialogueProgressor_OnReceivedDialogueInfo(DialogueNodeInfo dialogueInfo)
        {
            DialogueEntity speaker = dialogueInfo.Speaker;
            ProcessSpeaker(speaker); 

            fullText = dialogueInfo.DialogueText;

            BreakPreviousReveal();
            StartCoroutine(RevealText());
        }

        private void DialogueProgressor_OnReceivedChoiceInfo(DialogueNodeInfo choiceInfo)
        {
            DisplayChoices(choiceInfo.Choices);
        }

        private void DialogueProgressor_OnReceivedSituationEndedInfo(DialogueNodeInfo endInfo)
        {
            displayText.gameObject.SetActive(false);
        }

        private void DisplayChoices(List<string> choices)
        {
            choicesCanvas.gameObject.SetActive(true);

            int choicesToSpawn = choices.Count - choiceButtons.Count;

            if (choicesToSpawn > 0)
            {
                MakeNewChoices(choicesToSpawn);
            }

            choiceButtons.ForEach(button => button.gameObject.SetActive(false));
            ActivateChoices(choices);
        }

        private void ActivateChoices(List<string> choices)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                ChoiceOption choiceButton = choiceButtons[i];

                choiceButton.gameObject.SetActive(true);
                choiceButton.SetButton(choices[i], i, (index) => {
                    dialogueProgressor.SetChoice(index);
                    HideChoices();
                });
            }
        }

        private void MakeNewChoices(int choicesToSpawn)
        {
            for (int i = 0; i < choicesToSpawn; i++)
            {
                ChoiceOption newChoice = Instantiate(choicePrefab, choicesParent);
                choiceButtons.Add(newChoice);
            }
        }

        private void HideChoices()
        {
            foreach (ChoiceOption choiceButton in choiceButtons)
            {
                choiceButton.gameObject.SetActive(false);
            }

            choicesCanvas.gameObject.SetActive(false);
        }

        private void ProcessSpeaker(DialogueEntity newSpeaker)
        {
            using (DialogueEntity processedEntity = newSpeaker.Process(dialogueMaster))
            {
                SetSpeaker(processedEntity);
            }
        }

        private void SetSpeaker(DialogueEntity newSpeaker)
        {
            int currentSide = (int)speakerSide;
            if (speakerNamesDefined)
            {
                string speakerIdentifier = activeSpeakerIdentifiers[currentSide];
                if (string.IsNullOrEmpty(speakerIdentifier)
                    || !speakerIdentifier.Equals(newSpeaker.Id)
                )
                {
                    int enumMemberCount = 2;
                    speakerSide = (SpeakerSide)((currentSide + 1) % enumMemberCount);
                    currentSide = (int)speakerSide;
                }

                activeSpeakerIdentifiers[currentSide] = newSpeaker.Id;
                speakerNameText[currentSide].SetText(newSpeaker.entityName);
            }
            if (speakerImagesDefined)
            {
                speakerImage[currentSide].sprite = newSpeaker.entityImage;
            }
        }

        private void BreakPreviousReveal()
        {
            displayText.SetText(string.Empty);
            StopAllCoroutines();
        }

        private IEnumerator RevealText()
        {
            displayText.gameObject.SetActive(true);

            for (int i = 0; i < fullText.Length; i++)
            {
                displayText.text += fullText[i];
                yield return new WaitForSeconds(letterDelay);
            }
        }
    }
}
