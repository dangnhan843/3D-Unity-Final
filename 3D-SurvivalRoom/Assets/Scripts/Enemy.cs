using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    LivingEntity targetEntity;

    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;
    Color originalColor;

    bool hasTarget;
    float damage = 1.0f;
    float attackDistanceThresold = 0.5f;
    float timeBetweenAttack = 1.0f;
    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    public static event System.Action OnDeathStatic;
    public enum State { Idle, Chasing, Attacking};
    public ParticleSystem deathEffect;
    
    State currentState;
    private void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();
        if (GameObject.FindWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
   
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }
    protected override void Start()
    {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();
        // change anim
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if (hasTarget)
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
    }
    public void SetCharacteristics (float moveSpeed, int hitsToKillPlayer, 
        float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
            startingHealth = enemyHealth;
            deathEffect.startColor = new Color(skinColor.r, skinColor.g,skinColor.b,1);
            skinMaterial = GetComponent<Renderer>().material;
            skinMaterial.color = skinColor;
            originalColor = skinMaterial.color;
            ChangeMaterial(skinMaterial);
        }
    }
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if(damage >= health)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation
                (Vector3.forward, hitDirection)), deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDistanceToTarget = (target.position - transform.position)
                    .sqrMagnitude;
                if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThresold +
                    myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttack;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }

    }
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position)
                    .normalized;
        Vector3 attackPosition = target.position - directionToTarget *
            (myCollisionRadius /*+ targetCollisionRadius*/);
        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;
        float percent = 0.0f;
        float attackSpeed = 3.0f;
        while (percent <= 1)
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation =  (-percent *percent + percent)*4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition,
                interpolation);
            yield return null;
        }
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.2f;
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position)
                    .normalized;
                Vector3 targetPosition = target.position - directionToTarget *
                    (myCollisionRadius + targetCollisionRadius + attackDistanceThresold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
    void ChangeMaterial(Material newMat)
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in children)
        {
            var mats = new Material[rend.materials.Length];
            for (var j = 0; j < rend.materials.Length; j++)
            {
                mats[j] = newMat;
            }
            rend.materials = mats;
        }
    }

}
