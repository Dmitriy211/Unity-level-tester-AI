using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlatformGeneratorAgent : Agent
{
    [SerializeField] private Transform _finish;
    
    [SerializeField] private Range _distanceRange;
    [SerializeField] private Range _angleRange;
    [SerializeField] private Range _heightRange;
    [FormerlySerializedAs("_intrinsicRewardCoefficient")] [SerializeField] private float _auxiliaryInput;

    private PlatformGenerator _platformGenerator;
    private BehaviorParameters _behaviorParameters;

    private void Awake()
    {
        Academy.Instance.AgentPreStep += BeforeStep;
    }

    private void BeforeStep(int stepCount)
    {
        AddReward(-1f / MaxStep);
    }

    public override void Initialize()
    {
        base.Initialize();

        _platformGenerator = GetComponent<PlatformGenerator>();
        _behaviorParameters = GetComponent<BehaviorParameters>();
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (Vector3.Distance(_platformGenerator.LastPosition, _finish.position) < 4) return;

        float distance = actionBuffers.ContinuousActions[0].Remap(-1, 1, _distanceRange.Min, _distanceRange.Max);
        float angle = actionBuffers.ContinuousActions[1].Remap(-1, 1, _angleRange.Min, _angleRange.Max);
        float height = actionBuffers.ContinuousActions[2].Remap(-1, 1, _heightRange.Min, _heightRange.Max);

        Vector3 lastPosition = _platformGenerator.LastPosition;
        
        Transform platform = _platformGenerator.SpawnPlatform(distance, angle, height);
        
        if (Vector3.Distance(_finish.position, platform.position) < Vector3.Distance(_finish.position, lastPosition))
            AddReward(2f);
    }
    
    public override void OnEpisodeBegin()
    {
        _platformGenerator.Reset();
        _auxiliaryInput = Random.Range(-1f, 1f);
        RequestDecision();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetDirectionToFinish());
        sensor.AddObservation(GetDistanceToFinish());
        sensor.AddObservation(_auxiliaryInput);
    }

    private Vector3 GetDirectionToFinish() => (_finish.position - _platformGenerator.LastPosition).normalized;
    private float GetDistanceToFinish() => Vector3.Distance(_finish.position, _platformGenerator.LastPosition);

    public void AddIntrinsicReward(float reward)
    {
        AddReward(reward * _auxiliaryInput);
    }
}
