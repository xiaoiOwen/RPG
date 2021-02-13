using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{

    [Header("Skill")]
    public float kickForce = 25;

    // 石头人把主角击飞
    public void KickOff()
    {
        // 判定主角是否在自己正前方的扇区
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity  = direction * kickForce;
            // 把玩家击晕
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");

            // 产生伤害
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
