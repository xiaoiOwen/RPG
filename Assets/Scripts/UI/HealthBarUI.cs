using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public Transform barPoint;
    // 血条是否长久可见
    public bool alwaysVisible;
    // 血条不是长久可见的话, 要设置血条显示时间
    public float visibleTime;
    // 通过计时器处理血条剩余可显示时间
    private float timeLeft;


    Image healthSlider;
    Transform UIBar;
    // 摄像机位置, 保证始终看到血条
    Transform cam;
    // 角色当前状态, 用来更新血条显示
    CharacterStats currentStats;

    void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        // 添加事件监听函数
        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    // 人物启动的时候就就会调用这个方法
    void OnEnable()
    {
        cam = Camera.main.transform;

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            // 判断 canvas 是不是世界坐标渲染的
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                // 生成血条 prefab
                UIBar = Instantiate(healthUIPrefab, canvas.transform).transform;
                // 拿到Image子物体, 第一个子物体序号是0
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
            Destroy(UIBar.gameObject);

        // 每次攻击的时候要把血条置为可见
        UIBar.gameObject.SetActive(true);
        timeLeft = visibleTime;

        // 设置血条百分比
        float sliderPercent = (float)currentHealth/maxHealth;
        healthSlider.fillAmount = sliderPercent;

        
    }

    // 血条跟随角色移动, 血条永远对着摄像机
    void LateUpdate()
    {
        if (UIBar != null)
        {
            UIBar.position = barPoint.position;
            // 血条永远对着摄像机
            UIBar.forward  = - cam.forward;

            if (timeLeft <= 0 && !alwaysVisible)
                UIBar.gameObject.SetActive(false);
            else
                timeLeft -= Time.deltaTime;
        }
    }
}
