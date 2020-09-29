using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ExplosionManager : MonoBehaviour
{
    public ImpactEffect explosionEffect;
    public static ExplosionManager singleton;
    public AudioClip explosionSFX;
    
    CinemachineImpulseSource explosionImpulser;

    private void Awake()
    {
        if (!singleton)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playaCam = Camera.main.transform;
        explosionImpulser = GetComponent<CinemachineImpulseSource>();
    }

    Collider[] affected;
    List<Rigidbody> affectedRigs = new List<Rigidbody>();
    public void SpawnExplosion(Vector3 point, float power, float radius)
    {
        //play sound
        SFXManager.singleton.SpawnSFX(point, explosionSFX, false, 1, 1, Mathf.Max(1000, testRadius));

        //show visuals
        ParticlesManager.singleton.SpawnEffect(explosionEffect, point, Vector3.up);

        //StartCoroutine(explosionShakeDelay((point - playaCam.position).magnitude / 343f, point, radius)); //with delay
        explosionImpulser.m_ImpulseDefinition.m_DissipationDistance = radius * 10; 
        transform.position = point;
        explosionImpulser.GenerateImpulse();

        //affect physics
        affected = Physics.OverlapSphere(point, radius);
        affectedRigs.Clear();
        foreach (var item in affected)
        {
            var rig = item.GetComponent<Rigidbody>();
            if (rig != null) affectedRigs.Add(rig);
        }

        foreach (var rig in affectedRigs)
        {
            rig.AddExplosionForce(power, point, radius, 0, ForceMode.Impulse);
        }
    }

    Transform playaCam;
    public float testPower = 100f;
    public float testRadius = 50f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnExplosion(playaCam.position + playaCam.forward * 100 + Random.insideUnitSphere * (testRadius + 10), testPower, testRadius);
        }
    }

    IEnumerator explosionShakeDelay(float time, Vector3 point, float radius)
    {
        yield return new WaitForSeconds(time);

        explosionImpulser.m_ImpulseDefinition.m_DissipationDistance = radius * 10;
        transform.position = point;
        explosionImpulser.GenerateImpulse();
    }
}
