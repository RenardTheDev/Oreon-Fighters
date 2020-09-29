using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "team", menuName = "Scriptables/Team")]
public class Team : ScriptableObject
{
    public string TeamName = "Team";
    public Color TeamColor;
    public Material skin;
}
