using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] GameObject hpBar;

    public void SetupHP(float hp)
    {
        hpBar.GetComponent<Image>().fillAmount = hp;
    }

    public IEnumerator SetHP(float newHp, float maxHp)
    {
        //current fill of the hp bar
        float currentHp = hpBar.GetComponent<Image>().fillAmount;
        //amount
        float changeAmount = currentHp - newHp;

        while(currentHp - newHp > Mathf.Epsilon)
        {
            currentHp -= changeAmount * Time.deltaTime;
            hpBar.GetComponent<Image>().fillAmount = currentHp;
            yield return null;
        }
        hpBar.GetComponent<Image>().fillAmount = newHp;
    }
}