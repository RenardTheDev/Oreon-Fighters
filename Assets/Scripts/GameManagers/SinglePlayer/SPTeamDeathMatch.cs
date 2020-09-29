using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPTeamDeathMatch : SPGameManager
{
    public List<TeamScore> teams;

    public int botNumber = 19;

    public Text label_RoundTime;

    public bool roundStarted;
    public float roundTime;
    TimeSpan time;

    private void Awake()
    {
        AssignSingleton();

        StartCoroutine(_SpawnShips());

        foreach (TeamScore ts in teams)
        {
            ts.label_score.color = ts.team.TeamColor;
            ts.label_score.text = "0";
        }
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.transform.position.magnitude > 1200f)
            {
                DamageInfo dmg = new DamageInfo();

                dmg.direction = player.transform.position.normalized;
                dmg.hitPoint = player.transform.position;
                dmg.shooter = player;
                dmg.value = player.maxHealth;
                dmg.weapon = null;

                player.OnShipTakeDamage(dmg);
            }
        }

        if (roundStarted)
        {
            roundTime += Time.deltaTime;

            time = TimeSpan.FromSeconds(roundTime);
            label_RoundTime.text = string.Format("{0:00}:{1:00}", time.Minutes, time.Seconds);
        }
    }

    public override void OnPlayerKilled(Ship victim, DamageInfo damage)
    {
        base.OnPlayerKilled(victim, damage);
        teams.Find(x => x.team == damage.shooter.team).UpdateScore(1);
    }

    public override void OnBotKilled(Ship victim, DamageInfo damage)
    {
        base.OnBotKilled(victim, damage);
        teams.Find(x => x.team == damage.shooter.team).UpdateScore(1);
    }

    IEnumerator _SpawnShips()
    {
        yield return new WaitForSeconds(2f);

        SpawnPlayer().AssignTeam(teams[0].team);
        for (int i = 0; i < botNumber / 2f; i++)
        {
            SpawnBot().AssignTeam(teams[0].team);
        }
        for (int i = 0; i < botNumber / 2f; i++)
        {
            SpawnBot().AssignTeam(teams[1].team);
        }

        roundStarted = true;
    }
}

[System.Serializable]
public class TeamScore
{
    public Team team;
    public int score;
    public Text label_score;

    public void UpdateScore(int delta)
    {
        score += delta;
        label_score.text = score.ToString("0");
    }
}