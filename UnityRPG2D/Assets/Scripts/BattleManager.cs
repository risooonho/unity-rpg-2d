using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleManager : MonoBehaviour
{   
    public static BattleManager instance;
    public GameObject battleScene;
    private bool battleActive;

    public Transform[] playerPositions;
    public Transform[] enemyPositions;

    public BattleChar[] playerPrefabs;
    public BattleChar[] enemyPrefabs;

    public List<BattleChar> activeBattler = new List<BattleChar>();

    public int currentTurn;
    public bool turnWaiting;
    public GameObject uiButtonHolder;

    public BattleMove[] moveList;

    public GameObject enemyAttackEffect;
    public DamageNumber theDamgeNumber;

    public Text[] playerName, playerHP, playerMP;
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
            BattleStart(new string[]{"Goblin", "Spider", "Skeleton"});
        }

        if(battleActive)
        {
            if(turnWaiting)
            {
                if(activeBattler[currentTurn].isPlayer)
                {
                    uiButtonHolder.SetActive(true);
                }else
                {
                    uiButtonHolder.SetActive(false);
                    //enemy should attack
                    StartCoroutine(EnemyMoveCo());
                }
            }

            if(Input.GetKeyDown(KeyCode.N))
            {
                NextTurn();
            }
        }
    }

    public void BattleStart(string[] enemyToSpawn)
    {
        if(!battleActive)
        {
            battleActive = true;
            GameManager.instance.battleActive = true;
            battleScene.SetActive(true);
            AudioManager.instance.PlayBGM(0);
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);

            for(int i = 0; i < playerPositions.Length; i++)
            {
                if(GameManager.instance.playerStats[i].gameObject.activeInHierarchy)
                {
                    for(int j = 0; j < playerPrefabs.Length; j++)
                    {
                        if(playerPrefabs[j].charName == GameManager.instance.playerStats[i].charName)
                        {
                            BattleChar newPlayer = Instantiate(playerPrefabs[j], playerPositions[i].position, playerPositions[i].rotation);
                            newPlayer.transform.parent = playerPositions[i];
                            activeBattler.Add(newPlayer);

                            CharStats thePlayer =   GameManager.instance.playerStats[i];
                            activeBattler[i].currentHp = thePlayer.currentHP;
                            activeBattler[i].maxHP = thePlayer.maxHP;
                            activeBattler[i].currentMp = thePlayer.currentMP;
                            activeBattler[i].maxMP = thePlayer.maxMP;
                            activeBattler[i].strength = thePlayer.strength;
                            activeBattler[i].defence = thePlayer.defense;
                            activeBattler[i].wpnPower = thePlayer.wpnPwr;
                            activeBattler[i].armrPower = thePlayer.armrPwr;



                        }
                    }
                }

            }

            for(int i = 0; i < enemyToSpawn.Length; i++)
            {
                if(enemyToSpawn[i] != "")
                {
                    for(int j = 0; j < enemyPrefabs.Length; j++)
                    {
                        if(enemyPrefabs[j].charName == enemyToSpawn[i])
                        {
                            BattleChar newEnemy = Instantiate(enemyPrefabs[j], enemyPositions[i].position, enemyPositions[i].rotation);
                            newEnemy.transform.parent = enemyPositions[i];
                            activeBattler.Add(newEnemy);
                        }
                    }
                }
            }
            turnWaiting = true;
            currentTurn = Random.Range(0, activeBattler.Count);

            UpdateStatUI();
        }
    }

    public void NextTurn()
    {
        currentTurn++;
        if(currentTurn >= activeBattler.Count)
        {
            currentTurn = 0;
        }

        turnWaiting = true;

        UpdateBattle();
        UpdateStatUI();
    }

    public void UpdateBattle()
    {
        bool allEnemiesDead = true;
        bool allPlayerDead = true;

        for(int i = 0 ; i < activeBattler.Count; i++)
        {
            if(activeBattler[i].currentHp < 0)
            {
                activeBattler[i].currentHp = 0;
            }

            if(activeBattler[i].currentHp == 0)
            {
                //Handle Dead battler
            }else
            {
                if(activeBattler[i].isPlayer)
                {
                    allPlayerDead = false;
                }else
                {
                    allEnemiesDead = false;
                }
            }

        }

        if(allPlayerDead || allEnemiesDead)
        {
            if(allEnemiesDead)
            {   
                //end battle in victory
            } else
            {
                //end battle in failure
            }

            battleScene.SetActive(false);
            GameManager.instance.battleActive = false;
            battleActive = false;
        }
    }

    public IEnumerator EnemyMoveCo()
    {
        turnWaiting = false;
        yield return new WaitForSeconds(1f);
        EnemyAttack();
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    public void EnemyAttack()
    {
        List<int> players = new List<int>();
        for(int i = 0; i < activeBattler.Count; i++)
        {
            if(activeBattler[i].isPlayer && activeBattler[i].currentHp > 0)
            {
                players.Add(i);
            }
        }
        int selectedTarget = players[Random.Range(0, players.Count)];
        //activeBattler[selectedTarget].currentHp -= 30;

        int selectAttack = Random.Range(0, activeBattler[currentTurn].movesAvailable.Length);
        int movePower = 0;
        for(int i = 0; i < moveList.Length; i++)
        {
            if(moveList[i].moveName == activeBattler[currentTurn].movesAvailable[selectAttack])
            {
                Instantiate(moveList[i].theEffect, activeBattler[selectedTarget].transform.position, activeBattler[selectedTarget].transform.rotation);
                movePower = moveList[i].movePower;
            }
        }
        Instantiate(enemyAttackEffect, activeBattler[currentTurn].transform.position, activeBattler[currentTurn].transform.rotation);
        DealDame(selectedTarget, movePower);
    }

    public void DealDame(int target, int movePower)
    {
        float atkPwr = activeBattler[currentTurn].strength + activeBattler[currentTurn].wpnPower;
        float defPwr = activeBattler[target].defence + activeBattler[target].armrPower;

        float damageCalc = (atkPwr / defPwr) * movePower * Random.Range(.9f, 1.1f);
        int damageToGive = Mathf.RoundToInt(damageCalc);

        Debug.Log(activeBattler[currentTurn].charName + " is dealing " + damageCalc + "(" + damageToGive + ") damage to " + activeBattler[target].charName);
        activeBattler[target].currentHp -= damageToGive;

        Instantiate(theDamgeNumber , activeBattler[target].transform.position, activeBattler[target].transform.rotation).SetDamage(damageToGive);

        UpdateStatUI();
    }

    public void UpdateStatUI()
    {
        for(int i = 0; i < playerName.Length; i++)
        {
            if(activeBattler.Count > i)
            {
                if(activeBattler[i].isPlayer)
                {
                    BattleChar playerData = activeBattler[i];

                    playerName[i].gameObject.SetActive(true);
                    playerName[i].text = playerData.charName;
                    playerHP[i].text = Mathf.Clamp(playerData.currentHp,0 , int.MaxValue) + "/" + playerData.maxHP;
                    playerMP[i].text = Mathf.Clamp(playerData.currentMp,0 , int.MaxValue) + "/" + playerData.maxMP;

                }else
                {
                    playerName[i].gameObject.SetActive(false);
                    
                }
            }else
            {
                playerName[i].gameObject.SetActive(false);
            }
        }
    }
}
