using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEvent : MonoBehaviour
{
    public ShadowMonsterController monsterController;

    public void HitMages()
    {
        monsterController.CheckStrikeHitbox();

    }
}