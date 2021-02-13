using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;
    private CharacterStats characterStats;

    private GameObject attackTarget;
    private float lastAttackTime;  //上一次攻击时间点
    private bool isDead;



    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponent<Animator>();

        characterStats = GetComponent<CharacterStats>();
    }

    void Start() {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;

        GameManager.Instance.RigisterPlayer(characterStats);
    }

    

    void Update(){
        isDead = characterStats.CurrentHealth == 0;

        // 广播主角死亡了
        if (isDead)
            GameManager.Instance.NotifyObservers();

        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Die", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;

        agent.isStopped   = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;
        
        if (target != null){
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

        transform.LookAt(attackTarget.transform);
        
        // TOTO:修改攻击范围参数
        while(Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange){
            agent.destination = attackTarget.transform.position;
            yield return null;
            
        }

        agent.isStopped = true;
        // Attack
        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            // 重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    // Animation Event 处理攻击动画事件
    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();

        targetStats.TakeDamage(characterStats, targetStats);
    }
}
