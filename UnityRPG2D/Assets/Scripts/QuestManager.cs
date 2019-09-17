using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{   
    public string[] questMarkerName;
    public bool[] questMarkersComplete;


    public static QuestManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        questMarkersComplete = new bool[questMarkerName.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(CheckIfComplete("quest test"));
            MarkQuestComplete("quest test");
            MarkQuestInComplete("fight the demon");
        }
    }

    public int GetQuestNumber(string questToFind)
    {
        for(int i = 0; i < questMarkerName.Length ; i++)
        {
            if(questMarkerName[i] == questToFind)
            {
                return i;
            }
        }

        Debug.LogError("Quest "+ questToFind + " does exist");
        return 0;
    }

    public bool CheckIfComplete(string questToCheck)
    {   
        if(GetQuestNumber(questToCheck) != 0)
        {
            return questMarkersComplete[GetQuestNumber(questToCheck)];
        }
        return false;
    }

    public void MarkQuestComplete(string questToMark)
    {   
        questMarkersComplete[GetQuestNumber(questToMark)] = true;
        UpdateLocalQuestObject();
    }

    public void MarkQuestInComplete(string questToMark)
    {
        questMarkersComplete[GetQuestNumber(questToMark)] = false;
        UpdateLocalQuestObject();
    }

    public void UpdateLocalQuestObject()
    {
        QuestObjectActivator[] questObjects = FindObjectsOfType<QuestObjectActivator>();
        if(questObjects.Length > 0)
        {
            for(int i = 0 ; i < questObjects.Length; i++)
            {
                questObjects[i].CheckCompletion();
            }
        }
    }
}
