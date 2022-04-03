using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************
 * Author: Noah Judge
 * Summary: Holds the information for a card
 * GameObject and holds the function for playing
 * the death animation
 * Date Created: Jan. 29th, 2022
 * Date Last Edited: Jan. 31st, 2022
 ************************************************/
public class CardInfo : MonoBehaviour
{
    //Script variables
    public int value;
    public Elements.Element type;
    public Elements.Element deathType;

    //Card components for editing
    public TMPro.TextMeshPro cardType;
    public TMPro.TextMeshPro cardValue;
    public SpriteRenderer typeImage;

    //Symbol storage for later access
    public Sprite earthSymbol;
    public Sprite fireSymbol;
    public Sprite metalSymbol;
    public Sprite waterSymbol;
    public Sprite woodSymbol;

    private void Start()
    {
        UpdateCard();
    }

    public void UpdateCard()
    {
        //Sets the TMP text and sprites based on class variables
        cardType.text = type.ToString();
        cardValue.text = value.ToString();

        switch (type)
        {
            case Elements.Element.EARTH:
                typeImage.sprite = earthSymbol;
                break;
            case Elements.Element.FIRE:
                typeImage.sprite = fireSymbol;
                break;
            case Elements.Element.METAL:
                typeImage.sprite = metalSymbol;
                break;
            case Elements.Element.WATER:
                typeImage.sprite = waterSymbol;
                break;
            case Elements.Element.WOOD:
                typeImage.sprite = woodSymbol;
                break;
        }
    }

    //Plays the death sound for this card
    public void PlayDeath()
    {
        switch (deathType)
        {
            case Elements.Element.EARTH:
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayEDeath();
                break;
            case Elements.Element.FIRE:
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayFDeath();
                break;
            case Elements.Element.METAL:
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayMDeath();
                break;
            case Elements.Element.WATER:
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayWaDeath();
                break;
            case Elements.Element.WOOD:
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayWoDeath();
                break;
        }
        
    }
}
