using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float lifetime = 3.0f;
    float damage = 1.0f;
    float speed = 10.0f;
    float skinWidth = 0.1f;
    public LayerMask collisionMask;
    public Color trailColor;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position,
            0.1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0],transform.position);
        }
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }
    public void SetSpeed (float newSpeed)
    {
        speed = newSpeed;
    }
    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    void CheckCollisions (float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if(Physics.Raycast (ray, out hit , moveDistance + skinWidth, 
            collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    

    void OnHitObject (Collider c, Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();
        if (damagableObject != null)
        {
            damagableObject.TakeHit(damage, hitPoint,transform.forward);
        }
        GameObject.Destroy(gameObject);
    }


}
