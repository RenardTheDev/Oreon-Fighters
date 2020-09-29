using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipParts : MonoBehaviour
{
    Ship ship;
    public List<ShipPart> parts;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        ship.OnShipDestroyedEvent += OnShipDestroyed;
        ship.OnShipSpawned += OnShipSpawned;
    }

    void OnShipDestroyed(Ship victim, DamageInfo damage)
    {
        foreach (ShipPart p in parts)
        {
            if (p.dropOnExplosion)
            {
                var go = Instantiate(p.go, p.go.transform.position, p.go.transform.rotation);
                var rig = go.AddComponent<Rigidbody>();

                rig.mass = 50;
                rig.WakeUp();
                rig.AddExplosionForce(400, transform.position, 20f, 0, ForceMode.Impulse);

                Destroy(go, Random.Range(3f, 10f));
            }

            p.go.SetActive(false);
        }
    }

    void OnShipSpawned(Ship ship)
    {
        //ChangeSkin();
        foreach (ShipPart p in parts)
        {
            p.go.SetActive(true);
        }
    }

    public void ChangeSkin()
    {
        foreach (ShipPart p in parts)
        {
            p.go.GetComponent<Renderer>().material = ship.team.skin;
        }
    }
}

[System.Serializable]
public class ShipPart
{
    public GameObject go;
    public bool dropOnExplosion;

    public ShipPart(GameObject go)
    {
        this.go = go;
        dropOnExplosion = false;
    }
}

#if (UNITY_EDITOR) 
[CustomEditor(typeof(ShipParts))]
public class ShipPartsEditor : Editor
{
    ShipParts script;
    public override void OnInspectorGUI()
    {
        script = (ShipParts)target;
        base.OnInspectorGUI();

        EditorGUILayout.Separator();
        if (script.parts.Count < 1)
        {
            if (GUILayout.Button("Fill parts"))
            {
                script.parts = new List<ShipPart>();
                var trans = script.transform;

                for (int i = 0; i < trans.childCount; i++)
                {
                    script.parts.Add(new ShipPart(trans.GetChild(i).gameObject));
                }
            }
        }
    }
}
#endif