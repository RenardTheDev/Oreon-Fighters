using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "weapon", menuName = "Scriptables/Weapon")]
public class Weapon : ScriptableObject
{
    public string WeaponName = "The Gun";
    public AudioClip[] shotSFX;
    public AudioClip[] impactSFX;
    public Mesh projMesh;
    public Material projMaterial;
    public ImpactEffect impactEffect;

    public WeaponType type;
    public int clipSize = 10;

    public float impact = 20f;
    public float damage = 10f;
    public float rpm = 600f;
    public float reloadTime = 1.8f;
    public float maxTravel = 1000f;

    public float projSpeed = 250f;
    //public float projSize = 0.1f;
    public Vector3 projSize;
    public float physSize = 7.62f;
    public float projTrailLength = 0.1f;

    public int burstCount = 1;          //1 - for auto fire
    public float burstDelay = 0.1f;

    public int pelletCount = 1;         //shotguns > 1

    public float mainSpread = 0f;       //spread of shot
    public float pelletSpread = 0f;     //spread of pellets in one shot
}

[Flags]
public enum WeaponType : byte
{
    Melee = 1,
    Pistol = 2,
    SMG = 3,
    Shotgun = 4,
    Rifle = 5,
    Sniper = 6,
    Heavy = 7,
    Throwable = 8
}

#if (UNITY_EDITOR) 
[CustomPropertyDrawer(typeof(Enum), true)]
public sealed class EnumPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            if (HasEnumFlagsAttribute())
            {
                var intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);

                if (property.intValue != intValue)
                {
                    property.intValue = intValue;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        bool HasEnumFlagsAttribute()
        {
            var fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray)
            {
                var elementType = fieldType.GetElementType();

                return elementType.IsDefined(typeof(FlagsAttribute), false);
            }

            return fieldType.IsDefined(typeof(FlagsAttribute), false);

        }
    }
}
#endif