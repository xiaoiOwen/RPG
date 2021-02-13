using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 创建泛型类
public class Singleton<T> : MonoBehaviour where T:Singleton<T>
{
    // Start is called before the first frame update
    private static T instance;

    public static T Instance
    {
        get {return instance;}
    }

    // 类的继承
    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;
    }

    // 当前类是否已经生成过了
    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
