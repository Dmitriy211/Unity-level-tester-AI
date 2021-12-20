using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    private PlayerController _playerController;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private List<int> _visitedPlatforms = new List<int>();

    public override void Initialize()
    {
        base.Initialize();
        
        _playerController = GetComponent<PlayerController>();
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        
        if (TryGetComponent(out BehaviorParameters behaviorParameters))
            _playerInput.enabled = behaviorParameters.BehaviorType switch
            {
                BehaviorType.HeuristicOnly => false,
                BehaviorType.InferenceOnly => false,
                _ => _playerInput.enabled
            };
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector2 moveVector = new Vector2(actionBuffers.DiscreteActions[0], actionBuffers.DiscreteActions[1]);
        Vector2 lookVector = new Vector2(actionBuffers.DiscreteActions[2], actionBuffers.DiscreteActions[3]);
        float jump = actionBuffers.DiscreteActions[4];
        
        _playerController.Move(moveVector);
        _playerController.Look(lookVector);
        if (jump > 0.5f) _playerController.Jump();
    }

    public override void OnEpisodeBegin()
    {
        _playerController.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // some observation
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out UniqueID id))
        {
            if (!_visitedPlatforms.Contains(id.ID))
            {
                
                _visitedPlatforms.Add(id.ID);
            }
        }
    }
}
