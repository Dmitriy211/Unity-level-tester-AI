using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAgent : Agent
{
    [SerializeField] private PlatformManager _platformManager;
    [SerializeField] private Transform _finish;

    public UnityEvent ReachedPlatform;
    public UnityEvent ReachedFinish;
    public UnityEvent Failed;

    private PlayerController _playerController;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private List<int> _visitedPlatforms = new List<int>();
    private BehaviorParameters _behaviorParameters;

    public override void Initialize()
    {
        base.Initialize();
        
        _playerController = GetComponent<PlayerController>();
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        _behaviorParameters = GetComponent<BehaviorParameters>();
        
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
        if (_behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly) return;
        
        Vector2 moveVector = new Vector2(actionBuffers.DiscreteActions[0] - 1,
            actionBuffers.DiscreteActions[1] - 1);
        Vector2 lookVector = new Vector2(actionBuffers.DiscreteActions[2]-1, 0);
        float jump = actionBuffers.DiscreteActions[3];

        if (moveVector.sqrMagnitude > Mathf.Epsilon) moveVector = moveVector.normalized;
        if (lookVector.sqrMagnitude > Mathf.Epsilon) lookVector = lookVector.normalized;
        
        _playerController.Move(moveVector);
        _playerController.Look(lookVector * 30);
        
        if (jump > 0.5f) _playerController.Jump();
        
        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    public override void OnEpisodeBegin()
    {
        _visitedPlatforms = new List<int>();
        _playerController.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetLookDirection());
        sensor.AddObservation(GetDirectionToClosestPlatform());
        sensor.AddObservation(GetDistanceToClosestPlatform());
        sensor.AddObservation(GetDirectionToFinish());
        sensor.AddObservation(GetDistanceToFinish());
    }

    private Vector3 GetLookDirection() => transform.forward;
    
    private Vector3 GetDirectionToClosestPlatform() => _platformManager.GetDirectionToClosestPlatform(transform.position, _visitedPlatforms);
    
    private float GetDistanceToClosestPlatform() => _platformManager.GetDistanceToClosestPlatform(transform.position, _visitedPlatforms);
    
    private Vector3 GetDirectionToFinish() => (transform.position - _finish.position).normalized;

    private float GetDistanceToFinish() => Vector3.Distance(transform.position, _finish.position);

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Platform platform))
        {
            ReachPlatform(platform);
        }
    }

    private void ReachPlatform(Platform platform)
    {
        if (platform.ID == -1)
        {
            AddReward(10f);
            print("Win!");
            // _playerController.Reset();
            EndEpisode();
            ReachedFinish?.Invoke();
        }
        
        if (!_visitedPlatforms.Contains(platform.ID))
        {
            _visitedPlatforms.Add(platform.ID);
            AddReward(1f + (1f / (Vector3.Distance(transform.position, _finish.position) + 1)));
            ReachedPlatform?.Invoke();
        }
    }

    public void Fail()
    {
        AddReward(-1f / MaxStep * (MaxStep - StepCount));
        print("Fail!");
        // _playerController.Reset();
        EndEpisode();
        Failed?.Invoke();
    }
}
