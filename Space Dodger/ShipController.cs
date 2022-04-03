using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**********************************************
*Author: Noah Judge
*Summary: Keeps track of the ships health and
* makes the ship follow your cursor
*Date Created: Jan. 20th, 2021
*Date Last Edited: Mar. 26th, 2022
***********************************************/

public class ShipController : MonoBehaviour
{
    //Script variables
    const float SHAKE_MAG = 0.1f;
    const int BAD_LAYER = 7;
    public int health = 3;
    public GameObject goScreen;
    public GameObject[] lives;
    public AudioClip explode;
    public AudioClip hit;
    public AudioClip power;
    public bool hasBlasters;
    public bool hasShield;
    float shipSpeed = 3;
    float speedDamp = 0.7f;
    float shakeDuration = 0;
    Camera cam;
    Vector3 initialPosition;
    Rigidbody2D rb;
    AudioSource sound;
    GameObject powerup;
    GameObject[] laserGUI;
    GameObject score;

    //Sets up needed components
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sound = GetComponent<AudioSource>();
        cam = FindObjectOfType<Camera>();
        initialPosition = cam.transform.position;
        hasBlasters = false;
        laserGUI = GameObject.FindGameObjectsWithTag("Ammo");
        score = GameObject.FindGameObjectWithTag("Score");

        foreach (GameObject img in laserGUI)
        {
            img.GetComponent<Image>().enabled = false;
        }
    }

    void Update()
    {
        //Camera shake on hit
        if (shakeDuration > 0f)
        {
            cam.transform.localPosition = initialPosition + Random.insideUnitSphere * SHAKE_MAG;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;
            cam.transform.localPosition = initialPosition;
        }
    }

    private void FixedUpdate()
    {
        //Grabs WASD input
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        //Dampens diagnal speed
        if(vertical != 0 && horizontal != 0)
        {
            vertical *= speedDamp;
            horizontal *= speedDamp;
        }
        Vector2 newPos = new Vector2(horizontal * shipSpeed, vertical * shipSpeed);
        rb.velocity = newPos;
        //Stops player if no controls are being held
        if(vertical == 0 && horizontal == 0)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //Hurt player after collision with obstacle
        if(col.gameObject.layer == BAD_LAYER && health > 0)
        {
            //Destroy collision object
            col.gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            col.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            if (col.gameObject.tag == "Meteor")
            {
                col.gameObject.GetComponent<ParticleSystem>().Play();
                col.gameObject.GetComponent<AudioSource>().Play();
                Destroy(col.gameObject, 0.2f);
            }
            else if (col.gameObject.tag == "UFO")
            {
                GameObject explosion = Instantiate(Resources.Load("Explosion"), col.transform.position, new Quaternion(0, 0, 0, 0)) as GameObject;
                explosion.GetComponent<AudioSource>().Play();
                Destroy(col.gameObject);
            }

            //Destroys shield or hurts player
            if (hasShield)
            {
                hasShield = false;
                Destroy(GameObject.FindGameObjectWithTag("SOverlay"));
                lives[3].GetComponent<Image>().enabled = false;
            }
            else
            {
                lives[health - 1].GetComponent<Image>().enabled = false;
                health -= 1;
                sound.clip = hit;
                sound.Play();
                shakeDuration = 0.1f;
                //Destroys player if they lose all health
                if (health < 1)
                {
                    gameObject.layer = 7;
                    GameObject explosion = Instantiate(Resources.Load("Explosion"), transform.position, new Quaternion(0, 0, 0, 0)) as GameObject;
                    explosion.GetComponent<AudioSource>().Play();
                    KillPlayer();
                }
            }
            
        }
        //Collects Blaster powerup
        else if (col.gameObject.tag == "Blaster")
        {
            sound.clip = power;
            sound.Play();
            Destroy(col.gameObject);
            //Replenishes shots if player already has blaster
            if (hasBlasters)
            {
                GetComponentInChildren<ShootLasers>().shotsLeft = 6;
            }
            //Gives player blaster if they don't already have one
            else
            {
                powerup = Instantiate(Resources.Load("Ship_Blaster"), gameObject.transform) as GameObject;
                hasBlasters = true;
            }
            //Update ammo GUI
            foreach(GameObject img in laserGUI)
            {
                img.GetComponent<Image>().enabled = true;
            }

        }
        else if(col.gameObject.tag == "Life")
        {
            sound.clip = power;
            sound.Play();
            Destroy(col.gameObject);
            //Relenishes lost lives
            if(health < 3)
            {
                ++health;
                lives[health - 1].GetComponent<Image>().enabled = true;
            }
        }
        else if(col.gameObject.tag == "Shield")
        {
            sound.clip = power;
            sound.Play();
            Destroy(col.gameObject);
            hasShield = true;
            Instantiate(Resources.Load("Overlay"), this.transform);
            lives[3].GetComponent<Image>().enabled = true;
        }
    }

    //Destroys the player
    private void KillPlayer()
    {
        if (hasBlasters)
        {
            hasBlasters = false;
            Destroy(GetComponentInChildren<ShootLasers>().gameObject);
        }
        goScreen.transform.localPosition = Vector2.zero;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        int currScore = int.Parse(score.GetComponent<Text>().text.Substring(7));
        if (highscore < currScore)
        {
            PlayerPrefs.SetInt("Highscore", currScore);
            PlayerPrefs.Save();
        }
        Destroy(gameObject);
    }
}
