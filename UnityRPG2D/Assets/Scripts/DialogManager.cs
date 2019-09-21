using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DialogManager : MonoBehaviour
{
    public Text dialogText;
    public Text nameText;
    public GameObject dialogBox;
    public GameObject nameBox;

    private string[] dialogLines;
    public int currentLine;

    public static DialogManager instance;
    private bool justStarted;

    private string questToMark;
    private bool markQuestComplete;
    private bool shouldMarkQuest;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
       // dialogText.text = dialogLines[currentLine];
    }

    // Update is called once per frame
    void Update()
    {
        if(dialogBox.activeInHierarchy)
        {
            
                if(Input.GetButtonUp("Fire1"))
                {
                    if(!justStarted)
                    {
                        currentLine++;
                        if(currentLine >= dialogLines.Length)
                        {
                            dialogBox.SetActive(false);
                            GameManager.instance.dialogActive = false;
                            if(shouldMarkQuest)
                            {
                                shouldMarkQuest = false;
                                if(markQuestComplete)
                                {
                                    QuestManager.instance.MarkQuestComplete(questToMark);
                                } else 
                                {
                                    QuestManager.instance.MarkQuestInComplete(questToMark);
                                }
                            }
                        } else 
                        {
                            checkIfName();
                            dialogText.text = dialogLines[currentLine];
                        }

                    }else
                    {
                        justStarted = false;
                    }
                }
           
        }
    }

    public void showDialog(string[] newLines, bool isPerson)
    {
        dialogLines = newLines;
        currentLine = 0;
        //Check name of Speaker and increase currentLine to 1
        checkIfName();

        dialogText.text = dialogLines[currentLine];
        dialogBox.SetActive(true); 
        justStarted = true;

        nameBox.SetActive(isPerson);
        GameManager.instance.dialogActive = true;
    }
    public void checkIfName(){
        if(dialogLines[currentLine].StartsWith("n-"))
        {
            nameText.text = dialogLines[currentLine].Replace("n-","");
            currentLine++;
        }
    }

    public void ShouldActiveQuestAtEnd(string questName, bool markComplete)
    {
        questToMark = questName;
        markQuestComplete = markComplete;

        shouldMarkQuest = true;
    }
}
