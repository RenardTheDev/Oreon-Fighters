using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public string PilotName;

    Rigidbody rig;

    public float health;
    public float maxHealth = 100f;

    public bool isAlive;

    public Team team;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        health = maxHealth;

        PilotName = "Pilot#" + Random.Range(0, 10000).ToString("0000");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            var dmg = new DamageInfo();
            dmg.direction = -transform.forward;
            dmg.hitPoint = transform.forward + transform.position;
            dmg.shooter = this;
            dmg.value = Random.value * 20f;
            dmg.weapon = new Weapon();
            OnShipTakeDamage(dmg);
        }
    }

    public void OnShipTakeDamage(DamageInfo damage)
    {
        if (!isAlive) return;

        if (damage.value > health)
        {
            damage.value = health;  //last damage = health of ship
            OnShipDestroyed(damage);
            OnShipDestroyedEvent?.Invoke(this, damage);
        }
        else
        {
            health -= damage.value;
            OnShipTakeDamageEvent?.Invoke(this, damage);
        }
    }

    public void AssignTeam(Team newTeam)
    {
        team = newTeam;
        GetComponent<ShipParts>().ChangeSkin();
    }

    public delegate void ShipDamageHandler(Ship victim,DamageInfo damage);
    public event ShipDamageHandler OnShipTakeDamageEvent;
    public event ShipDamageHandler OnShipDestroyedEvent;

    public void OnShipDestroyed(DamageInfo damage)
    {
        health = 0;
        isAlive = false;

        rig.isKinematic = true;

        ExplosionManager.singleton.SpawnExplosion(transform.position, 1000f, 30f);
    }

    public delegate void SpawnHandler(Ship ship);
    public event SpawnHandler OnShipSpawned;

    public void SpawnShip()
    {
        health = maxHealth;
        isAlive = true;

        rig.isKinematic = false;
        
        OnShipSpawned.Invoke(this);
    }
}

public class DamageInfo
{
    public Ship shooter;
    public Weapon weapon;
    public Vector3 hitPoint;
    public Vector3 direction;
    public float value;
}