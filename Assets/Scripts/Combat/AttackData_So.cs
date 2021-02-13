using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Attack/Attack Data")]

public class AttackData_So : ScriptableObject
{
    [Header("Stats Info")]

    public float attackRange;
    public float skillRange;
    public float coolDown;
    public int minDamage;
    public int maxDamage;

    public float criticalMultiplier;    // 暴击后加成百分比
    public float criticalChance;    // 暴击概率 1= 100%， 0.2 = 20%
}
