using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public Joystick joy;

    Ship player;
    ShipMotor playerMotor;
    ShipWeapon playerWeapon;

    CameraController camCtrl;
    
    public TouchButton[] buttons;

    public GameObject[] elements;

    public AnimationCurve stickInputCurve;
    public AnimationCurve gyroInputCurve;
    public Vector3 gyroInputScale;

    [Header("Inputs")]
    public ControllerType controller;
    [Space]
    public Vector3 steer;
    public bool triggerPrim;
    public bool triggerSec;
    public float thrust;

    private void Awake()
    {
        camCtrl = FindObjectOfType<CameraController>();

        debugTexture = new Texture2D(1, 1);
        debugTexture.SetPixel(0, 0, new Color(1f, 0f, 0f, 0.5f));
        debugTexture.Apply();

        if (Application.isMobilePlatform) controller = ControllerType.touch;
    }

    private void Start()
    {
        foreach (TouchButton b in buttons)
        {
            b.Update();
        }
    }

    private void Update()
    {
        if (controller == ControllerType.touch)
        {
            steer.x = -stickInputCurve.Evaluate(joy.Vertical);
            steer.y = stickInputCurve.Evaluate(joy.Horizontal);
            steer.z = gyroInputCurve.Evaluate(Input.acceleration.x * gyroInputScale.x);

            triggerPrim = false;
            triggerSec = false;

            foreach (TouchButton b in buttons)
            {
                if (Input.touchCount > 0)
                {
                    foreach (Touch t in Input.touches)
                    {
                        var tPos = t.position;
                        tPos.y = Screen.height - t.position.y;
                        if (b.guiarea.Contains(tPos))
                        {
                            if (!b.active)
                            {
                                b.lastActivID = t.fingerId;
                                b.active = true;
                                b.sprite.color = b.activeColor;
                            }

                            switch (b.type)
                            {
                                case TouchButtonType.Primary:
                                    {
                                        triggerPrim = true;
                                        break;
                                    }
                                case TouchButtonType.Secondary:
                                    {
                                        triggerSec = true;
                                        break;
                                    }
                                case TouchButtonType.Engine:
                                    {
                                        if (t.phase == TouchPhase.Began) thrust = thrust > 0.5f ? 0f : 1f;
                                        break;
                                    }
                                case TouchButtonType.Camera:
                                    {
                                        if (t.phase == TouchPhase.Began)
                                        {
                                            //freelook maybe
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (b.active && t.fingerId == b.lastActivID)
                            {
                                b.active = false;
                                b.sprite.color = b.normalColor;
                                b.lastActivID = -1;
                            }
                        }
                    }
                }
                else
                {
                    if (b.active)
                    {
                        b.active = false;
                        b.sprite.color = b.normalColor;
                    }
                }
            }

            if (playerMotor != null)
            {
                playerMotor.steer = steer;
                playerMotor.thrust = thrust;
            }
            if (playerWeapon != null)
            {
                playerWeapon.triggerPrim = triggerPrim;
                playerWeapon.triggerSec = triggerSec;
            }
        }

        if(controller == ControllerType.mousekeyboard)
        {
            if (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f)
            {
                mouseSteer.x += -Input.GetAxis("Mouse Y") * mouseSens;
                mouseSteer.y += Input.GetAxis("Mouse X") * mouseSens;
            }
            else
            {
                mouseSteer.x = Mathf.Lerp(mouseSteer.x, 0f, Time.deltaTime * 4f);
                mouseSteer.y = Mathf.Lerp(mouseSteer.y, 0f, Time.deltaTime * 4f);
            }
            mouseSteer = Vector2.ClampMagnitude(mouseSteer, 1f);

            steer.x = mouseSteer.x;
            steer.y = mouseSteer.y;
            steer.z = -Input.GetAxis("Roll");

            triggerPrim = Input.GetButton("Fire1");
            triggerSec = Input.GetButton("Fire2");

            if (Input.GetButtonDown("Engine")) thrust = thrust > 0.5f ? 0f : 1f;

            if (playerMotor != null)
            {
                playerMotor.steer = steer;
                playerMotor.thrust = thrust;
            }
            if (playerWeapon != null)
            {
                playerWeapon.triggerPrim = triggerPrim;
                playerWeapon.triggerSec = triggerSec;
            }
        }
    }

    Vector2 mouseSteer;
    public float mouseSens = 0.001f;

    [Header("Debug")]
    public Texture2D debugTexture;
    public bool DebugGUI;
    private void OnGUI()
    {
        if (DebugGUI)
        {
            foreach (TouchButton b in buttons)
            {
                b.Update();
                GUI.DrawTexture(b.guiarea, debugTexture);
            }
        }
    }

    void UpdateButtons()
    {

    }

    public void OnPlayerTakenDamage(Ship victim, DamageInfo damage)
    {

    }

    public void OnPlayerDestroyed(Ship victim, DamageInfo damage)
    {
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
        FindObjectOfType<UIPlayerHUD>().AssignPlayer(ship);
        camCtrl.AssignPlayer(ship);

        player = ship;
        playerMotor = player.GetComponent<ShipMotor>();
        playerWeapon = player.GetComponent<ShipWeapon>();

        player.OnShipTakeDamageEvent += OnPlayerTakenDamage;
        player.OnShipDestroyedEvent += OnPlayerDestroyed;
        player.OnShipSpawned += OnPlayerSpawned;

        OnPlayerSpawned(player);
    }
}

[System.Serializable]
public class TouchButton
{
    public string name = "button";
    public GameObject go;
    [HideInInspector] public Image sprite;
    [HideInInspector] public RectTransform trans;

    public Rect area;
    public Rect guiarea;

    public TouchButtonType type;
    public Color activeColor;
    public Color normalColor;
    public int lastActivID = -1;

    public bool active;

    public void Update()
    {
        if (sprite == null) sprite = go.GetComponent<Image>();
        if (trans == null) trans = go.GetComponent<RectTransform>();

        float ratio = Screen.height / 1920f;
        Vector2 pos = trans.position;
        Vector2 size = trans.sizeDelta * ratio * 1.765f;
        area = new Rect(pos - size * 0.5f, size);

        pos.y = Screen.height - pos.y;
        guiarea = new Rect(pos - size * 0.5f, size);
    }
}

public enum TouchButtonType
{
    Primary,
    Secondary,
    Engine,
    Camera
}

public enum ControllerType
{
    mousekeyboard,
    touch,
    gamepad
}