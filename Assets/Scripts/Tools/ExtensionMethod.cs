using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 扩张方法不继承任何类，
// static 方便其他类调用
public static class ExtensionMethod
{

    // 两个向量 dot 运算 > 0.5f
    private const float dotThreshold = 0.5f;

    // this 后面的变量表示你要扩张的是哪个类
    // 逗号后面的才是参数
    public static bool IsFacingTarget(this Transform transform, Transform target)
    {
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);

        return dot >= dotThreshold;
    }
}
