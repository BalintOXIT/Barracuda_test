using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private int _healthpoints;

    private void Awake()
    {
        _healthpoints = 30;
    }

    public bool TakeHit()
    {
        _healthpoints -= 10;
        bool isdead = _healthpoints <= 0;
        if (isdead) Die();
        return isdead;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
