using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************
 * Author: Noah Judge
 * Summary: Controls the flow of the game from
 * the beginning to the end. Breaks things up
 * stages with coroutines
 * Date Created: Jan. 29th, 2022
 * Date Last Edited: Jan. 31st, 2022
 ************************************************/
public class GameSessionSP : MonoBehaviour
{
    //Variables that handle the cards
    public GameObject selectedCard;
    PlayerDeck playerDeck;
    PlayerDeck opponentDeck;
    GameObject[] playerHand;
    GameObject[] opponentHand;
    GameObject opponentCard;
    CardInfo playerInfo;
    CardInfo oppInfo;

    //Checkpoint booleans for flow control
    bool readyToStart = false;
    bool hasStarted = false;
    bool selectionStage = false;
    bool revealStage = false;
    bool boostStage = false;
    bool winnerStage = false;
    bool clearingStage = false;
    bool pointsStage = false;
    bool wincheckStage = false;
    bool restartStage = false;

    //Other booleans for gameplay
    public bool hasForfeited = false;
    bool playerWon = false;
    bool wasTie = false;
    bool wasBoosted = false;
    bool overallWinner = false;

    //Counters for card dealing
    int drawCount = 0;
    int cardsDealt = 0;

    //Win counters for player and opponent
    int playerFireWins = 0;
    int playerEarthWins = 0;
    int playerMetalWins = 0;
    int playerWaterWins = 0;
    int playerWoodWins = 0;
    int oppFireWins = 0;
    int oppEarthWins = 0;
    int oppMetalWins = 0;
    int oppWaterWins = 0;
    int oppWoodWins = 0;


    private void Start()
    {
        //Initializing variables
        playerDeck = GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        opponentDeck = GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
        playerHand = new GameObject[5];
        opponentHand = new GameObject[5];
        
        //1) Shuffle Deck
        ShuffleDeck(ref playerDeck);
        ShuffleDeck(ref opponentDeck);

        //2) Deal cards
        StartCoroutine(StartDealing());
    }
    private void Update()
    {
        //3) Initiate start
        if(!readyToStart && cardsDealt == 10)
        {
            readyToStart = true;
        }
        else if (readyToStart && !hasStarted)
        {
            //Enables colliders allowing cards to be chosen
            foreach(GameObject card in playerHand)
            {
                card.GetComponent<BoxCollider2D>().enabled = true;
            }

            hasStarted = true;
            selectionStage = true;
        }


        if (hasStarted)
        {
            //4) Card select
            if (selectedCard != null && selectionStage)
            {
                //Player card selection
                foreach (GameObject card in playerHand)
                {
                    //Keeps other cards from being picked
                    card.GetComponent<BoxCollider2D>().enabled = false;
                }
                //Flips card and moves it to the play position
                Vector2 cardPos = selectedCard.transform.position;
                Vector2 destPos = GameObject.FindGameObjectWithTag("PlayerPlay").transform.position;
                selectedCard.GetComponent<Animator>().SetTrigger("Unflip");
                StartCoroutine(MoveCard(selectedCard, cardPos, destPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

                //Opponent card selection; Random
                int cardSelected = Random.Range(0, 5);
                opponentCard = opponentHand[cardSelected];
                //Flips card and moves it to the play position
                Vector2 oppCardPos = opponentCard.transform.position;
                Vector2 oppDestPos = GameObject.FindGameObjectWithTag("OpponentPlay").transform.position;
                StartCoroutine(MoveCard(opponentCard, oppCardPos, oppDestPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

                //Decrements how many cards are playable to trigger drawing later
                cardsDealt -= 2;

                //Delays and then initiates the reveal stage
                StartCoroutine(InitiateRevealStage());
            }

            if (revealStage)
            {
                //5) Reveal cards
                selectedCard.GetComponent<Animator>().SetTrigger("Flip");
                opponentCard.GetComponent<Animator>().SetTrigger("FlipU");

                //Delays and then initiates the boost checking stage
                StartCoroutine(InitiateBoostStage());
            }

            if (boostStage)
            {
                //Grabs the card info from both selected cards
                playerInfo = selectedCard.GetComponent<CardInfo>();
                oppInfo = opponentCard.GetComponent<CardInfo>();

                //6) Add boosts
                if (playerInfo.type == Elements.Element.FIRE && oppInfo.type == Elements.Element.EARTH ||
                    playerInfo.type == Elements.Element.EARTH && oppInfo.type == Elements.Element.METAL ||
                    playerInfo.type == Elements.Element.METAL && oppInfo.type == Elements.Element.WATER ||
                    playerInfo.type == Elements.Element.WATER && oppInfo.type == Elements.Element.WOOD ||
                    playerInfo.type == Elements.Element.WOOD && oppInfo.type == Elements.Element.FIRE)
                {
                    //Add boost value to opponents card
                    oppInfo.value += playerInfo.value / 2;
                    oppInfo.cardValue.text = oppInfo.value.ToString();
                    oppInfo.cardValue.color = new Color(0, 0.5f, 0, 1);
                    GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayBoost();
                    wasBoosted = true;
                }
                else if (oppInfo.type == Elements.Element.FIRE && playerInfo.type == Elements.Element.EARTH ||
                        oppInfo.type == Elements.Element.EARTH && playerInfo.type == Elements.Element.METAL ||
                        oppInfo.type == Elements.Element.METAL && playerInfo.type == Elements.Element.WATER ||
                        oppInfo.type == Elements.Element.WATER && playerInfo.type == Elements.Element.WOOD ||
                        oppInfo.type == Elements.Element.WOOD && playerInfo.type == Elements.Element.FIRE)
                {
                    //Add boost value to players card
                    playerInfo.value += oppInfo.value / 2;
                    playerInfo.cardValue.text = playerInfo.value.ToString();
                    playerInfo.cardValue.color = new Color(0, 0.5f, 0, 1);
                    GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayBoost();
                    wasBoosted = true;
                }

                //Delays before initiating winner stage if there was a boost
                if (wasBoosted)
                {
                    StartCoroutine(InitiateWinnerStage());
                }
                //Initiates it immediately otherwise
                else
                {
                    boostStage = false;
                    winnerStage = true;
                }
            }

            if (winnerStage)
            {
                //Grabs the card info from both selected cards
                playerInfo = selectedCard.GetComponent<CardInfo>();
                oppInfo = opponentCard.GetComponent<CardInfo>();

                //7) Determine winner
                if (playerInfo.type == Elements.Element.FIRE && oppInfo.type == Elements.Element.METAL ||
                    playerInfo.type == Elements.Element.EARTH && oppInfo.type == Elements.Element.WATER ||
                    playerInfo.type == Elements.Element.METAL && oppInfo.type == Elements.Element.WOOD ||
                    playerInfo.type == Elements.Element.WATER && oppInfo.type == Elements.Element.FIRE ||
                    playerInfo.type == Elements.Element.WOOD && oppInfo.type == Elements.Element.EARTH)
                {
                    playerWon = true;
                }
                else if (oppInfo.type == Elements.Element.FIRE && playerInfo.type == Elements.Element.METAL ||
                        oppInfo.type == Elements.Element.EARTH && playerInfo.type == Elements.Element.WATER ||
                        oppInfo.type == Elements.Element.METAL && playerInfo.type == Elements.Element.WOOD ||
                        oppInfo.type == Elements.Element.WATER && playerInfo.type == Elements.Element.FIRE ||
                        oppInfo.type == Elements.Element.WOOD && playerInfo.type == Elements.Element.EARTH)
                {
                    playerWon = false;
                }
                else if (playerInfo.value > oppInfo.value)
                {
                    playerWon = true;
                }
                else if (oppInfo.value > playerInfo.value)
                {
                    playerWon = false;
                }
                else
                {
                    wasTie = true;
                }

                //Delays and initiates clear stage
                StartCoroutine(InitiateClearingStage());
            }

            //8) Clear board
            if (clearingStage)
            {
                if (playerWon)
                {
                    //Gets type to find specific death animation and moves the opponents card to the center to play
                    Elements.Element cardType = selectedCard.GetComponent<CardInfo>().type;
                    StartCoroutine(MoveToCenter(opponentCard));
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            opponentCard.GetComponent<CardInfo>().deathType = cardType;
                            opponentCard.GetComponent<Animator>().SetTrigger("EDeath");
                            ++playerEarthWins;
                            break;
                        case Elements.Element.FIRE:
                            opponentCard.GetComponent<CardInfo>().deathType = cardType;
                            opponentCard.GetComponent<Animator>().SetTrigger("FDeath");
                            ++playerFireWins;
                            break;
                        case Elements.Element.METAL:
                            opponentCard.GetComponent<CardInfo>().deathType = cardType;
                            opponentCard.GetComponent<Animator>().SetTrigger("MDeath");
                            ++playerMetalWins;
                            break;
                        case Elements.Element.WATER:
                            opponentCard.GetComponent<CardInfo>().deathType = cardType;
                            opponentCard.GetComponent<Animator>().SetTrigger("WaDeath");
                            ++playerWaterWins;
                            break;
                        case Elements.Element.WOOD:
                            opponentCard.GetComponent<CardInfo>().deathType = cardType;
                            opponentCard.GetComponent<Animator>().SetTrigger("WoDeath");
                            ++playerWoodWins;
                            break;
                    }
                }
                //Gets type to find specific death animation and moves the players card to the center to play
                else if (!playerWon && !wasTie)
                {
                    Elements.Element cardType = opponentCard.GetComponent<CardInfo>().type;
                    StartCoroutine(MoveToCenter(selectedCard));
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            selectedCard.GetComponent<CardInfo>().deathType = cardType;
                            selectedCard.GetComponent<Animator>().SetTrigger("EDeath");
                            ++oppEarthWins;
                            break;
                        case Elements.Element.FIRE:
                            selectedCard.GetComponent<CardInfo>().deathType = cardType;
                            selectedCard.GetComponent<Animator>().SetTrigger("FDeath");
                            ++oppFireWins;
                            break;
                        case Elements.Element.METAL:
                            selectedCard.GetComponent<CardInfo>().deathType = cardType;
                            selectedCard.GetComponent<Animator>().SetTrigger("MDeath");
                            ++oppMetalWins;
                            break;
                        case Elements.Element.WATER:
                            selectedCard.GetComponent<CardInfo>().deathType = cardType;
                            selectedCard.GetComponent<Animator>().SetTrigger("WaDeath");
                            ++oppWaterWins;
                            break;
                        case Elements.Element.WOOD:
                            selectedCard.GetComponent<CardInfo>().deathType = cardType;
                            selectedCard.GetComponent<Animator>().SetTrigger("WoDeath");
                            ++oppWoodWins;
                            break;
                    }
                }
                //Delays and initiates points stage
                if (!wasTie)
                {
                    StartCoroutine(InitiatePointsStage());
                }
                //Fade both cards away because nobody won and initiate next stage immediately
                else
                {
                    selectedCard.GetComponent<Animator>().SetTrigger("Fade");
                    opponentCard.GetComponent<Animator>().SetTrigger("Fade");
                    overallWinner = false;
                    restartStage = true;
                }
            }

            //9) Award play points
            if (pointsStage)
            {
                if (playerWon)
                {
                    //Awards player respective point on their chart and fades card away
                    Elements.Element cardType = selectedCard.GetComponent<CardInfo>().type;
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            GameObject.FindGameObjectWithTag("PEP" + playerEarthWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.FIRE:
                            GameObject.FindGameObjectWithTag("PFP" + playerFireWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.METAL:
                            GameObject.FindGameObjectWithTag("PMP" + playerMetalWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WATER:
                            GameObject.FindGameObjectWithTag("PWaP" + playerWaterWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WOOD:
                            GameObject.FindGameObjectWithTag("PWoP" + playerWoodWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                    }
                    GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayCardPass3();
                    selectedCard.GetComponent<Animator>().SetTrigger("Fade");
                }
                //Awards opponent respective point on their chart and fades card away
                else
                {
                    Elements.Element cardType = opponentCard.GetComponent<CardInfo>().type;
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            GameObject.FindGameObjectWithTag("OEP" + oppEarthWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.FIRE:
                            GameObject.FindGameObjectWithTag("OFP" + oppFireWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.METAL:
                            GameObject.FindGameObjectWithTag("OMP" + oppMetalWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WATER:
                            GameObject.FindGameObjectWithTag("OWaP" + oppWaterWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WOOD:
                            GameObject.FindGameObjectWithTag("OWoP" + oppWoodWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                    }
                    GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayCardPass3();
                    opponentCard.GetComponent<Animator>().SetTrigger("Fade");
                }
                //Delay and initiate winner check stage
                StartCoroutine(InitiateWinCheckStage());
            }

            //10) Check for round winner
            if (wincheckStage)
            {
                //Moves played cards back to pile
                selectedCard.transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;
                opponentCard.transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;

                //Declare overall winner
                if ((playerEarthWins >= 1 && playerFireWins >= 1 && playerMetalWins >= 1 && playerWaterWins >= 1 && playerWoodWins >= 1) ||
                playerEarthWins == 3 || playerFireWins == 3 || playerMetalWins == 3 || playerWaterWins == 3 || playerWoodWins == 3)
                {
                    overallWinner = true;
                }
                else if ((oppEarthWins >= 1 && oppFireWins >= 1 && oppMetalWins >= 1 && oppWaterWins >= 1 && oppWoodWins >= 1) ||
                          oppEarthWins == 3 || oppFireWins == 3 || oppMetalWins == 3 || oppWaterWins == 3 || oppWoodWins == 3)
                {
                    overallWinner = true;
                }

                if (overallWinner)
                {
                    //14) End game
                    restartStage = false;
                    GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayWinGong();
                    GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>().Stop();
                    GameObject endScreen = GameObject.FindGameObjectWithTag("End");
                    //Display win or loss
                    if (playerWon)
                    {
                        endScreen.transform.Find("Win-Loss").GetComponent<TMPro.TextMeshPro>().text = "You Win!";
                    }
                    else
                    {
                        endScreen.transform.Find("Win-Loss").GetComponent<TMPro.TextMeshPro>().text = "You Lost...";
                    }
                    //Disable colliders and remove pause screen if open
                    GameObject[] objsToDisable = GameObject.FindGameObjectsWithTag("DisableEnd");
                    foreach(GameObject obj in objsToDisable)
                    {
                        obj.GetComponent<BoxCollider2D>().enabled = false;
                        Pause pauseComponent = obj.GetComponent<Pause>();
                        if(pauseComponent != null)
                        {
                            pauseComponent.UnpauseGame();
                        }
                    }
                    //Move end screen to camera
                    Vector3 screenPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, endScreen.transform.position.z);
                    endScreen.transform.position = screenPos;
                    wincheckStage = false;
                }
                else
                {
                    //Delay and initiate restart stage
                    StartCoroutine(InitiateRestartStage());
                }
            }

            if (restartStage)
            {
                //12) Draw cards
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayCardPass1();
                //Have player draw their card
                int playerReplacePos = -1;
                for (int i = 0; i < 5; i++)
                {
                    if (playerHand[i] == selectedCard)
                    {
                        playerReplacePos = i;
                    }
                }
                StartCoroutine(DealPlayer(1, playerReplacePos));
                //Have opponent draw their card
                int oppReplacePos = -1;
                for (int i = 0; i < 5; i++)
                {
                    if (opponentHand[i] == opponentCard)
                    {
                        oppReplacePos = i;
                    }
                }
                StartCoroutine(DealOpponent(1, oppReplacePos));

                //13) Restart
                Restart();
            }

            if (hasForfeited)
            {
                //Play end sound
                GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayWinGong();

                //Remove music
                Destroy(GameObject.FindGameObjectWithTag("Music"));

                //Move end game prompt to center of screen
                GameObject endScreen = GameObject.FindGameObjectWithTag("End");
                endScreen.transform.Find("Win-Loss").GetComponent<TMPro.TextMeshPro>().text = "You Lost...";
                Vector3 screenPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, endScreen.transform.position.z);
                endScreen.transform.position = screenPos;

                //Reset the booleans
                Restart();
                cardsDealt = 0;
                GameObject.FindGameObjectWithTag("Pause").SetActive(false);
                hasForfeited = false;
            }
        }
    }

    //Resets all of the booleans for next round; not reset for overall game
    private void Restart()
    {
        readyToStart = false;
        hasStarted = false;
        selectionStage = false;
        revealStage = false;
        boostStage = false;
        winnerStage = false;
        clearingStage = false;
        pointsStage = false;
        wincheckStage = false;
        restartStage = false;
        playerWon = false;
        wasTie = false;
        wasBoosted = false;
        overallWinner = false;
        selectedCard = null;
        opponentCard = null;
    }

    //Randomizes the deck for drawing
    private void ShuffleDeck(ref PlayerDeck deck)
    {
        for (int i = 0; i < 600; i++)
        {
            int pos1 = Random.Range(0, 50);
            int pos2 = Random.Range(0, 50);
            GameObject temp = deck.deck[pos1];
            deck.deck[pos1] = deck.deck[pos2];
            deck.deck[pos2] = temp;
        }
    }

    //Deals cards to the players with a delay for effect
    IEnumerator StartDealing()
    {
        //Delay code execution
        yield return new WaitForSeconds(2);

        //Deal five cards to each player
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(DealPlayer(i + 1, i));
            StartCoroutine(DealOpponent(i + 1, i));
        }
    }

    //Adds card to players hand and initiates movement from pile to hand
    IEnumerator DealPlayer(int waitTime, int handPos)
    {
        //Delay code execution
        yield return new WaitForSeconds(waitTime * 0.2f);

        //Set drawn cards position in hand
        playerHand[handPos] = playerDeck.deck[drawCount];
        playerDeck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;

        //Grab card positions and change order
        Vector2 cardPos = playerDeck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), -4.4f);
        playerDeck.deck[drawCount].GetComponent<SpriteRenderer>().sortingOrder = 4;

        //Start flipping card and moving to hand
        playerDeck.deck[drawCount].GetComponent<Animator>().SetTrigger("Flip");
        StartCoroutine(MoveCard(playerDeck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
    }

    //Adds card to opponents hand and initiates movement from pile to hand
    IEnumerator DealOpponent(int waitTime, int handPos)
    {
        //Delay code execution
        yield return new WaitForSeconds(waitTime * 0.2f);

        //Set drawn cards position in hand
        opponentHand[handPos] = opponentDeck.deck[drawCount];
        opponentDeck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;

        //Grab card positions and change order
        Vector2 cardPos = opponentDeck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), 4.5f);
        opponentDeck.deck[drawCount].GetComponent<SpriteRenderer>().sortingOrder = 4;

        //Initiate card movement
        StartCoroutine(MoveCard(opponentDeck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;

        //Loops back what cards to use starting from beginning
        if (drawCount == 49)
        {
            drawCount = 0;
            ShuffleDeck(ref playerDeck);
            ShuffleDeck(ref opponentDeck);
        }
        else
        {
            ++drawCount;
        }
    }

    //Lerp the card from one place to another and change sorting order to not poke above other cards
    IEnumerator MoveCard(GameObject card, Vector2 startPos, Vector2 destPos, Vector2 startSize, Vector2 endSize)
    {
        //Create timer variable
        float lerpTime = 0;

        //Play sound
        GameObject.FindGameObjectWithTag("SFX").GetComponent<SFX>().PlayCardPass1();

        //Change the cards sorting order to be higher then the other cards
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 6;
        card.GetComponent<SpriteRenderer>().sortingOrder = 7;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 8;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 8;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 8;

        //Move card
        while (lerpTime < 1.1f)
        {
            card.transform.position = Vector2.Lerp(startPos, destPos, lerpTime);
            card.transform.localScale = Vector2.Lerp(startSize, endSize, lerpTime);
            lerpTime += Time.deltaTime * 3;

            yield return null;
        }

        //Change the cards sorting order back to normal
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 3;
        card.GetComponent<SpriteRenderer>().sortingOrder = 4;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 5;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 5;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 5;
        card.transform.position = destPos;

        yield return null;
    }

    //Moves the card to the center of the board for destruction animation
    IEnumerator MoveToCenter(GameObject card)
    {
        //Create timer variable
        float lerpTime = 0;

        //Change the cards sorting order to be higher then the other cards
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 9;
        card.GetComponent<SpriteRenderer>().sortingOrder = 10;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 11;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 11;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 11;

        //Grab positions and rotations
        Vector2 startPos = card.transform.position;
        Vector2 destPos = new Vector2(-0.3f, 0);
        Vector2 startSize = card.transform.localScale;
        Vector2 destSize = new Vector2(0.25f, 0.25f);
        Quaternion startRot = card.transform.rotation;
        Quaternion destRot = Quaternion.identity;

        //Move card
        while(lerpTime < 1.1f)
        {
            card.transform.position = Vector2.Lerp(startPos, destPos, lerpTime);
            card.transform.localScale = Vector2.Lerp(startSize, destSize, lerpTime);
            card.transform.rotation = Quaternion.Lerp(startRot, destRot, lerpTime);
            lerpTime += Time.deltaTime * 3;

            yield return null;
        }

        yield return null;
    }

    //Delays and initiates reveal stage
    IEnumerator InitiateRevealStage()
    {
        selectionStage = false;

        yield return new WaitForSeconds(1);

        revealStage = true;
    }

    //Delays and initiates boost stage
    IEnumerator InitiateBoostStage()
    {
        revealStage = false;

        yield return new WaitForSeconds(1);

        boostStage = true;
    }

    //Delays and initiates winner stage
    IEnumerator InitiateWinnerStage()
    {
        boostStage = false;

        yield return new WaitForSeconds(0.5f);

        winnerStage = true;
    }

    //Delays and initiates clearing stage
    IEnumerator InitiateClearingStage()
    {
        winnerStage = false;

        yield return new WaitForSeconds(0.5f);

        clearingStage = true;
    }

    //Delays and initiates points stage
    IEnumerator InitiatePointsStage()
    {
        clearingStage = false;

        yield return new WaitForSeconds(2);

        pointsStage = true;
    }

    //Delays and initiates win check stage
    IEnumerator InitiateWinCheckStage()
    {
        pointsStage = false;

        yield return new WaitForSeconds(1);

        wincheckStage = true;
    }

    //Delays and initiates restart stage
    IEnumerator InitiateRestartStage()
    {
        wincheckStage = false;

        yield return new WaitForSeconds(0.5f);

        restartStage = true;
    }
}
