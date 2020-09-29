using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "impact", menuName = "Scriptables/Impact effect")]
public class ImpactEffect : ScriptableObject
{
    //public string name;
    public GameObject prefab;

    public int minEmit;
    public int maxEmit;
}
