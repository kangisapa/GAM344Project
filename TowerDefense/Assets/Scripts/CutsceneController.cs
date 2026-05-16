using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
 
[Serializable]
public class Cutscene
{
    public string dialogue;
    public Sprite p1;
    public Sprite p2;
    public float dialogueSpeed;
    public bool isChoice;
    public enum whosTalking
    {
        personA,
        personB,
        none,
    }  
    public whosTalking whostalking;

    internal enum panicChoices
    {
        Headache = 0,
        Fever = 1,
        Chills = 2,
    }

}

public class CutsceneController : MonoBehaviour
{

    public List<Cutscene> cutsceneLine;
    public float baseTalkSpeed;
    public GameObject choiceUI;

    internal int currentLine;
    

    [SerializeField]
    private TextMeshProUGUI textBox;
    [SerializeField]
    private Image personA;
    [SerializeField] 
    private Image personB;

    private bool isTyping;
    private Color qColor;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        /* use when sprites are actually given
        personA.GetComponent<Image>().sprite = null;
        personB.GetComponent<Image>().sprite = null;
        */

        for (int i = 0; i < cutsceneLine.Count; i++)
        {
            if (cutsceneLine[i].dialogueSpeed <= 0)
            {
                cutsceneLine[i].dialogueSpeed = baseTalkSpeed;
            }
        }
        qColor = new Color(1f, 1f, 1f, 0.5f);

        personA.GetComponent<Image>().color = qColor;
        personB.GetComponent<Image>().color = qColor;
        Debug.Log(aTalking(cutsceneLine[1].whostalking));
        
    }

    //used to continue the conversation
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping && cutsceneLine != null && currentLine < cutsceneLine.Count)
        {

            StartCoroutine(typeText(currentLine));
            swapSprites(currentLine);
            checkWhosTalking(currentLine);
            currentLine++;
        }
        Debug.Log(currentLine);
    }
    public IEnumerator typeText(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= cutsceneLine.Count) yield break;
        Cutscene line = cutsceneLine[lineIndex];
        if (line == null || line.dialogue == null) yield break;


        textBox.text = string.Empty;
        isTyping = true;
        foreach (char c in line.dialogue)
        {
            textBox.text += c;
            yield return new WaitForSeconds(line.dialogueSpeed);
        }

        if (line.isChoice)
        {
            choiceUI.SetActive(true);
            yield break;
        }
        else
        {
            choiceUI.SetActive(false);
        }

        isTyping = false;

    }

    public void swapSprites(int spriteIndex)
    {
        if (spriteIndex < 0 || spriteIndex >= cutsceneLine.Count) return;
 
        Cutscene line = cutsceneLine[spriteIndex];
        if (line == null) return;
 
        if (personA != null && line.p1 != null)
        {
            personA.sprite = line.p1;
            personA.enabled = true;
        }
        else if (personA != null && line.p1 == null)
        {
            //personA.enabled = false;
        }
 
        if (personB != null && line.p2 != null)
        {
            personB.sprite = line.p2;
            personB.enabled = true;
        }
        else if (personB != null && line.p2 == null)
        {
            //personB.enabled = false;
        }
    }
    public void checkWhosTalking(int personIndex)
    {
        if (personIndex < 0 || personIndex >= cutsceneLine.Count) return;

        Cutscene line = cutsceneLine[personIndex];
        if(line == null) return;

        if (aTalking(line.whostalking)) { personA.color = Color.white; }
        else { personA.color = qColor; }

        if(bTalking(line.whostalking)) { personB.color = Color.white; }
        else { personB.color = qColor; }
    }

    private bool aTalking(Cutscene.whosTalking current)
    {
        if(current is Cutscene.whosTalking.personA) { return true; }
        else { return false; }
    }

    private bool bTalking(Cutscene.whosTalking current)
    {
        if (current is Cutscene.whosTalking.personB) { return true; }
        else { return false; }
    }
}