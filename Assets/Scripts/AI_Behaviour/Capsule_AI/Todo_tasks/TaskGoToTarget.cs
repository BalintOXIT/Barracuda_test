using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class TaskGoToTarget : Node
{
    private Transform _transform;
    private float _currDetectTime = 0f;

    public TaskGoToTarget(Transform t)
    {
        _currDetectTime = 0f;
        _transform = t;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
     
        if (Vector3.Distance(_transform.position, target.position) > 0.01f)
        {
            if (_currDetectTime < CapsuleBT.detectTime)
            {
                _currDetectTime += Time.deltaTime;
            }
            else
            {
                _transform.position = Vector3.MoveTowards(_transform.position, target.position, CapsuleBT.speed * Time.deltaTime);
                _transform.LookAt(target.position);
            }
        }
        state = NodeState.RUNNING;
        return state;
    }

}
