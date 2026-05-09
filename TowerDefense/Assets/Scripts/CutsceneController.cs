using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Cutscene
{
    public string dialogue;
    public Sprite p1;
    public Sprite p2;
}
public class CutsceneController : MonoBehaviour
{

    public List<Cutscene> cutsceneLine;

    [SerializeField]
    private float dialogueSpeed;
    [SerializeField]
    private TextMeshProUGUI textBox;
    [SerializeField]
    private Image personA;
    [SerializeField] 
    private Image personB;


    private int currentLine;
    private bool isTyping;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        /* use when sprites are actually given
        personA.GetComponent<Image>().sprite = null;
        personB.GetComponent<Image>().sprite = null;
        */


    }

    //used to continue the conversation
    private void Update()
    {
        if (Input.anyKeyDown && !isTyping)
        {

            StartCoroutine(typeText(currentLine));
            swapSprites(currentLine);
            currentLine++;
        }
    }
    private IEnumerator typeText(int lineIndex)
    {
        
        textBox.text = string.Empty;
        isTyping = true;
        foreach(char c in cutsceneLine[lineIndex].dialogue)
        {
            textBox.text += c;
            yield return new WaitForSeconds(dialogueSpeed);
        }

        isTyping=false;

    }

    private void swapSprites(int spriteIndex)
    {
       personA.GetComponent<Image>().sprite = cutsceneLine[spriteIndex].p1;
       personB.GetComponent<Image>().sprite = cutsceneLine[spriteIndex].p2;
    }

}
