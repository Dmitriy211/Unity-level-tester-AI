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
    [SerializeField] private PlatformManager _platformManager;
    
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
                BehaviorType.HeuristicOnly => true,
                BehaviorType.InferenceOnly => false,
                BehaviorType.Default => false,
                _ => _playerInput.enabled
            };
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector2 moveVector = new Vector2(actionBuffers.DiscreteActions[0] - 1,
            actionBuffers.DiscreteActions[1] - 1).normalized;
        Vector2 lookVector = new Vector2(actionBuffers.DiscreteActions[2]-1, 0).normalized;
        float jump = actionBuffers.DiscreteActions[3];
        
        _playerController.Move(moveVector);
        _playerController.Look(lookVector * 30);
        if (jump > 0.5f) _playerController.Jump();
        
        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("reset");
        _visitedPlatforms = new List<int>();
        _playerController.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(_platformManager.GetDirectionToClosestPlatform(transform.position, _visitedPlatforms));
        sensor.AddObservation(_platformManager.GetDistanceToClosestPlatform(transform.position, _visitedPlatforms));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Platform platform))
        {
            if (!_visitedPlatforms.Contains(platform.ID))
            {
                _visitedPlatforms.Add(platform.ID);
                AddReward(5f);
            }
        }
    }
}
