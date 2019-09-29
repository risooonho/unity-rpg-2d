using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleMagicSelect : MonoBehaviour
{

    public string spellName;
    public int spellCost;
    public Text nameText;
    public Text costText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Press()
    {
        if(BattleManager.instance.activeBattler[BattleManager.instance.currentTurn].currentMp >= spellCost)
        {
            BattleManager.instance.OpenMenuTarget(spellName);
            BattleManager.instance.magicMenu.SetActive(false);
            BattleManager.instance.activeBattler[BattleManager.instance.currentTurn].currentMp -= spellCost;
        }else
        {
            BattleManager.instance.battleNotice.theText.text = "Not Enough MP!";
            BattleManager.instance.battleNotice.Activate();
            BattleManager.instance.magicMenu.SetActive(false);
            //let player know there is not enough MP
        }
    }
}
