using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRadarItemShip : MonoBehaviour
{
    public RectTransform trans;

    public GameObject nearGraphics;
    public GameObject farGraphics;
    public GameObject outGraphics;

    public Image healthBar;
    public Image[] pin;
    public Text distance;
    public Text nameTag;

    Ship ship;
    Transform ship_trans;

    Vector3 shipPos;

    Vector3 scrCenter;
    float scrRatio;

    Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        InvokeRepeating("SlowUpdate", 0, 0.3f);
    }

    [Header("Screen Object")]
    public float obj_distance;
    public float obj_diameter;
    public float obj_angularSize;
    public float obj_pixelSize;
    public Vector3 obj_scrPos;

    private void Update()
    {
        if (ship.isAlive)
        {
            //---Ship size---
            obj_distance = Vector3.Distance(ship_trans.position, mainCam.transform.position);
            obj_angularSize = (obj_diameter / obj_distance) * Mathf.Rad2Deg;
            obj_pixelSize = ((obj_angularSize * 1080f) / mainCam.fieldOfView);
            obj_pixelSize = Mathf.Clamp(obj_pixelSize, 32f, 256f);

            //---Ship position---
            shipPos = mainCam.WorldToViewportPoint(ship_trans.position);

            scrRatio = (float)Screen.width / Screen.height;
            shipPos.x = shipPos.x * 1080f * scrRatio;
            shipPos.y = shipPos.y * 1080f;
            float depth = shipPos.z;
            shipPos.z = 0;

            scrCenter = new Vector3(540f * scrRatio, 540f);

            Vector3 direction = shipPos - scrCenter;

            bool isOut = false;

            float sideSize = UIPlayerHUD.instance.sideSize;
            float farThreshold = UIPlayerHUD.instance.farThreshold;

            if(depth < 0)
            {
                shipPos = scrCenter + direction.normalized * 2000f;
            }

            if (shipPos.x < sideSize)
            {
                shipPos.x = sideSize; isOut = true;
            }
            if (shipPos.x > 1080f * scrRatio - sideSize)
            {
                shipPos.x = 1080f * scrRatio - sideSize; isOut = true;
            }
            if (shipPos.y < sideSize)
            {
                shipPos.y = sideSize; isOut = true;
            }
            if (shipPos.y > 1080f - sideSize)
            {
                shipPos.y = 1080f - sideSize; isOut = true;
            }

            if (isOut)
            {
                trans.anchoredPosition = shipPos;
                trans.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                trans.sizeDelta = new Vector2(64, 64);

                farGraphics.SetActive(false);
                nearGraphics.SetActive(false);
                outGraphics.SetActive(true);
            }
            else
            {
                trans.anchoredPosition = shipPos;
                trans.rotation = Quaternion.identity;
                trans.sizeDelta = new Vector2(obj_pixelSize, obj_pixelSize);

                farGraphics.SetActive(depth > farThreshold);
                nearGraphics.SetActive(depth <= farThreshold);
                outGraphics.SetActive(false);
            }

            /*if (direction.magnitude < 490f && depth > 0)
            {
                //---on screen---
                trans.anchoredPosition = shipPos;
                trans.rotation = Quaternion.identity;

                farGraphics.SetActive(depth > UIPlayerHUD.instance.farThreshold);
                nearGraphics.SetActive(depth <= UIPlayerHUD.instance.farThreshold);
                outGraphics.SetActive(false);
            }
            else
            {
                //---out of screen---
                trans.anchoredPosition = scrCenter + direction.normalized * 490f;
                trans.rotation = Quaternion.LookRotation(Vector3.forward, direction);

                farGraphics.SetActive(false);
                nearGraphics.SetActive(false);
                outGraphics.SetActive(true);
            }*/

            healthBar.fillAmount = ship.health / ship.maxHealth;

        }
        else
        {
            trans.anchoredPosition = Vector2.left * 1000f;
        }
    }

    void SlowUpdate()
    {
        if (ship.isAlive)
        {
            distance.text = (ship_trans.position - mainCam.transform.position).magnitude.ToString("0 m");
            nameTag.text = ship.PilotName;
            
            foreach (Image p in pin)
            {
                if (ship.team != null)
                {
                    p.color = ship.team.TeamColor;
                    
                }
                else
                {
                    p.color = Color.white;
                }
            }
        }
    }

    public void AssignShip(Ship ship)
    {
        this.ship = ship;
        ship_trans = ship.transform;

        Collider col = ship_trans.Find("bounds").GetComponent<Collider>();
        col.enabled = true;
        obj_diameter = col.bounds.extents.magnitude;
        col.enabled = false;
    }
}
