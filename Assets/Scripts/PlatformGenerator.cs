using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private PlatformManager _platformManager;
    [SerializeField] private Transform _finish;

    public Vector3 LastPosition
    {
        get
        {
            int platformsNumber = _platformManager.Platforms.Count;
            return platformsNumber <= 1 ? transform.position : _platformManager.Platforms[platformsNumber - 1].position;
        }
    }
    
    private int _nextID = 1;
    
    public Transform SpawnPlatform(Vector3 position)
    {
        GameObject platformObject = Instantiate(_platformPrefab, position, Quaternion.identity, transform);
        Platform platform = platformObject.GetComponentInChildren<Platform>();
        platform.ID = _nextID;
        _nextID++;
        
        _platformManager.AddPlatform(platformObject.transform);

        return platformObject.transform;
    }

    public Transform SpawnPlatform(float distance, float angle, float height)
    {
        Vector3 position = LastPosition 
                           + Vector3.up * height 
                           + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward * distance;
        return SpawnPlatform(position);
    }

    public void Reset()
    {
        _platformManager.Reset();
        _platformManager.AddPlatform(_finish);
        _nextID = 1;
    }
}
