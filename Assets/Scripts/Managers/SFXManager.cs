using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager singleton;

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

        playerCamera = Camera.main.transform;
    }

    public List<sfxItem> items = new List<sfxItem>();
    public GameObject prefab;
    Transform playerCamera;

    private void Update()
    {
        foreach (sfxItem sfx in items)
        {
            if (sfx.isActive)
            {
                if (sfx.src.isPlaying)
                {

                }
                else
                {
                    sfx.src.Stop();
                    sfx.isActive = false;
                }
            }
        }
    }

    public void SpawnSFX(Vector3 pos, AudioClip clip, bool useDelay = false, float volume = 1f, float minDist = 1f, float maxDist = 100f)
    {
        if (Vector3.Distance(pos, playerCamera.position) > maxDist) return;

        int id = GetFreeItem();
        if (id == -1)
        {
            id = CreateItem();
        }

        var sfx = items[id];

        sfx.trans.position = pos;

        sfx.isActive = true;

        sfx.src.minDistance = minDist;
        sfx.src.maxDistance = maxDist;

        sfx.src.clip = clip;
        sfx.src.volume = volume;
        if (useDelay)
        {
            sfx.src.PlayDelayed((pos - playerCamera.position).magnitude / 343f);
        }
        else
        {
            sfx.src.PlayOneShot(clip);
        }
    }

    int GetFreeItem()
    {
        int returnValue = -1;

        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].isActive)
            {
                returnValue = i;
                break;
            }
        }

        return returnValue;
    }

    int CreateItem()
    {
        sfxItem p = new sfxItem(Instantiate(prefab, transform));
        p.isActive = false;

        items.Add(p);

        return items.IndexOf(p);
    }
}

public class sfxItem
{
    public GameObject go;
    public Transform trans;
    public AudioSource src;

    public bool isActive;

    public sfxItem(GameObject gameObject)
    {
        go = gameObject;
        trans = go.transform;
        src = go.GetComponent<AudioSource>();

        isActive = false;
    }
}

#if (UNITY_EDITOR) 
[CustomEditor(typeof(SFXManager))]
public class SFXManagerEditor : Editor
{
    SFXManager script;
    public override void OnInspectorGUI()
    {
        script = (SFXManager)target;
        base.OnInspectorGUI();

        EditorGUILayout.Separator();

        int activeCount = 0;
        foreach (var item in script.items)
        {
            if (item.isActive) activeCount++;
        }

        EditorGUILayout.Foldout(false, "Active sfxItems = " + activeCount + "/" + script.items.Count + "");
    }
}
#endif