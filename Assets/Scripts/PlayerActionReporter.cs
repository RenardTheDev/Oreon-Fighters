using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionReporter : MonoBehaviour
{
    public List<ActionRecordItem> action = new List<ActionRecordItem>();
    public Text reportLog;
    public Text reportScore;
    public Text reportKills;

    int score;
    int kills;

    public float reportTimeLength = 10f;

    private void Awake()
    {
        
    }

    private void Start()
    {
        SPGameManager.singleton.ActionRecord_Damage += Action_Damage;
        SPGameManager.singleton.ActionRecord_Kill += Action_Kill;
    }

    private void Update()
    {
        if (action.Count > 0) action.RemoveAll(x => Time.time - x.updated > reportTimeLength);    //remove old reports

        reportLog.text = "";
        foreach (var i in action)
        {
            switch (i.type)
            {
                case ActionType.damage:
                    {
                        var damAct = (ActionRecordDamage)i.item;
                        reportLog.text += "<color=#fff>+" + damAct.damage.ToString("0") + "</color> <size=20>Enemy hit</size>\n";
                        break;
                    }

                case ActionType.kill:
                    {
                        var killAct = (ActionRecordKill)i.item;


                        reportLog.text += "<color=#fff>+100</color> <size=20>Killed</size> <color=#F84>" + killAct.victim.PilotName + "</color>\n";
                        break;
                    }
            }
        }

        reportKills.text = "Kills <color=#fff>" + kills + "</color>";
        reportScore.text = "Score <color=#fff>" + score + "</color>";
    }

    void Action_Damage(Ship victim, DamageInfo damage)
    {
        var damageAction = action.Find(x => x.type == ActionType.damage);
        if (damageAction != null)
        {
            damageAction.updated = Time.time;
            ((ActionRecordDamage)damageAction.item).damage += damage.value;
        }
        else
        {
            action.Add(new ActionRecordItem(ActionType.damage, Time.time, new ActionRecordDamage(damage.value)));
        }

        score += Mathf.RoundToInt(damage.value);
    }

    void Action_Kill(Ship victim, DamageInfo damage)
    {
        action.Add(new ActionRecordItem(ActionType.kill, Time.time, new ActionRecordKill(victim)));
        kills++;
        score += 100;
    }
}

public class ActionRecordItem
{
    public ActionType type;
    public float updated;
    public object item;

    public ActionRecordItem(ActionType type, float updated, object item)
    {
        this.type = type;
        this.updated = updated;
        this.item = item;
    }
}
public class ActionRecordDamage
{
    public float damage;

    public ActionRecordDamage(float damage)
    {
        this.damage = damage;
    }
}
public class ActionRecordKill
{
    public Ship victim;

    public ActionRecordKill(Ship victim)
    {
        this.victim = victim;
    }
}

public enum ActionType
{
    damage, kill
}