using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_So templateData;

    public CharacterData_So characterData;
    public AttackData_So attackData;

    [HideInInspector]
    public bool isCritical;  // 是否暴击

    void Awake()
    {
        if (templateData != null)
            characterData = Instantiate(templateData);
    }

    #region Read from Data_So
    public int MaxHealth
    {
        get { if (characterData != null) return characterData.maxHealth; else return 0;}
        set { characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0;}
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0;}
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.currentDefence; else return 0;}
        set { characterData.currentDefence = value; }
    }
    #endregion


    #region Character Combat

    public void TakeDamage(CharacterStats attacker, CharacterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence, 0);
        Debug.Log("TakeDamage CurrentHealth = " + (CurrentHealth));
        Debug.Log("TakeDamage damage = " + (damage));
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        if (isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }

        //TODO: 更新血量UI
        //TODO: 更新经验
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击 ！" + coreDamage);
        }

        return (int)coreDamage;
    }

    #endregion
}
