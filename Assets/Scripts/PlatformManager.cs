using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [SerializeField] private List<Transform> _platforms = new List<Transform>();

    public List<Transform> Platforms => _platforms;

    public float GetDistanceToClosestPlatform(Vector3 position, List<int> visited)
    {
        float minSqrDist = Mathf.Infinity;
        
        List<Transform> platforms = _platforms;
        if (visited.Count == 0) platforms = _platforms.Where(p => !visited.Contains(p.GetComponentInChildren<Platform>().ID)).ToList();
        
        foreach (var platform in platforms)
        {
            float platformSqrDist = (platform.position - position).sqrMagnitude;
            if (platformSqrDist < minSqrDist)
                minSqrDist = platformSqrDist;
        }
        return minSqrDist > 9999 ? -1 : Mathf.Sqrt(minSqrDist);
    }

    public Vector3 GetDirectionToClosestPlatform(Vector3 position, List<int> visited)
    {
        Transform closest = null;
        float minSqrDist = Mathf.Infinity;

        List<Transform> platforms = _platforms;
        if (visited.Count == 0) platforms = platforms.Where(p => !visited.Contains(p.GetComponentInChildren<Platform>().ID)).ToList();
        
        foreach (var platform in platforms)
        {
            float platformSqrDist = (platform.position - position).sqrMagnitude;
            if (platformSqrDist < minSqrDist)
            {
                minSqrDist = platformSqrDist;
                closest = platform;
            }
        }

        return closest ? (closest.position - position).normalized : Vector3.zero;
    }

    public void AddPlatform(Transform platformTransform)
    {
        _platforms.Add(platformTransform);
    }

    public void Reset()
    {
        foreach (var p in _platforms.Where(platform => platform.GetComponentInChildren<Platform>().ID != -1))
        {
            Destroy(p.gameObject);
        }
        
        _platforms = new List<Transform>();
    }
}
