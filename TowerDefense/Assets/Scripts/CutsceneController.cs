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
        if (Input.anyKeyDown && !isTyping && cutsceneLine != null && currentLine < cutsceneLine.Count)
        {

            StartCoroutine(typeText(currentLine));
            swapSprites(currentLine);
            currentLine++;
        }
    }
    private IEnumerator typeText(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= cutsceneLine.Count) yield break;
        Cutscene line = cutsceneLine[lineIndex];
        if (line == null || line.dialogue == null) yield break;
 
        textBox.text = string.Empty;
        isTyping = true;
        foreach (char c in line.dialogue)
        {
            textBox.text += c;
            yield return new WaitForSeconds(dialogueSpeed);
        }

        isTyping = false;

    }

    private void swapSprites(int spriteIndex)
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
}