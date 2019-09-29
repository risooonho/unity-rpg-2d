using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{   
    [Header("Item Type")]
    public bool isItem;
    public bool isWeapon;
    public bool isArmor;

    [Header("Item Details")]
    public string itemName;
    public string description;
    public int value;
    public Sprite itemSprite;

    [Header("Item Details")]
    public int amountToChange;
    public bool affectHp, affectMp, affectStr;
    
    [Header("Weapon/Armor Details")]
    public int weaponStrength;

    public int armorStrength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(int charToUseOn)
    {
        CharStats selectedChar = GameManager.instance.playerStats[charToUseOn];
        if(isItem)
        {
            if(affectHp){
                selectedChar.currentHP += amountToChange;
                if(selectedChar.currentHP > selectedChar.maxHP)
                {
                    selectedChar.currentHP = selectedChar.maxHP;
                }
            }

            if(affectMp){
                selectedChar.currentMP += amountToChange;
                if(selectedChar.currentMP > selectedChar.maxMP)
                {
                    selectedChar.currentMP = selectedChar.maxMP;
                }
            }

            if(affectStr)
            {
                selectedChar.strength += amountToChange;
            }
        }

        if(isWeapon)
        {
            if(selectedChar.equippedWpn != "")
            {
                GameManager.instance.AddItem(selectedChar.equippedWpn);
            }

            selectedChar.equippedWpn = itemName;
            selectedChar.wpnPwr = weaponStrength;
        }

        if(isArmor)
        {
            if(selectedChar.equippedArmr != "")
            {
                GameManager.instance.AddItem(selectedChar.equippedArmr);
            }

            selectedChar.equippedArmr = itemName;
            selectedChar.armrPwr = armorStrength;
        }

        GameManager.instance.RemoveItem(itemName);
    }

    public BattleChar UseBattleItem(BattleChar selectedChar)
    {   
        BattleChar usedItemChar = selectedChar;
        if(isItem)
        {
            if(affectHp){
                usedItemChar.currentHp += amountToChange;
                if(usedItemChar.currentHp > usedItemChar.maxHP)
                {
                    usedItemChar.currentHp = usedItemChar.maxHP;
                }
            }

            if(affectMp){
                usedItemChar.currentMp += amountToChange;
                if(usedItemChar.currentMp > usedItemChar.maxMP)
                {
                    usedItemChar.currentMp = usedItemChar.maxMP;
                }
            }

            if(affectStr)
            {
                usedItemChar.strength += amountToChange;
            }
        }
        GameManager.instance.RemoveItem(itemName);
        return usedItemChar;
    }
}
