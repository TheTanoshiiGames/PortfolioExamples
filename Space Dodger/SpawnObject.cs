using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**********************************************
 *Author: Noah Judge
 *Summary: Control the spawning of enemies and
 * items alike at variable rates based on game
 * speed
 *Date Created: Nov. 17th, 2021
 *Date Last Edited: Mar. 24th, 2022
 ***********************************************/
public class SpawnObject : MonoBehaviour
{
    //Variables
    public byte speed = 0;
    int playerHealth;
    bool triggerEnemySpawn = false;
    bool triggerItemSpawn = false;

    private void Start()
    {
        speed = GameObject.FindGameObjectWithTag("Data").GetComponent<GameData>().gameSpeed;
        StartCoroutine(triggerEnemy());
        StartCoroutine(triggerItem());
    }

    void Update()
    {
        //Get player health
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            playerHealth = player.GetComponent<ShipController>().health;
        }

        //Spawns meteors based on speed
        if (triggerEnemySpawn && player != null)
        {
            //Create a random spawn position
            float yPos = Random.Range(-2.5f, -9.5f);
            float xPos = Random.Range(-15.0f, -17.0f);

            //Random number to select which item to spawn
            int randNum = Random.Range(0, 100);

            if (randNum >= 0 && randNum <= 4)
            {
                SpawnUFO(yPos, xPos);
            }
            else
            {
                //Spawn Meteor
                SpawnMeteor(yPos, xPos);
            }

            //Reset Trigger bool
            triggerEnemySpawn = false;
            StartCoroutine(triggerEnemy());
        }

        if(triggerItemSpawn && player != null)
        {
            //Create a random spawn position
            float yPos = Random.Range(-2.5f, -9.5f);
            float xPos = Random.Range(-15.0f, -17.0f);

            //Random number to select which item to spawn
            int randNum = Random.Range(0, 100);
            
            //Spawn object
            if (randNum >= 0 && randNum <= 9 && playerHealth < 3)
            {
                SpawnItem("Life", yPos, xPos);
            }
            else if (randNum >= 10 && randNum <= 34 && player.GetComponent<ShipController>().hasShield == false)
            {
                SpawnItem("ShieldPowerup", yPos, xPos);
            }
            else if(randNum >= 35 && randNum <= 92)
            {
                SpawnItem("BlasterPowerup", yPos, xPos);
            }

            //Reset trigger bool
            triggerItemSpawn = false;
            StartCoroutine(triggerItem());
        }
    }

    //Spawns a meteor at position
    private void SpawnMeteor(float yPos, float xPos)
    {
        //Put the information together and spawn the meteor
        float angle = Random.Range(-0.3f, 0.3f);
        Vector2 direction = new Vector2(-1, angle);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);
        GameObject meteor = Instantiate(Resources.Load("Meteor"), spawnPos, new Quaternion(0, 0, 0, 0)) as GameObject;
        float meteorSize = Random.Range(0.75f, 1.25f);
        meteor.transform.localScale = new Vector3(meteorSize, meteorSize, 1);
        meteor.GetComponent<Rigidbody2D>().velocity = direction * speed * Random.Range(0.7f, 1.3f) * 2;
    }

    //Spawns a UFO at position
    private void SpawnUFO(float yPos, float xPos)
    {
        //Put the information together and spawn the UFO
        int intpolWidth = Random.Range(1, 4);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);
        GameObject ufo = Instantiate(Resources.Load("UFO"), spawnPos, new Quaternion(0, 0, 0, 0)) as GameObject;
        ufo.GetComponent<Float>().intpolWidth = intpolWidth;
        ufo.GetComponent<Rigidbody2D>().velocity = Vector2.left * speed * Random.Range(0.7f, 1.3f) * 2;
    }

    //Spawns specified item
    private void SpawnItem(string itemName, float yPos, float xPos)
    {
        //Put the information together and spawn the blaster
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);
        GameObject item = Instantiate(Resources.Load(itemName), spawnPos, new Quaternion(0, 0, 0, 0)) as GameObject;
        item.GetComponent<Rigidbody2D>().velocity = Vector2.left * speed * Random.Range(0.7f, 1.3f);
    }

    IEnumerator triggerEnemy()
    {
        //Get wait time based on speed
        switch (speed)
        {
            case 1:
                yield return new WaitForSeconds(1f);
                break;
            case 2:
                yield return new WaitForSeconds(0.75f);
                break;
            case 3:
                yield return new WaitForSeconds(0.5f);
                break;
        }
        //Trigger spawn
        triggerEnemySpawn = true;
    }

    IEnumerator triggerItem()
    {
        //Wait and trigger spawn
        yield return new WaitForSeconds(10);
        triggerItemSpawn = true;
    }
}