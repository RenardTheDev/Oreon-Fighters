using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISimpleController : MonoBehaviour
{
    Ship ship;
    ShipMotor motor;
    ShipWeapon weapon;

    Transform shipTrans;
    Vector3 shipPosition;
    Quaternion shipRotation;
    Vector3 targetPosition;
    Quaternion targetRotation;

    //Dictionary<Ship, float> enemies = new Dictionary<Ship, float>();
    public List<Ship> enemies = new List<Ship>();
    public Ship target;
    public Vector3 destination;
    float gotShotTime;
    
    public AiState state;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        motor = GetComponent<ShipMotor>();
        weapon = GetComponent<ShipWeapon>();

        ship.OnShipTakeDamageEvent += OnTakeDamage;
        ship.OnShipDestroyedEvent += OnDestroyed;
        ship.OnShipSpawned += OnSpawned;

        state = AiState.idle;

        shipTrans = transform;
    }

    private void Start()
    {
        InvokeRepeating("UpdateLogic", Random.value, 0.1f);

        GetNewDestination();
        gotShotTime = 0;

        ChangeAIState(AiState.idle);
    }

    private void OnSpawned(Ship ship)
    {
        GetNewDestination();
        gotShotTime = 0;

        ChangeAIState(AiState.idle);
    }

    private void OnTakeDamage(Ship victim, DamageInfo damage)
    {
        if (damage.shooter != null)
        {
            gotShotTime = Time.time;

            if (ship.health < 25)
            {
                ChangeAIState(AiState.flee);
            }
        }
    }

    private void OnDestroyed(Ship victim, DamageInfo damage)
    {
        gotShotTime = 0;

        ChangeAIState(AiState.idle);
    }

    private void GetNewDestination()
    {
        System.Random rand = new System.Random();
        //destination = Random.insideUnitSphere * 1000f;
        destination = new Vector3(rand.Next(-1000, 1000) * 0.001f, rand.Next(-1000, 1000) * 0.001f, rand.Next(-1000, 1000) * 0.001f);
        destination = destination.normalized * rand.Next(1000);
    }

    private bool IsGotToDestination()
    {
        return (shipPosition - destination).sqrMagnitude < 100f;
    }

    private List<Ship> CheckForEnemies()
    {
        enemies.Clear();

        //var nearby = Physics.OverlapSphere(shipPosition, 400f);
        foreach (var sh in SPGameManager.singleton.ships)
        {
            if (sh != null)
            {
                if (sh.team != ship.team && sh.isAlive)
                {
                    if (!enemies.Contains(sh))
                    {
                        enemies.Add(sh);
                    }
                }
            }
        }

        return null;
    }

    private bool IsEnemiesNear()
    {
        bool returnValue = false;

        if(enemies.Count > 0)
        {
            foreach (Ship s in enemies)
            {
                if ((s.transform.position-transform.position).sqrMagnitude < 10000)
                {
                    returnValue = true;
                    break;
                }
            }
        }

        return returnValue;
    }

    private void GetClosest()
    {
        if (enemies.Count > 1)
        {
            float dist = float.MaxValue;
            foreach (var s in enemies)
            {
                if (Vector3.Distance(shipPosition, s.transform.position) < dist)
                {
                    target = s;
                }
            }
        }
        else if (enemies.Count == 1)
        {
            target = enemies[0];
        }
        else
        {
            target = null;
        }
    }

    private void Update()
    {
        shipPosition = transform.position;
        shipRotation = transform.rotation;
        if (target != null)
        {
            targetPosition = target.transform.position;
            targetRotation = target.transform.rotation;
        }
        else
        {
            targetPosition = Vector3.zero;
        }

        switch (state)
        {
            case AiState.idle:
                {
                    ClearControls();
                    break;
                }
            case AiState.roaming:
                {
                    if (!IsGotToDestination())
                    {
                        UpdateControls();
                    }
                    break;
                }
            case AiState.attack:
                {
                    if (target != null)
                    {
                        if(Vector3.Distance(shipPosition, targetPosition) < 50f)
                        {
                            destination = shipPosition + ((shipPosition - targetPosition) * 200f);
                        }
                        else
                        {
                            destination = targetPosition;
                        }
                        UpdateControls();
                    }
                    break;
                }
            case AiState.flee:
                {
                    UpdateControls();
                    break;
                }

            default: { state = AiState.idle; break; }
        }
    }

    private void UpdateLogic()
    {
        weapon.triggerPrim = false;

        switch (state)
        {
            case AiState.idle:
                {
                    CheckForEnemies();

                    if (enemies.Count > 0)
                    {
                        ChangeAIState(AiState.attack);
                    }
                    else
                    {
                        ChangeAIState(AiState.roaming);
                    }
                    break;
                }
            case AiState.roaming:
                {
                    CheckForEnemies();
                    if (enemies.Count > 0)
                    {
                        ChangeAIState(AiState.attack);
                    }
                    else
                    {
                        if (IsGotToDestination())
                        {
                            ChangeAIState(AiState.idle);
                        }
                    }
                    break;
                }
            case AiState.attack:
                {
                    if (target == null || !target.isAlive)
                    {
                        ChangeAIState(AiState.idle);
                    }
                    else
                    {
                        UpdateControls();

                        Vector3 dirToEnemy = (destination - shipPosition);
                        dirToEnemy = transform.InverseTransformVector(dirToEnemy);
                        if (dirToEnemy.normalized.z > 0.99f && dirToEnemy.z < 500f && Random.value > 0.7f)
                        {
                            weapon.triggerPrim = true;
                        }
                    }
                    break;
                }
            case AiState.flee:
                {
                    break;
                }

            default: { state = AiState.idle; break; }
        }
    }

    void ChangeAIState(AiState newState)
    {
        state = newState;

        switch (state)
        {
            case AiState.idle:
                {
                    break;
                }
            case AiState.roaming:
                {
                    GetNewDestination();
                    break;
                }
            case AiState.attack:
                {
                    GetClosest();
                    break;
                }
            case AiState.flee:
                {
                    if (IsEnemiesNear())
                    {
                        Vector3 dangerDir = Vector3.zero;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            dangerDir += (shipPosition - enemies[i].transform.position);
                        }
                        dangerDir.Normalize();
                        Debug.DrawRay(shipPosition, dangerDir * 100f, Color.red, 0.1f);

                        destination = shipPosition + dangerDir;
                    }
                    break;
                }

            default: { state = AiState.idle; break; }
        }
    }

    RaycastHit hit;
    void UpdateControls()
    {
        Vector3 direction = Vector3.zero;

        if (Physics.SphereCast(transform.position + transform.forward * 10f, 5f, transform.forward, out hit, 10f))
        {
            direction = shipTrans.InverseTransformVector((shipPosition - (hit.point + hit.normal * 10f)).normalized);
        }
        else
        {
            direction = shipTrans.InverseTransformVector((destination - shipPosition).normalized);
        }

        if (direction.z < 0)
        {
            motor.thrust = 1f;
            direction.z = 0;
            direction.Normalize();
        }
        else
        {
            motor.thrust = 1f;
            direction.z = 0;
            direction.Normalize();
        }

        motor.steer.x = -direction.y;
        motor.steer.y = direction.x;
    }

    void ClearControls()
    {
        motor.thrust = 0;
        motor.steer = Vector3.zero;
    }
}

public enum AiState
{
    idle,
    roaming,
    attack,
    flee
}