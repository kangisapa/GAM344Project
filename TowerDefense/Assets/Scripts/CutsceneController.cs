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
    public Sprite boxImage;
    public float dialogueSpeed;
    public bool isChoice;

    public enum whosTalking
    {
        personA,
        personB,
        none,
    }  
    public whosTalking whostalking;

    public enum fontStyle
    {
        Bold,
        Italic,
        Underline,
        Strikethrough,
        Lowercase,
        Uppercase,
        Smallcaps,
        None,
    }
    public fontStyle fontstyle = fontStyle.None;

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
    [SerializeField]
    private Sprite baseBoxImage;
    [SerializeField]
    private string nextScene;

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

            if (cutsceneLine[i].boxImage == null)
            {
                cutsceneLine[i].boxImage = baseBoxImage;
            }
        }
        qColor = new Color(1f, 1f, 1f, 0.5f);

        personA.GetComponent<Image>().color = qColor;
        personB.GetComponent<Image>().color = qColor;

        
    }

    bool lockout = false;

    //used to continue the conversation
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Mouse0) && !isTyping && cutsceneLine != null && currentLine < cutsceneLine.Count)
        {

            StartCoroutine(typeText(currentLine));
            swapSprites(currentLine);
            ChangeFont(cutsceneLine[currentLine].fontstyle);
            checkWhosTalking(currentLine);
            currentLine++;
        }
        // Debug.Log(currentLine);
        // Debug.Log(cutsceneComplete());
        if (!lockout && Input.anyKey && cutsceneComplete())
        {
            lockout = true;
            GameManager.Instance.ShowContinueMenu();
        }
    }

    public void ContinueToNextScene()
    {
        if (cutsceneComplete())
        {
            GameManager.Instance.GoToNewSceneAfterDelay(1, nextScene);
        }
    }

    public IEnumerator typeText(int lineIndex)
    {
        
        if (lineIndex < 0 || lineIndex >= cutsceneLine.Count) yield break;
            Cutscene line = cutsceneLine[lineIndex];
        if (line == null || line.dialogue == null) yield break;


        textBox.maxVisibleCharacters = 0;
        textBox.text = line.dialogue;
        isTyping = true;
        for(int i = 0; i <= textBox.textInfo.characterCount; i++)
        {
            textBox.maxVisibleCharacters = i;
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

    public void ChangeFont(Cutscene.fontStyle font)
    {
        switch (font)
        {
            case Cutscene.fontStyle.Bold:
                textBox.fontStyle = FontStyles.Bold; 
                break;
            case Cutscene.fontStyle.Italic:
                textBox.fontStyle = FontStyles.Italic;
                break;
            case Cutscene.fontStyle.Strikethrough:
                textBox.fontStyle = FontStyles.Strikethrough;
                break;
            case Cutscene.fontStyle.Lowercase:
                textBox.fontStyle = FontStyles.LowerCase;
                break;
            case Cutscene.fontStyle.Uppercase:
                textBox.fontStyle = FontStyles.UpperCase;
                break;
            case Cutscene.fontStyle.Smallcaps:
                textBox.fontStyle = FontStyles.SmallCaps;
                break;
            case Cutscene.fontStyle.None:
                textBox.fontStyle = FontStyles.Normal;
                break;
        }
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

    private bool cutsceneComplete()
    {
        
        if (currentLine >= cutsceneLine.Count && !isTyping) { return true; }

        else { return false; }
    }
}