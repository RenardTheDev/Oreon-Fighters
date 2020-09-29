using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWeapon : MonoBehaviour
{
    Ship ship;

    public ShipGun[] bulletWeapons;
    public ShipGun[] chamberWeapons;

    public bool triggerPrim;
    public bool triggerSec;

    public AudioSource SFX;

    private void Awake()
    {
        ship = GetComponent<Ship>();

        foreach (ShipGun g in bulletWeapons)
        {
            g.fireRate = 60f / g.data.rpm;
            //g.ps = g.pivot.GetComponentInChildren<ParticleSystem>();
        }
        foreach (ShipGun g in chamberWeapons)
        {
            g.fireRate = 60f / g.data.rpm;
            //g.ps = g.pivot.GetComponentInChildren<ParticleSystem>();
        }
    }

    private void Update()
    {
        
    }

    public delegate void MakeShotHandler(Vector3 pos, Vector3 dir, Ship shooter, Weapon weapon);
    public event MakeShotHandler ShipMadeShot;

    Dictionary<Weapon, int> gunSound = new Dictionary<Weapon, int>();
    private void FixedUpdate()
    {
        if (gunSound.Count > 0) gunSound.Clear();

        if (!ship.isAlive) return;

        if (triggerPrim)
        {
            Vector3 direction = transform.forward;
            for (int i = 0; i < bulletWeapons.Length; i++)
            {
                var gun = bulletWeapons[i];
                if (Time.time > gun.lastShot + gun.fireRate)
                {
                    direction = transform.forward * 100 + Random.onUnitSphere * gun.data.mainSpread;
                    ProjectileManager.singleton.SpawnProjectile(gun.pivot.position, direction.normalized, ship, gun.data);

                    ShipMadeShot?.Invoke(gun.pivot.position, direction.normalized, ship, gun.data);
                    //ShipMadeShot(gun.pivot.position, direction.normalized, ship, gun.data);

                    gun.lastShot = Time.time;
                    //gun.ps.Emit(5);

                    if (gunSound.ContainsKey(gun.data))
                    {
                        gunSound[gun.data]++;
                    }
                    else
                    {
                        gunSound.Add(gun.data, 1);
                    }
                }
            }
        }

        if (triggerSec)
        {
            Vector3 direction = transform.forward;
            for (int i = 0; i < chamberWeapons.Length; i++)
            {
                var gun = chamberWeapons[i];
                if (Time.time > gun.lastShot + gun.fireRate)
                {
                    direction = transform.forward * 100 + Random.onUnitSphere * gun.data.mainSpread;
                    ProjectileManager.singleton.SpawnProjectile(gun.pivot.position, direction.normalized, ship, gun.data);

                    ShipMadeShot?.Invoke(gun.pivot.position, direction.normalized, ship, gun.data);
                    //ShipMadeShot(gun.pivot.position, direction.normalized, ship, gun.data);

                    gun.lastShot = Time.time;
                    //gun.ps.Emit(5);

                    if (gunSound.ContainsKey(gun.data))
                    {
                        gunSound[gun.data]++;
                    }
                    else
                    {
                        gunSound.Add(gun.data, 1);
                    }
                }
            }
        }

        if (Vector3.Distance(Camera.main.transform.position, transform.position) < SFX.maxDistance)
        {
            foreach (KeyValuePair<Weapon, int> item in gunSound)
            {
                SFX.PlayOneShot(item.Key.shotSFX[Random.Range(0, item.Key.shotSFX.Length)]);
            }
        }
    }
}

[System.Serializable]
public class ShipGun
{
    public Transform pivot;
    public Weapon data;
    [HideInInspector] public float lastShot;
    [HideInInspector] public float fireRate;
    //[HideInInspector] public ParticleSystem ps;
}