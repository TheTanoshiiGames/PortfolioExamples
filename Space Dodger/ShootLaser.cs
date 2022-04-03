using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**********************************************
*Author: Noah Judge
*Summary: Summons laser projectiles to shoot
*at enemies
*Date Created: Jan. 14th, 2022
*Date Last Edited: Jan. 24th, 2022
***********************************************/
public class ShootLasers : MonoBehaviour
{
    //Script variables
    const int SPEED = 10;
    public int shotsLeft = 6;
    GameObject[] laserGUI;
    AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        laserGUI = new GameObject[6];
        GetAmmoElements(ref laserGUI);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && shotsLeft > 0)
        {
            //Decrements shots
            --shotsLeft;

            //Update ammo GUI
            int index = 5 - shotsLeft;
            laserGUI[index].GetComponent<Image>().enabled = false;

            //Play sound
            source.Play();

            //Creates lasers for both blasters
            Vector3 currPos = transform.position;
            GameObject laser1 = Instantiate(Resources.Load("Laser"), currPos + new Vector3(0.475f, 0.312f, 0), new Quaternion(0, 0, 0, 0)) as GameObject;
            GameObject laser2 = Instantiate(Resources.Load("Laser"), currPos + new Vector3(0.475f, -0.312f, 0), new Quaternion(0, 0, 0, 0)) as GameObject;

            //Set velocity for lasers
            laser1.GetComponent<Rigidbody2D>().velocity = Vector2.right * SPEED;
            laser2.GetComponent<Rigidbody2D>().velocity = Vector2.right * SPEED;            
        }
        
        //Removes powerup if no shots remaining
        if(shotsLeft == 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<ShipController>().hasBlasters = false;
            Destroy(gameObject, 0.2f);
        }
    }

    private void GetAmmoElements(ref GameObject[] lasers)
    {
        //Grabs ammo images and stores them in laserGUI
        for(int i = 0; i < laserGUI.Length; i++)
        {
            laserGUI[i] = GameObject.Find("Ammo" + (i + 1).ToString());
        }
    }
}
