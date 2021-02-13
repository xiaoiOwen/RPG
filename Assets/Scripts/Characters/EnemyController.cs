using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates{ GUARD, PATROL, CHASE, DEAD}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private Collider coll;


    private CharacterStats characterStats;

    [Header("Basic Settings")]
    public float sightRadius;// 可视范围
    public bool isGuard; // 守卫
    private float speed;

    protected GameObject attackTarget;
    public float lookAtTime;  // 走到一个巡逻点后停下来观察一段时间
    private float remainLookAtTime;  // 剩余观察时间

    private float lastAttackTime;  // 上次攻击时间

    private Quaternion guardRotation;  // 怪物初始旋转角度

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;  // 守卫最原始的位置


    // bool配合动画
    bool isWalk;
    bool isChace;
    bool isFollow;
    bool isDead;    // 自己是否死亡了
    bool playerDead;    // 主角是否死亡了

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim  = GetComponent<Animator>();
        coll  = GetComponent<Collider>();
        characterStats = GetComponent<CharacterStats>();
        speed = agent.speed;

        guardPos = transform.position;
        guardRotation = transform.rotation;

        remainLookAtTime = lookAtTime;

        
    }

    void Start() 
    {
        Debug.Log("EnemyController Start");
        if (isGuard)
        {
            // 守卫 站桩状态
            Debug.Log("守卫 站桩状态");
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            // 巡逻状态
            Debug.Log("巡逻状态");
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }

        // TODO 场景切换后修改掉
        GameManager.Instance.AddObserver(this);
    }

    // 切换场景时启用
    // void OnEnable()
    // {
    //     GameManager.Instance.AddObserver(this);
    // }

    // 人物消失、游戏停止的时候都会调用这个
    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;
        
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chace", isChace);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Die", isDead);
    }

    void SwitchStates()
    {
        // Debug.Log("SwitchStates 1 enemyStates = " + enemyStates);

        if (isDead)
            enemyStates = EnemyStates.DEAD;

        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            // Debug.Log("找到Player");
        }
        // Debug.Log("SwitchStates 2 enemyStates = " + enemyStates);

        // 如果发现 player 切换到CHASE
        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                // 守卫 站桩,
                isChace = false;
                // 回到辗转个位置
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    // 计算两个向量的距离
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        // 怪物缓慢的转向原来的朝向
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.02f);
                    }

                }
                break;
            case EnemyStates.PATROL:
                // 巡逻
                isChace = false;
                agent.speed = speed * 0.5f;
                
                // 判断是否到了随机巡逻点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if(remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                
                break;
            case EnemyStates.CHASE:
                // 追击
                // 回到上一个状态
                // 在攻击方位内则攻击
                // 配合动画
                isWalk = false;
                isChace = true;
                
                agent.speed = speed;

                if (!FoundPlayer())
                {
                    //拉脱回到上一个状态
                    isFollow = false;
                    if(remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                // 攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;

                        // 判定暴击
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        // 执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                coll.enabled  = false;
                // agent.enabled = false;
                agent.radius = 0;  // 范围为0, 这样怪物就不会成为障碍物了

                // 延迟2s销毁
                Destroy(gameObject, 2f);
                break;
        }

        // Debug.Log("SwitchStates 3 enemyStates = " + enemyStates);
    }

    void Attack()
    {
        // Debug.Log("Attack Begin");
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            // Debug.Log("Attack 近身攻击动画");
            // 近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            // Debug.Log("Attack 远程攻击动画");
            // 远程攻击动画
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }

        attackTarget = null;
        return false;
    } 

    bool TargetInAttackRange()
    {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    void GetNewWayPoint()
    {
        Debug.Log("GetNewWayPoint");
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        bool bCanWalk = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1);

        wayPoint = bCanWalk ? hit.position : transform.position;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    // Animation Event 处理攻击动画事件
    void Hit()
    {
        // 判定主角是否在自己正前方的扇区
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        // 获胜动画
        anim.SetBool("Win", true);
        // 主角已经死亡了
        playerDead = true;

        // 停止所有移动
        // 停止 Agent
        isChace = false;
        isWalk  = false;
        attackTarget = null;
        
        
    }

}

