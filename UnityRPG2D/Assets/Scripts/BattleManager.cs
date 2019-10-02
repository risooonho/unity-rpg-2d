using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    public GameObject targetMenu;
    public BattleTargetButton[] targetButtons;

    public GameObject magicMenu;

    public BattleMagicSelect[] magicButtons;
    public BattleNotification battleNotice;

    public int chanceToFlee = 35;
    private bool fleeing;

    public Item activeItem;
    public Text itemName, itemDescription;
    public GameObject ItemsMenu;
    public ItemButton[] battleUseItemsButtons;

    public string gameOverScene;

    public int rewardXP;
    public string[] rewardItems;

    public bool cannotFlee;

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
            BattleStart(new string[]{"Goblin", "Spider", "Skeleton"}, false);
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

    public void BattleStart(string[] enemyToSpawn , bool cantFlee)
    {   
        if(!battleActive)
        {
            battleActive = true;
            cannotFlee = cantFlee;
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
                if(activeBattler[i].isPlayer)
                {
                    activeBattler[i].theSprite.sprite = activeBattler[i].deadSprite;
                }else
                {
                    activeBattler[i].EnemyFade();
                }
            }else
            {
                if(activeBattler[i].isPlayer)
                {
                    allPlayerDead = false;
                    activeBattler[i].theSprite.sprite = activeBattler[i].aliveSprite;

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
                StartCoroutine(EndBattleCo());
            } else
            {
                //end battle in failure
                StartCoroutine(GameOverCo());
            }

            // battleScene.SetActive(false);
            // GameManager.instance.battleActive = false;
            // battleActive = false;
        }else
        {
            while(activeBattler[currentTurn].currentHp == 0)
            {
                currentTurn++;
                if(currentTurn >= activeBattler.Count)
                {
                    currentTurn = 0;
                }
            }
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

    public void PlayerAttack(string moveName, int selectedTarget)
    {
         int movePower = 0;
        for(int i = 0; i < moveList.Length; i++)
        {
            if(moveList[i].moveName == moveName)
            {
                Instantiate(moveList[i].theEffect, activeBattler[selectedTarget].transform.position, activeBattler[selectedTarget].transform.rotation);
                movePower = moveList[i].movePower;
            }
        }
        Instantiate(enemyAttackEffect, activeBattler[currentTurn].transform.position, activeBattler[currentTurn].transform.rotation);
        DealDame(selectedTarget, movePower);

        uiButtonHolder.SetActive(false);
        targetMenu.SetActive(false);

        NextTurn();
    }

    public void OpenMenuTarget(string moveName)
    {
        targetMenu.SetActive(true);
        List<int> Enemies = new List<int>();
        for(int i = 0; i < activeBattler.Count; i++)
        {
            if(!activeBattler[i].isPlayer)
            {
                Enemies.Add(i);
            }
        }

        for(int i = 0; i < targetButtons.Length; i++)
        {
            if(Enemies.Count > i && activeBattler[Enemies[i]].currentHp > 0)
            {
                targetButtons[i].gameObject.SetActive(true);
                targetButtons[i].moveName = moveName;
                targetButtons[i].activeBattlerTarget = Enemies[i];
                targetButtons[i].targetName.text = activeBattler[Enemies[i]].charName;
            }else
            {
                targetButtons[i].gameObject.SetActive(false);
                
            }
        }
    }

    public void OpenMagicMenu()
    {
        magicMenu.SetActive(true);
        for(int i = 0; i < magicButtons.Length; i++)
        {
            if(activeBattler[currentTurn].movesAvailable.Length > i)
            {
                magicButtons[i].gameObject.SetActive(true);

                magicButtons[i].spellName = activeBattler[currentTurn].movesAvailable[i];
                magicButtons[i].nameText.text = magicButtons[i].spellName;

                for(int j = 0; j < moveList.Length; j++)
                {
                    if(moveList[j].moveName == magicButtons[i].spellName)
                    {
                        magicButtons[i].spellCost = moveList[j].moveCost;
                        magicButtons[i].costText.text = magicButtons[i].spellCost.ToString();
                    }
                }

            }else
            {
                magicButtons[i].gameObject.SetActive(false);
            }
        } 
    }

    public void Flee()
    {   
        if(cannotFlee)
        {
            battleNotice.theText.text = "Can not flee this battle";
            battleNotice.Activate();
        } else
        {
          int fleeSuccess = Random.Range(0, 100);
            if(fleeSuccess < chanceToFlee)
            {
                //end the battle
                StartCoroutine(EndBattleCo());
                fleeing = true;
                // battleActive = false;
                // battleScene.SetActive(false);
            }else
            {
                NextTurn();
                battleNotice.theText.text = "Couldn't escape!";
                battleNotice.Activate();
            }  
        }
        
    }

    public void OpenItemsMenu()
    {
        ItemsMenu.SetActive(true);
        ShowBattleItem();
    }

    public void CloseItemsMenu()
    {
        ItemsMenu.SetActive(false);
    }

    public void ShowBattleItem(){
        GameManager.instance.SortItems();
        for(int i = 0; i < battleUseItemsButtons.Length; i++){
            battleUseItemsButtons[i].buttonValue = i;

            if(GameManager.instance.itemsHeld[i] != "")
            {
                battleUseItemsButtons[i].buttonImage.gameObject.SetActive(true);
                battleUseItemsButtons[i].buttonImage.sprite = GameManager.instance.GetItemDetail(GameManager.instance.itemsHeld[i]).itemSprite;
                battleUseItemsButtons[i].amountText.text = GameManager.instance.numberOfItems[i].ToString();
            }else
            {
                battleUseItemsButtons[i].buttonImage.gameObject.SetActive(false);
                battleUseItemsButtons[i].amountText.text = "";             
            }   
        }
    }

    public void SelectedBattleItem(Item newItem)
    {
        activeItem = newItem;
        itemName.text = activeItem.itemName;
        itemDescription.text = activeItem.description;
    }

    public void UseBattleItem()
    {   
        if(activeItem.isItem)
        {
            BattleChar selectedChar = activeBattler[currentTurn];
            activeBattler[currentTurn] = activeItem.UseBattleItem(selectedChar);
            UpdateStatUI();
            ItemsMenu.SetActive(false);
            NextTurn();
        }else
        {
            battleNotice.theText.text = "Choosen item can't use in battle";
            battleNotice.Activate();
            ItemsMenu.SetActive(false);
        }
    }

    public IEnumerator EndBattleCo()
    {
        battleActive = false;
        uiButtonHolder.SetActive(false);
        targetMenu.SetActive(false);
        magicMenu.SetActive(false);
        ItemsMenu.SetActive(false);

        yield return new WaitForSeconds(.5f);

        UIFade.instance.FadeToBlack();

        yield return new WaitForSeconds(1.5f);

        for(int i = 0 ; i < activeBattler.Count; i++)
        {
            if(activeBattler[i].isPlayer)
            {
                for(int j = 0; j < GameManager.instance.playerStats.Length; j++)
                {
                    GameManager.instance.playerStats[j].currentHP = activeBattler[i].currentHp;
                    GameManager.instance.playerStats[j].currentMP = activeBattler[i].currentMp;
                }
            }

            Destroy(gameObject);
        }

        UIFade.instance.FadeFromBlack();
        battleScene.SetActive(false);
        activeBattler.Clear();
        currentTurn = 0;
        // GameManager.instance.battleActive = false;
        if(fleeing)
        {
            GameManager.instance.battleActive = false;
            fleeing = false;
        }else
        {
            BattleRewards.instance.OpenRewardScreen(rewardXP, rewardItems);
        }

        AudioManager.instance.PlayBGM(FindObjectOfType<CameraController>().musicToPlay);
    }

    public IEnumerator GameOverCo()
    {
        battleActive = false;
        UIFade.instance.FadeToBlack();
        yield return new WaitForSeconds(1.5f);
        battleScene.SetActive(false);
        SceneManager.LoadScene(gameOverScene);
    }
}
