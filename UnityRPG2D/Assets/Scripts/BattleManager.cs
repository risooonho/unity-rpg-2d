using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{   
    public static BattleManager instance;
    public GameObject battleScene;
    private bool battleActive;

    public Transform[] playerPositions;
    public Transform[] enemyPositions;

    public BattleChar[] playerPrefabs;
    public BattleChar[] enemyPrefabs;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            BattleStart(new string[]{"Goblin", "Spider"});
        }
    }

    public void BattleStart(string[] enemyToSpawn)
    {
        if(!battleActive)
        {
            battleActive = true;
            GameManager.instance.battleActive = true;
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
            AudioManager.instance.PlayBGM(0);
            battleScene.SetActive(true);
        }
    }
}
