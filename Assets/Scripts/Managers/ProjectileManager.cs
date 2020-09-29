using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager singleton;

    GameManager gManager;

    public GameObject prefab_projectile;
    public LayerMask hitMask;

    public List<Projectile> proj = new List<Projectile>();
    
    public float projLifeTime = 5f;

    public int preloadProjectiles = 250;

    public Weapon defaultWeap;

    public bool FriendlyFire = false;

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

        gManager = FindObjectOfType<GameManager>();

        CreateProjectile();
        /*for (int i = 0; i < preloadProjectiles; i++)
        {
            var id = CreateProjectile();
            proj[id].weapon = defaultWeap;
            HideProjectile(proj[id]);
        }*/

        //InvokeRepeating("UpdateProjectiles", 0, 1f / 24f);
    }

    Ship hitShip;
    Rigidbody hitRig;
    bool isHit;

    public bool debugTracers;
    public float debugTracersTime = 1f;
    private void FixedUpdate()
    {
        //UpdateProjectiles();
    }

    private void Update()
    {
        UpdateProjectiles();
    }

    List<RaycastHit> hitTest;
    List<Collider> overlap;
    Vector3 hitPoint;
    float delta;
    float lastUpdate;

    int pointer;

    void UpdateProjectiles()
    {
        delta = Time.time - lastUpdate;
        lastUpdate = Time.time;
        foreach (Projectile p in proj)
        {
            if (p.isActive)
            {
                //overlap = new List<Collider>(Physics.OverlapCapsule(p.pos(), p.pos() + p.dir() * p.speed * delta, p.weapon.physSize, hitMask.value));
                overlap = new List<Collider>(Physics.OverlapCapsule(p.pos, p.pos + p.dir * p.speed * delta, p.weapon.physSize, hitMask.value));

                isHit = false;

                foreach (Collider collider in overlap)
                {
                    //hitPoint = collider.ClosestPoint(p.pos());
                    hitPoint = collider.ClosestPoint(p.pos);

                    hitShip = collider.GetComponentInParent<Ship>();
                    hitRig = collider.GetComponentInParent<Rigidbody>();

                    if (hitShip != null)
                    {
                        if (hitShip != p.shooter)
                        {
                            //hitTest = new List<RaycastHit>(Physics.RaycastAll(p.pos(), (hitPoint - p.pos()).normalized, p.speed * delta * 1.5f));
                            /*hitTest = new List<RaycastHit>(Physics.RaycastAll(p.pos, (hitPoint - p.pos).normalized, p.speed * delta * 1.5f));
                            hitTest.RemoveAll(x => x.collider != collider);*/

                            OnProjectileHitShip(p, hitPoint, p.dir, hitShip);

                            isHit = true;

                            if (!hitShip.isAlive)
                            {
                                //hitRig.AddForceAtPosition(p.dir() * p.weapon.impact, p.pos(), ForceMode.Impulse);
                                hitRig.AddForceAtPosition(p.dir * p.weapon.impact, p.pos, ForceMode.Impulse);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //hitTest = new List<RaycastHit>(Physics.RaycastAll(p.pos(), (hitPoint - p.pos()).normalized, p.speed * delta * 1.5f));
                        /*hitTest = new List<RaycastHit>(Physics.RaycastAll(p.pos, (hitPoint - p.pos).normalized, p.speed * delta * 1.5f));
                        hitTest.RemoveAll(x => x.collider != collider);*/

                        if (hitRig != null)
                        {
                            //hitRig.AddForceAtPosition(p.dir() * p.weapon.impact, p.pos(), ForceMode.Impulse);
                            hitRig.AddForceAtPosition(p.dir * p.weapon.impact, p.pos, ForceMode.Impulse);
                        }
                        OnProjectileHit(p, hitPoint, p.dir);
                        isHit = true;
                        break;
                    }
                }

                if (isHit)
                {
                    continue;
                }
                else
                {
                    OnProjectileMiss(p);
                }
            }
        }
    }

    /*void UpdateProjectilesOLD()
    {
        foreach (Projectile p in proj)
        {
            if (!p.isActive) continue;
            if (p.isActive)
            {
                RaycastHit[] HIT = Physics.SphereCastAll(p.pos(), p.weapon.physSize, p.dir(), p.speed * Time.fixedDeltaTime, hitMask.value);

                if (debugTracers)
                {
                    int counter = 0;
                    foreach (RaycastHit hit in HIT)
                    {
                        if (hit.distance == 0) continue;
                        debugHitPoint.Add(new debugHitPoint(Time.time, hit, "[" + counter + "] " + hit.collider.name));
                        counter++;
                    }
                }

                isHit = false;

                //check hit array for proper contacts
                foreach (RaycastHit hit in HIT)
                {
                    if (hit.distance == 0) continue;

                    if (debugTracers)
                    {
                        Debug.DrawRay(hit.point, hit.normal, Color.green, debugTracersTime);
                        Debug.DrawRay(hit.point, Vector3.up, Color.yellow, debugTracersTime);
                    }

                    hitShip = hit.collider.GetComponentInParent<Ship>();
                    hitRig = hit.collider.GetComponentInParent<Rigidbody>();

                    if (hitShip != null)
                    {
                        if (hitShip != p.shooter)
                        {
                            OnProjectileHitShip(p, hit, hitShip);
                            isHit = true;
                            if (!hitShip.isAlive)
                            {
                                hitRig.AddForceAtPosition(p.dir() * p.weapon.impact, p.pos(), ForceMode.Impulse);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (hitRig != null)
                        {
                            hitRig.AddForceAtPosition(p.dir() * p.weapon.impact, p.pos(), ForceMode.Impulse);
                        }
                        OnProjectileHit(p, hit);
                        isHit = true;
                        break;
                    }
                }

                if (isHit)
                {
                    continue;
                }
                else
                {
                    OnProjectileMiss(p);
                }
            }
        }

        if (debugTracers) debugHitPoint.RemoveAll(x => x.time < Time.time - debugTracersTime);
    }*/

    Vector3 spot;
    private void OnGUI()
    {
        if (debugTracers)
        {
            for (int i = 0; i < debugHitPoint.Count; i++)
            {
                var dhp = debugHitPoint[i];
                spot = Camera.main.WorldToScreenPoint(dhp.rch.point);
                if (spot.z > 0)
                {
                    spot.y = Screen.height - spot.y;
                    GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
                    style.alignment = TextAnchor.UpperCenter;
                    GUI.Label(new Rect(spot.x - 64f, spot.y, 200f, 200f), "x\n" + dhp.name + "\n" + dhp.rch.distance.ToString("0.000"), style);


                    int activeCount = 0;
                    foreach (var item in proj)
                    {
                        if (item.isActive) activeCount++;
                    }
                    GUILayout.Space(128f);
                    GUILayout.Label("\nActive projectiles = " + activeCount + "/" + proj.Count);
                }
            }
        }
    }

    void OnProjectileHitShip(Projectile p, Vector3 point, Vector3 normal, Ship victim)
    {
        if (debugTracers)
        {
            //Vector3 travel = hit.point - p.trans.position;
            //Debug.DrawRay(p.trans.position, travel, Color.red, debugTracersTime);
            Vector3 travel = point - p.pos;
            Debug.DrawRay(p.pos, travel, Color.red, debugTracersTime);
        }

        DamageInfo dmg = new DamageInfo();

        dmg.shooter = p.shooter;
        dmg.weapon = p.weapon;
        dmg.hitPoint = point;
        //dmg.direction = p.dir();
        dmg.direction = p.dir;
        dmg.value = 0;
        if (p.shooter != null && victim != null)
        {
            if ((FriendlyFire && victim.team == p.shooter.team) || victim.team != p.shooter.team)
            {
                dmg.value = p.weapon.damage * 1.0f;

                ParticlesManager.singleton.SpawnEffect(p.weapon.impactEffect, point, normal);
                SFXManager.singleton.SpawnSFX(point, p.weapon.impactSFX[Random.Range(0, p.weapon.impactSFX.Length)], false, 0.05f, 10f, 100f);
            }
        }

        victim.OnShipTakeDamage(dmg);

        HideProjectile(p);
    }

    void OnProjectileHit(Projectile p, Vector3 point, Vector3 normal)
    {
        if (debugTracers)
        {
            //Vector3 travel = hit.point - p.trans.position;
            //Debug.DrawRay(p.trans.position, travel, Color.red, debugTracersTime);
            travel = point - p.pos;
            Debug.DrawRay(p.pos, travel, Color.red, debugTracersTime);
        }

        ParticlesManager.singleton.SpawnEffect(p.weapon.impactEffect, point, normal);

        HideProjectile(p);
    }

    List<debugHitPoint> debugHitPoint = new List<debugHitPoint>();

    Vector3 travel;
    void OnProjectileMiss(Projectile p)
    {
        //travel = (p.dir() * p.speed * Time.deltaTime);
        travel = (p.dir * p.speed * delta);
        if (debugTracers)
        {
            //Debug.DrawRay(p.trans.position, travel, Color.red, debugTracersTime);
            Debug.DrawRay(p.pos, travel, Color.red, debugTracersTime);
        }
        //p.trans.position = p.trans.position + travel;
        p.pos = p.pos + travel;

        p.travel += p.speed * Time.deltaTime;
        p.lifeTime += Time.deltaTime;

        if (p.travel >= p.weapon.maxTravel || p.lifeTime >= projLifeTime)
        {
            HideProjectile(p);
        }
    }

    void HideProjectile(Projectile p)
    {
        p.isActive = false;
        p.lifeTime = 0;
        p.travel = 0;
        p.shooter = null;
    }

    public void SpawnProjectile(Vector3 pos, Vector3 dir, Ship shooter, Weapon weapon)
    {
        int id = GetFreeProjectile();
        if (id == -1)
        {
            id = CreateProjectile();
        }

        //gManager.SendLocalPlayerShot(pos, dir, shooter, weapon);

        //proj[id].go.SetActive(true);

        proj[id].isActive = true;
        proj[id].shooter = shooter;
        proj[id].shWeapon = shooter.GetComponent<ShipWeapon>();
        //proj[id].trans.position = pos;
        proj[id].pos = pos;

        proj[id].weapon = weapon;
        proj[id].speed = weapon.projSpeed * (weapon.pelletCount > 1 ? Random.Range(0.9f, 1.1f) : 1f);

        //proj[id].entity.ShowProjectile(weapon);
        //proj[id].entity.SetProjectileSize(weapon.projSize);

        //proj[id].trans.rotation = Quaternion.LookRotation(dir, Vector3.up);
        proj[id].dir = dir;
    }

    int projCount;
    int GetFreeProjectile()
    {
        int returnValue = -1;

        for (int i = 0; i < projCount; i++)
        {
            if (!proj[i].isActive)
            {
                returnValue = i;
                break;
            }
        }

        return returnValue;
    }

    int CreateProjectile()
    {
        for (int i = 0; i < preloadProjectiles; i++)
        {
            var p = new Projectile();
            p.isActive = false;
            proj.Add(p);
            p.weapon = defaultWeap;
            HideProjectile(p);
        }

        projCount = proj.Count;
        return GetFreeProjectile();
    }
}

public class Projectile
{
    public Vector3 pos;
    public Vector3 dir;

    public Ship shooter;
    public ShipWeapon shWeapon;

    public float lifeTime;
    public float travel;
    public bool isActive;
    
    public float speed;
    public Weapon weapon;
}

public class debugHitPoint
{
    public float time;
    public string name;
    public RaycastHit rch;

    public debugHitPoint(float time, RaycastHit rch, string name)
    {
        this.time = time;
        this.rch = rch;
        this.name = name;
    }
}

#if (UNITY_EDITOR) 
[CustomEditor(typeof(ProjectileManager))]
public class ProjectileMotorEditor : Editor
{
    ProjectileManager script;
    public override void OnInspectorGUI()
    {
        /*script = (ProjectileManager)target;
        base.OnInspectorGUI();

        EditorGUILayout.Separator();
        
        int activeCount = 0;
        foreach (var item in script.proj)
        {
            if (item.isActive) activeCount++;
        }

        EditorGUILayout.Foldout(false, "Active projectiles = " + activeCount + "/" + script.proj.Count + "");*/
    }
}
#endif