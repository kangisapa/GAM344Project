
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;


public class Choices : Cutscene
{
    
    internal static string choiceName(int choiceIndex)
    {
        string name = Enum.GetName(typeof(panicChoices), choiceIndex);
        return name;
    } 
   
}
public class DialogueChoice : MonoBehaviour
{
    

    [SerializeField]
    private List<Button> button;
    [SerializeField]
    private List<TextMeshProUGUI> buttonText;
    private CutsceneController controller;

    public int currentChoice;

    private void Awake()
    {
        controller = GetComponentInParent<CutsceneController>();
    }
    private void OnEnable()
    {
        

        /*this is my first time accessing grandchildren from code and not the editor so if anyone 
         * can give me feedback/an easier way to do it I would be v grateful
         * - Mike
         */

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            button.Add(gameObject.transform.GetChild(i).GetComponent<Button>());
            
            int index = i;
            button[i].onClick.AddListener(() => Choose(index));

        }

        for (int i = 0; i < button.Count; i++)
        {
            buttonText.Add(button[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>());
            buttonText[i].text = Choices.choiceName(i);
            
        }

        
    }

    private void OnDisable()
    {
        for (int i = 0; i < button.Count; i++)
        {   
            buttonText[i].text = string.Empty;
        }
        buttonText.Clear();
        button.Clear();

    }

    public void Choose(int Choice)
    {    
        currentChoice = Choice; 
        StartCoroutine(controller.typeText(controller.currentLine));
        controller.swapSprites(controller.currentLine);
        controller.ChangeFont(controller.cutsceneLine[controller.currentLine].fontstyle);
        controller.checkWhosTalking(controller.currentLine);
        controller.currentLine++;


    }





}
