using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Scene
{

    public string dialogueLine;
    public Sprite personA;
    public Sprite personB;
}

public class CutsceneController : MonoBehaviour
{
    private int currentLine;

    [SerializeField]
    private string[] dialogue;

    [SerializeField]
    private TextMeshProUGUI textBox;

    [SerializeField]
    private float textSpeed;

    [SerializeField]
    private List<Scene> scenelines;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentLine = 1;

        //temp begin text on start will change to on awake for final pull req
        StartCoroutine(typeText(scenelines[currentLine].dialogueLine));

        
    }


    private IEnumerator typeText(string line)
    {
        foreach(char c in line)
        {
            textBox.text += c;
            yield return new WaitForSeconds(textSpeed);
        } 
    }
}
