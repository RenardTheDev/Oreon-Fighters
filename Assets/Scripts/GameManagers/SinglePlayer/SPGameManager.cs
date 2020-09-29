using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPGameManager : MonoBehaviour
{
    public bool debugAI;

    public static SPGameManager singleton;

    PlayerControl lpControl;

    public GameObject[] shipPrefab;

    [HideInInspector] public Ship player;
    public List<Ship> ships = new List<Ship>();

    private void Awake()
    {
        AssignSingleton();
    }

    public virtual void AssignSingleton()
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

        lpControl = FindObjectOfType<PlayerControl>();
    }

    public virtual void OnPlayerKilled(Ship victim, DamageInfo damage)
    {
        RespawnPlayer(5f);
    }

    public virtual void OnBotKilled(Ship victim, DamageInfo damage)
    {
        if (damage.shooter != null)
        {
            if (damage.shooter.CompareTag("Player"))
            {
                ActionRecord_Kill.Invoke(victim, damage);
            }
        }

        RespawnBot(victim, 5f);
    }

    IEnumerator _respawnPlayer(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        player.transform.position = Random.onUnitSphere * 500f;
        player.SpawnShip();
    }

    IEnumerator _respawnBot(Ship ship, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        ship.transform.position = Random.onUnitSphere * 500f;
        ship.SpawnShip();
    }

    public virtual Ship SpawnPlayer()
    {
        var shipGO = Instantiate(shipPrefab[0], Random.onUnitSphere * 500f, Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f));
        player = shipGO.GetComponent<Ship>();
        shipGO.tag = "Player";

        lpControl.AssignPlayer(player);
        player.OnShipDestroyedEvent += OnPlayerKilled;
        player.OnShipTakeDamageEvent += OnAnyShipGotDamage;

        OnShipSpawned.Invoke(player);

        if (!ships.Contains(player)) ships.Add(player);

        return player;
    }

    public virtual void RespawnPlayer(float time)
    {
        StartCoroutine(_respawnPlayer(time));
    }

    public virtual Ship SpawnBot()
    {
        var go = Instantiate(shipPrefab[Random.Range(0, shipPrefab.Length)], Random.onUnitSphere * 500f, Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f));
        var ship = go.GetComponent<Ship>();

        ship.OnShipSpawned += OnAnyShipSpawned;

        ship.SpawnShip();
        ship.OnShipDestroyedEvent += OnBotKilled;
        ship.OnShipTakeDamageEvent += OnAnyShipGotDamage;
        go.AddComponent<AISimpleController>();

        OnShipSpawned.Invoke(ship);

        if (!ships.Contains(ship)) ships.Add(ship);

        return ship;
    }

    public virtual void RespawnBot(Ship ship, float time)
    {
        StartCoroutine(_respawnBot(ship, time));
    }

    public void OnAnyShipSpawned(Ship ship)
    {

    }

    public void OnAnyShipGotDamage(Ship ship, DamageInfo damage)
    {
        if (damage.shooter != null)
        {
            if (damage.shooter.CompareTag("Player"))
            {
                ActionRecord_Damage.Invoke(ship, damage);
            }
        }
    }

    public delegate void OnShipSpawnEventHandler(Ship ship);
    public event OnShipSpawnEventHandler OnShipSpawned;

    public delegate void ActionRecordHandler_Damage(Ship ship, DamageInfo damage);
    public event ActionRecordHandler_Damage ActionRecord_Damage;
    public delegate void ActionRecordHandler_Kill(Ship ship, DamageInfo damage);
    public event ActionRecordHandler_Kill ActionRecord_Kill;
}

public class ShipBot
{
    public GameObject go;
    public Ship ship;

    public ShipBot(GameObject go)
    {
        this.go = go;
        ship = go.GetComponent<Ship>();
    }
}