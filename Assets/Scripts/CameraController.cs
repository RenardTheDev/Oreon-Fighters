using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    new Camera camera;

    Animator states;
    CinemachineStateDrivenCamera cinema;


    private void Awake()
    {
        camera = Camera.main;

        states = GetComponent<Animator>();
        cinema = GetComponent<CinemachineStateDrivenCamera>();
    }

    private void Update()
    {

    }

    public void AssignPlayer(Ship player)
    {
        cinema.m_Follow = player.transform;
        cinema.m_LookAt = player.transform;

        player.OnShipTakeDamageEvent += OnPlayerTakenDamage;
        player.OnShipDestroyedEvent += OnPlayerDestroyed;
        player.OnShipSpawned += OnPlayerSpawned;
    }

    public void OnPlayerTakenDamage(Ship victim, DamageInfo damage)
    {

    }

    public void OnPlayerDestroyed(Ship victim, DamageInfo damage)
    {
        cinema.m_Follow = null;
        cinema.m_LookAt = damage.shooter.transform;
    }

    public void OnPlayerSpawned(Ship ship)
    {
        cinema.m_Follow = ship.transform;
        cinema.m_LookAt = ship.transform;
    }
}