using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public static ParticlesManager singleton;

    public List<ImpactEffectItem> effects;

    private void Awake()
    {
        if (!singleton)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;

            foreach (ImpactEffectItem i in effects)
            {
                i.instance = Instantiate(i.impact.prefab, transform);
                i.effect = i.instance.GetComponent<ParticleSystem>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnEffect(ImpactEffect impact, Vector3 position, Vector3 normal)
    {
        if (impact == null) return;

        var effect = effects.Find(x => x.impact == impact);

        if (effect != null) effect.SpawnEffect(position, normal);
    }
}

[System.Serializable]
public class ImpactEffectItem
{
    public ImpactEffect impact;
    [HideInInspector] public GameObject instance;
    [HideInInspector] public ParticleSystem effect;

    public void SpawnEffect(Vector3 position, Vector3 direction)
    {
        instance.transform.position = position + direction * 0.1f;
        instance.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        effect.Emit(Random.Range(impact.minEmit, impact.maxEmit));
    }
}