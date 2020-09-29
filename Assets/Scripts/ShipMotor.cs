using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMotor : MonoBehaviour
{
    Ship ship;

    Rigidbody rig;

    public float engineForce;

    public float flySpeed;
    public float boostSpeed;
    public float steerSpeed;

    public float boost;

    public float thrust;
    float _thrust;
    public Vector3 steer;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        rig = GetComponent<Rigidbody>();
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        thrust = Mathf.Clamp01(thrust);

        steer.x = Mathf.Clamp(steer.x, -1, 1);
        steer.y = Mathf.Clamp(steer.y, -1, 1);
        steer.z = Mathf.Clamp(steer.z, -1, 1);

        _thrust = Mathf.Lerp(_thrust, thrust, Time.deltaTime);

        rig.velocity = transform.forward * engineForce * _thrust * Time.fixedDeltaTime;

        rig.angularVelocity = transform.TransformVector(steer * steerSpeed * Time.deltaTime);
    }

    /*private void OnGUI()
    {
        if (!CompareTag("Player")) return;

        GUILayout.Label("Speed: " + rig.velocity.magnitude.ToString("0.00m/s"));
    }*/
}
