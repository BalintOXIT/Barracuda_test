using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class TaskAttack : Node
{
    private Transform _lastTarget;
    private EnemyManager _enemymanager;

    private float _attackTime = 1f;
    private float _attackCounter = 0;

    private Animator _animator;

    public TaskAttack(Transform t)
    {
        _animator = t.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if (target != _lastTarget)
        {
            _enemymanager = target.GetComponent<EnemyManager>();
            _lastTarget = target;
        }

        _attackCounter += Time.deltaTime;
        if (_attackCounter >= _attackTime)
        {
            bool enemyDead = _enemymanager.TakeHit();
            if (enemyDead)
            {
                ClearData("target");
                if (_animator != null)
                {
                    _animator.SetBool("Attacking", false);
                    _animator.SetBool("Walking", true);
                }
            }
            else
            { _attackCounter = 0f; }
        }
        state = NodeState.RUNNING;
        return state;
    }

}
