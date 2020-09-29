using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerHUD : MonoBehaviour
{
    public static UIPlayerHUD instance;
    public float farThreshold = 100f;
    public float sideSize = 16f;

    public GameObject[] elements;

    Camera mainCam;
    Ship player;
    ShipWeapon playerWeap;

    //---uis---
    public Image health_bar;
    public Image health_dmg;
    float lastDmgTaken;

    public Image[] cs_dot;
    public Color cs_noHit;
    public Color cs_hasHit;

    //---Radar---
    public GameObject prefab_ShipRadarItem;
    public Transform radarShipParent;

    private void Awake()
    {
        instance = this;
        mainCam = Camera.main;
    }

    private void Start()
    {
        SPGameManager.singleton.OnShipSpawned += OnShipSpawned;
    }

    Ray cs_ray;
    RaycastHit cs_hit;
    Vector3 cs_point;
    float scrRatio;

    private void Update()
    {
        if (player == null) return;

        if (lastDmgTaken < Time.time - 1)
        {
            health_dmg.fillAmount = Mathf.MoveTowards(health_dmg.fillAmount, health_bar.fillAmount, Time.deltaTime);
        }


        scrRatio = (float)Screen.width / Screen.height;
        cs_point = Vector3.zero;

        for (int i = 0; i < cs_dot.Length; i++)
        {
            if (i >= playerWeap.bulletWeapons.Length)
            {
                cs_dot[i].rectTransform.anchoredPosition = Vector2.left * 1000f;
                continue;
            }
            else
            {
                var w = playerWeap.bulletWeapons[i];

                cs_ray = new Ray(w.pivot.position + w.pivot.forward, w.pivot.forward);
                if (Physics.Raycast(cs_ray, out cs_hit, 1000f))
                {
                    cs_dot[i].color = cs_hasHit;
                    cs_point = mainCam.WorldToViewportPoint(cs_hit.point);
                }
                else
                {
                    cs_dot[i].color = cs_noHit;
                    cs_point = mainCam.WorldToViewportPoint(w.pivot.position + w.pivot.forward * 1000f);
                }

                cs_point.x = cs_point.x * 1080f * scrRatio;
                cs_point.y = cs_point.y * 1080f;
                cs_point.z = 0;

                cs_dot[i].rectTransform.anchoredPosition = cs_point;
            }
        }
    }

    public void OnPlayerTakenDamage(Ship victim, DamageInfo damage)
    {
        if (player == null) return;

        Debug.Log("Player got damage");
        health_bar.fillAmount = player.health / player.maxHealth;
        lastDmgTaken = Time.time;
    }

    public void OnPlayerDestroyed(Ship victim, DamageInfo damage)
    {
        if (player == null) return;

        Debug.Log("Player got killed");

        foreach (var item in elements)
        {
            item.SetActive(false);
        }
    }

    public void OnPlayerSpawned(Ship ship)
    {
        foreach (var item in elements)
        {
            item.SetActive(true);
        }
    }

    public void AssignPlayer(Ship ship)
    {
        player = ship;
        playerWeap = player.GetComponent<ShipWeapon>();
        
        player.OnShipTakeDamageEvent += OnPlayerTakenDamage;
        player.OnShipDestroyedEvent += OnPlayerDestroyed;
        player.OnShipSpawned += OnPlayerSpawned;

        health_bar.fillAmount = player.health / player.maxHealth;
        health_dmg.fillAmount = health_bar.fillAmount;

        foreach (var item in elements)
        {
            item.SetActive(true);
        }
    }

    private void OnShipSpawned(Ship ship)
    {
        if (!ship.CompareTag("Player"))
        {
            var newItemGO = Instantiate(prefab_ShipRadarItem, radarShipParent);
            newItemGO.GetComponent<UIRadarItemShip>().AssignShip(ship);
        }
    }

    private void LateUpdate()
    {

    }
}