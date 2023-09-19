using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;
    public AudioClip shellAudio;

    float liftime = 4.0f;
    float facetime = 2.0f;
    void Start()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);
        StartCoroutine(Fade());
        AudioManager.instance.PlaySound(shellAudio, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(liftime);
        float percent = 0.0f;
        float fadeSpeed = 1 / facetime;
        Material mat = GetComponent<Renderer>().material;

        Color initialColor = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;

        }

        Destroy(gameObject);
    }
}
