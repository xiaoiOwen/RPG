using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{

    public enum RockStates{HitPlayer, HitEnemy, HitNothing}
    private Rigidbody rb;
    public RockStates rockStates;

    [Header("Basic Settings")]
    public float force;
    // 石头基础伤害
    public int damage;
    public GameObject target;
    private Vector3 direction;
    // 扔出来的石头砸碎的效果
    public GameObject breakEffect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 石头生成的那一帧速度是0, 这里直接给他速度赋值1
        rb.velocity = Vector3.one;
        
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    void FixedUpdate()
    {
        // 判定石头的速度
        if (rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }
        // Debug.Log(rb.velocity.sqrMagnitude);
    }

    public void FlyToTarget()
    {
        // 避免生成石头后主句跑脱了
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    // 两个刚体碰撞的时候用来判断跟我相撞的是什么
    void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    // 停止主角移动
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    // 把主角击退
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction *force;
                    // 把主角眩晕
                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    // 产生伤害
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage,  other.gameObject.GetComponent<CharacterStats>());
                    rockStates = RockStates.HitNothing;
                }
                break;

            case RockStates.HitEnemy:
                // 只能攻击石头人
                if (other.gameObject.GetComponent<Golem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    // 生成石头砸碎的粒子效果
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
