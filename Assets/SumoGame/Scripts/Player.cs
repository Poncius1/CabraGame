using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour

{
    private Rigidbody rb;
    public float speed = 3.0f;
    private GameObject FocalPoint;
    public bool hasPowerup = false;
    public GameObject PowerUpIndicator;



    public PowerUpType currentPowerUp = PowerUpType.None;
    public GameObject rocketPrefab;
    private GameObject tmpRocket;
    private Coroutine powerupCountdown;



    public float hangTime;
    public float smashSpeed;
    public float explosionForce;
    public float explosionRadius;
    bool smashing = false;
    float floorY;




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FocalPoint = GameObject.Find("Focal Point");
    }

    // Update is called once per frame
    void Update()
    {
        float forwardInput = Input.GetAxisRaw("Vertical");
        rb.AddForce(FocalPoint.transform.forward * forwardInput * speed);
        PowerUpIndicator.transform.position = transform.position + new Vector3(0,-0.5f,0);


        if (currentPowerUp == PowerUpType.Rockets && Input.GetKeyDown(KeyCode.Space))
        {
            LaunchRockets();
        }

        if (currentPowerUp == PowerUpType.Smash && Input.GetKeyDown(KeyCode.E) && !smashing)
        {
            smashing = true;
            StartCoroutine(Smash());
        }

    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            hasPowerup = true;
            currentPowerUp = other.gameObject.GetComponent<PowerUps>().powerUpType;

            PowerUpIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            StartCoroutine(PowerUpRoutine());


            if (powerupCountdown != null)
            {
                StopCoroutine(powerupCountdown);
            }
            powerupCountdown = StartCoroutine(RocketPowerUp());

        }
    }


    IEnumerator RocketPowerUp()
    {
        yield return new WaitForSeconds(3);
        hasPowerup = false;
        currentPowerUp = PowerUpType.None;
        PowerUpIndicator.gameObject.SetActive(false);
    }



    IEnumerator PowerUpRoutine()
    {
        yield return new WaitForSeconds(5);
        hasPowerup = false;
        PowerUpIndicator.gameObject.SetActive(false);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && currentPowerUp == PowerUpType.Pushback)
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;

            enemyRigidbody.AddForce(awayFromPlayer * 10, ForceMode.Impulse);

            Debug.Log("Player collided with: " + collision.gameObject.name + " with powerup set to " + currentPowerUp.ToString());
        }
    }




    void LaunchRockets()
    {
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            tmpRocket = Instantiate(rocketPrefab, transform.position + Vector3.up,
            Quaternion.identity);
            tmpRocket.GetComponent<Rocket>().Fire(enemy.transform);
        }
    }




    IEnumerator Smash()
    {
        var enemies = FindObjectsOfType<Enemy>();
        //Store the y position before taking off
        floorY = transform.position.y;

        //Calculate the amount of time we will go up
        float jumpTime = Time.time + hangTime;


        while (Time.time < jumpTime)
        {
            //move the player up while still keeping their x velocity.
            rb.velocity = new Vector2(rb.velocity.x, smashSpeed);
            yield return null;
        }


        //Now move the player down
       
        while (transform.position.y > floorY)
        {
            rb.velocity = new Vector2(rb.velocity.x, -smashSpeed * 2);
            yield return null;
        }
        //Cycle through all enemies.
        for (int i = 0; i < enemies.Length; i++)
        {
            //Apply an explosion force that originates from our position.
            if (enemies[i] != null)
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce,
                transform.position, explosionRadius, 0.0f, ForceMode.Impulse);
        }
        //We are no longer smashing, so set the boolean to false
        smashing = false;

    }

}
