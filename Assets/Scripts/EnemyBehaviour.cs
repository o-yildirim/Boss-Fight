﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public float enemySightAngle = 75f;
    public float enemySightRange = 20f;
    public float enemyReturningNormalTime = 1f;
    public float rotateSpeed = 4.5f;
    public float rocketCooldown = 9f;
    public float rocketTimeCounter;
    public float timeBetweenRockets = 1f;

    [SerializeField]
    private float playerLosingTimeCounter = 0f;
    public Transform playerPos;
    public bool playerOnSight = false;
    public bool bothArmsDestroyed = false;
    public Color angryEyeColor;
    public Color normalEyeColor;
    public GameObject[] eyes;
    public EnemyArm[] arms;
    private Rigidbody enemyRb;
    private NavMeshAgent agent;
    private Animator bossAnimator;
    private Vector3 playerLastSeenPos;
    
    


    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        bossAnimator = GetComponent<Animator>();
        normalEyeColor = eyes[0].GetComponent<Renderer>().material.color;
        rocketTimeCounter = rocketCooldown-1f;
       
    }


    void Update()
    {

        if (GameCoordinator.instance.isPaused)
        {
            return;
        }
        /*
        Enemies will be patrolling while there is no threat like the player.

        if (!playerOnSight)
        {
            patrol();
        }
        */

        rocketTimeCounter += Time.deltaTime;

        checkIsPlayerOnSight();

        //Incrementing that but when i see player, i reset that counter.
        playerLosingTimeCounter += Time.deltaTime;

        if (playerOnSight)
        {
            lookAtPlayer();
            float distanceBetweenPlayer = Vector3.Distance(transform.position, playerPos.position);
            if (distanceBetweenPlayer <= 3f && bothArmsDestroyed)
            {
                lookAtPlayer();
                //meleeAttack
               // Debug.Log("Hitting Player");
            }
            else if(distanceBetweenPlayer >= 3f && !bothArmsDestroyed)
            {
                enemyRb.velocity = Vector3.zero;
                if(rocketTimeCounter >= rocketCooldown)
                {
                    agent.isStopped = true; ;
                    lookAtPlayer();
                    StartCoroutine(fireRockets());
                    rocketTimeCounter = 0f;
                }
            }
            checkIsPlayerLost();
        }
    }



    public void checkIsPlayerOnSight()
    {
        //If player is in the angle of the sight of the enemy.
        float currentAngle = Vector3.Angle(playerPos.position - transform.position, transform.forward);
        if (currentAngle <= enemySightAngle)
        {
            //If there is anything between enemy and player like a wall or something so i cast a ray and check if that hits player except than a wall or something.
            //Also check if player is in the enemy sight range.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, playerPos.transform.position - transform.position, out hit, enemySightRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    playerSeen();
                }
            }
        }
    }

    public void checkIsPlayerLost()
    {
        //Player can be lost in two ways. In both ways, time must be passed for enemy to return normal.
        //First : Player could escape from sight angle of the enemy.
        //Second :  Player could go far away and gets out of enemy range.

        RaycastHit hit;



        if (playerLosingTimeCounter >= enemyReturningNormalTime)
        {
            //Calculating angle between enemy and player.
            float currentAngle = Vector3.Angle(playerPos.position, transform.forward);

            if (currentAngle > enemySightAngle)
            {
                //Check the distance, also if player is hid behind some walls or something.
                playerLost();
            }
            if (Physics.Raycast(transform.position, playerPos.position - transform.position, out hit, enemySightRange))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    playerLost();
                }
            }
        }

    }

    public void playerSeen()
    {
        playerOnSight = true;
        playerLastSeenPos = playerPos.position;
    
            

        
        playerLosingTimeCounter = 0f; //Saw the player so cooldown counter resets.
        
        //Turning the eye color to red.
        for (int i = 0; i < eyes.Length; i++)
        {
            eyes[i].GetComponent<Renderer>().material.color = angryEyeColor;
        }

        // agent.SetDestination(playerPos.position); //Chasing the player
        GameCoordinator.instance.playerIsBeingChased();





        //Harekete geçmesi için yarım saniye falan delay koy yoksa çok op.
    }
    public void playerLost()
    {
        playerOnSight = false;
        GameCoordinator.instance.playerIsSafe();
        //Turning the eye color to normal.
        for (int i = 0; i < eyes.Length; i++)
        {
            eyes[i].GetComponent<Renderer>().material.color = normalEyeColor;
        }
        agent.isStopped = false;
        agent.SetDestination(playerLastSeenPos);
    }
    
    public void lookAtPlayer()
    {
        Vector3 direction = playerPos.position - transform.position;
        Quaternion lookToRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookToRotation, rotateSpeed * Time.deltaTime);
     
    }

    public IEnumerator fireRockets()
    {

            if (bothArmsDestroyed)
            {
                yield break;
            }

        if (!bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("RocketLauncherSetUp"))
        {
            bossAnimator.SetBool("deployLaunchers", true);
            yield return new WaitForSeconds(bossAnimator.GetCurrentAnimatorStateInfo(0).length + bossAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.25f);
        }
           

            int i = 0;
                foreach (EnemyArm arm in arms)
                {
                    if (arm != null)
                    {
                        arm.fireRocket(playerPos);
                        yield return new WaitForSeconds(timeBetweenRockets);                     
                    }
                    else
                    {
                        i++;
                        if (i == arms.Length)
                        {
                            bothArmsDestroyed = true;
                    //----------------DENEME----------------
                    bossAnimator.SetBool("deployLaunchers", false);
                    bossAnimator.SetBool("pullSword", true);

                    //----------------DENEME----------------
                    
                        
                        //Kılıç çıkarma animasyonu tetikle
                    //Animasyon süresi kadar yield yap
                }
            }

                }         
    }

    public IEnumerator meleeAttack()
    {
        yield return null;
    }

  
}
