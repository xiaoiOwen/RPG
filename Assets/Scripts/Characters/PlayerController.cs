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

    // 主角移动停止位置, 1表示距离选中的点1的时候就停下来
    private float stopDistance;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponent<Animator>();

        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
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

        // 日常移动的时候结束距离要还原默认值，不用攻击距离
        agent.stoppingDistance = stopDistance;

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
        // 攻击的时候移动的结束距离改成攻击范围
        agent.stoppingDistance = characterStats.attackData.attackRange;


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
