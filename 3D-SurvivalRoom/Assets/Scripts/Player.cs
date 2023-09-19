using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5.0f;
    public Crosshairs crosshairs;
    GunController gunController;
    PlayerController controller;
    Camera viewCamera;

    protected override void Start()
    {
        base.Start();
        
    }
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumnber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumnber - 1);
    }
    // Update is called once per frame
    void Update()
    {
        // movement
        Vector3 moveInput = new Vector3
            (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // gun point direction
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

        if(groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin,point, Color.red);
            controller.LookAt(point);

            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x,
                transform.position.z)).magnitude > 1)
            {
                gunController.Aim(point);
            }
            
        }

        // gun 
        if(Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if ( Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
        if(transform.position.y  <-10)
        {
            TakeDamage(health);
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
