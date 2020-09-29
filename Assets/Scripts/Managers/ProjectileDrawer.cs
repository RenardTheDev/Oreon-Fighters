using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDrawer : MonoBehaviour
{
    public Mesh testMesh;
    public Material testMaterial;

    public float drawDist = 100;

    Matrix4x4 trs;
    //public List<Material> matsToCall;

    public Dictionary<Weapon, Material> matToRender;
    public Dictionary<Weapon, List<Projectile>> projToRender;

    private void OnPostRender()
    {
        //matsToCall.Clear();

        projToRender = new Dictionary<Weapon, List<Projectile>>();
        matToRender = new Dictionary<Weapon, Material>();

        foreach (Projectile p in ProjectileManager.singleton.proj)
        {
            if (p.isActive)
            {
                if ((transform.position - p.pos).sqrMagnitude < drawDist * drawDist)
                {
                    if (projToRender.ContainsKey(p.weapon))
                    {
                        projToRender[p.weapon].Add(p);
                    }
                    else
                    {
                        projToRender.Add(p.weapon, new List<Projectile>() { p });
                    }

                    if (matToRender.ContainsKey(p.weapon))
                    {
                        matToRender[p.weapon] = p.weapon.projMaterial;
                    }
                    else
                    {
                        matToRender.Add(p.weapon, p.weapon.projMaterial);
                    }

                    //if (!matsToCall.Contains(p.weapon.projMaterial)) matsToCall.Add(p.weapon.projMaterial);
                }
            }
        }

        foreach (Weapon w in matToRender.Keys)
        {
            matToRender[w].SetPass(0);

            foreach (Projectile p in projToRender[w])
            {
                trs.SetTRS(p.pos, Quaternion.LookRotation(p.dir), w.projSize);
                Graphics.DrawMeshNow(w.projMesh, trs);
            }
        }

        /*foreach (Material m in matsToCall)
        {
            m.SetPass(0);
        }

        foreach (Projectile p in ProjectileManager.singleton.proj)
        {
            if (p.isActive)
            {
                if ((transform.position - p.pos).sqrMagnitude < drawDist * drawDist)
                {
                    trs.SetTRS(p.pos, Quaternion.LookRotation(p.dir), p.weapon.projSize);

                    Graphics.DrawMeshNow(p.weapon.projMesh, trs);
                }
            }
        }*/
    }
}
