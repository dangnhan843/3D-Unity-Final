using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst ,Single};
    public FireMode fireMode;
    [Header("Effect")]
    public Transform shell;
    public Transform shellEjection;
    public Transform mag;
    public Transform magEjection;
    public Transform[] projectileSpawn;
    public Projectile projectile;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;
    public AudioClip reloadVoice;
    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(0.05f, 0.2f);
    public Vector2 recoilAngelMinMax = new Vector2(3.0f, 5.0f);

    public float recoilMoveSettleTime = 0.1f;
    public float recoilRotationSettleTime = 0.1f;
    public int burstCount;
    public int projectilePerMag;
    public float muzzleVelocity = 35.0f;
    public float msBetweenShots = 100.0f;
    public float reloadTime = 0.3f;
    int shotRemainingInBurst;
    int projectilesReaminingInMag;

    float nextShotTime;
    bool triggerReleasedSinceLastShot;
    bool isReloading;
    Vector3 recoilSmoothDampVelocity;
    float recoilAngel;
    float recoilRotSmoothDampVelocity;
    MuzzleFlash muzzleFlash;
    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotRemainingInBurst = burstCount;
        projectilesReaminingInMag = projectilePerMag;
    }
    private void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition,
            Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngel = Mathf.SmoothDamp(recoilAngel, 0, ref recoilRotSmoothDampVelocity,
            recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left*
            recoilAngel;
        if (!isReloading && projectilesReaminingInMag ==0)
        {
            Reload();
        }
    }
    void Shoot()
    {
        if ( !isReloading && Time.time > nextShotTime && projectilesReaminingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if(shotRemainingInBurst == 0)
                {
                    return;
                }
                shotRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if(projectilesReaminingInMag == 0)
                {
                    break;
                }
                projectilesReaminingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile,
                    projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }

                Instantiate(shell, shellEjection.position, shellEjection.rotation);
                muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward *
                Random.Range (kickMinMax.x, kickMinMax.y);
            recoilAngel += Random.Range(recoilAngelMinMax.x, recoilAngelMinMax.y);
            recoilAngel = Mathf.Clamp(recoilAngel, 0, 30);
            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
        
    }
    public void Reload()
    {
        if (!isReloading && projectilesReaminingInMag != projectilePerMag)
        {
            StartCoroutine(AnimatedReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
            AudioManager.instance.PlaySound(reloadVoice, transform.position);
            Instantiate(mag, magEjection.position, magEjection.rotation);
        }
    }

    IEnumerator AnimatedReload()
    {
        isReloading = true;

        yield return new WaitForSeconds(0.2f);
        float reloadSpeed = 1.0f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 90.0f;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-percent * percent + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
            yield return null;
        }
        isReloading = false;
        projectilesReaminingInMag = projectilePerMag;
    }
    public void Aim (Vector3 aimPoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }
    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotRemainingInBurst = burstCount;

    }


}
