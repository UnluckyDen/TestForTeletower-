using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Map
{
    public class MapGenerator : NetworkBehaviour
    {
        [SerializeField] private SerializedDictionary<Obstacle, int> _obstacleCountDictionary; 
        [SerializeField] private CorneredZone _spawnZone;
        [SerializeField] private List<CorneredZone> _exclusionZones = new();

        [SerializeField] private List<Obstacle> _spawnedObstacles = new();
        
        [ClientRpc]
        public void GenerateMapClientRpc(int seed)
        {
            ClearPrevious();
            Random.InitState(seed);

            int totalSpawned = 0;

            foreach (var entry in _obstacleCountDictionary)
            {
                int spawned = 0;
                int attempts = 0;
                int maxAttempts = entry.Value * 30;

                while (spawned < entry.Value && attempts < maxAttempts)
                {
                    attempts++;

                    Vector3[] spawnArea = _spawnZone.GetWorldCorners();
                    Vector3 randomPoint = GetRandomPointInZone(spawnArea);

                    float randomYRotation = Random.Range(0f, 360f);

                    Obstacle obstacle = Instantiate(entry.Key, randomPoint, Quaternion.Euler(0f, randomYRotation, 0f), _spawnZone.transform);

                    _spawnedObstacles.Add(obstacle);


                    if (IsOverlappingObstacle(obstacle) 
                        || IsOverlappingExclusionZones(obstacle)
                        || !IsInsideSpawnArea(obstacle))
                    {
                        _spawnedObstacles.Remove(obstacle);
                        DestroyImmediate(obstacle.gameObject);
                        continue;
                    }

                    spawned++;
                    totalSpawned++;
                }
            }
        }

        private Vector3 GetRandomPointInZone(Vector3[] corners)
        {
            Vector3 localPoint = new Vector3(
                Random.Range(0f, 1f),
                0f,
                Random.Range(0f, 1f)
            );

            Vector3 bottom = Vector3.Lerp(corners[0], corners[1], localPoint.x);
            Vector3 top = Vector3.Lerp(corners[3], corners[2], localPoint.x);
            Vector3 point = Vector3.Lerp(bottom, top, localPoint.z);

            return point;
        }

        private bool IsInsideSpawnArea(Obstacle obstacle)
        {
            Vector3[] obstacleCorners = obstacle.CorneredZone.GetWorldCorners();
            Vector3[] spawnZoneCorners = _spawnZone.GetWorldCorners();

            foreach (var corner in obstacleCorners)
            {
                if (!PointInsidePolygon(corner, spawnZoneCorners))
                    return false;
            }

            return true;
        }

        private bool IsOverlappingExclusionZones(Obstacle obstacle)
        {
            Vector3[] obstacleCorners = obstacle.CorneredZone.GetWorldCorners();

            foreach (var zone in _exclusionZones)
            {
                Vector3[] zoneCorners = zone.GetWorldCorners();
                if (ArePolygonsOverlapping(obstacleCorners, zoneCorners))
                    return true;
            }

            return false;
        }

        private bool IsOverlappingObstacle(Obstacle newObstacle)
        {
            Vector3[] newCorners = newObstacle.CorneredZone.GetWorldCorners();

            foreach (Transform child in _spawnZone.transform)
            {
                if (child == newObstacle.transform)
                    continue;

                Obstacle existingObstacle = child.GetComponent<Obstacle>();
                if (existingObstacle == null)
                    continue;

                Vector3[] existingCorners = existingObstacle.CorneredZone.GetWorldCorners();
                if (ArePolygonsOverlapping(newCorners, existingCorners))
                    return true;
            }

            return false;
        }

        private bool ArePolygonsOverlapping(Vector3[] polyA, Vector3[] polyB) =>
            !HasSeparatingAxis(polyA, polyB) && !HasSeparatingAxis(polyB, polyA);

        private bool HasSeparatingAxis(Vector3[] polyA, Vector3[] polyB)
        {
            int countA = polyA.Length;

            for (int i = 0; i < countA; i++)
            {
                Vector3 p1 = polyA[i];
                Vector3 p2 = polyA[(i + 1) % countA];
                Vector2 edge = new Vector2(p2.x - p1.x, p2.z - p1.z);
                Vector2 axis = new Vector2(-edge.y, edge.x).normalized;

                float minA, maxA, minB, maxB;
                ProjectPolygon(polyA, axis, out minA, out maxA);
                ProjectPolygon(polyB, axis, out minB, out maxB);

                if (maxA < minB || maxB < minA)
                    return true;
            }

            return false;
        }

        private void ProjectPolygon(Vector3[] poly, Vector2 axis, out float min, out float max)
        {
            float first = Vector2.Dot(new Vector2(poly[0].x, poly[0].z), axis);
            min = max = first;

            for (int i = 1; i < poly.Length; i++)
            {
                float projection = Vector2.Dot(new Vector2(poly[i].x, poly[i].z), axis);
                if (projection < min)
                    min = projection;
                if (projection > max)
                    max = projection;
            }
        }

        private bool PointInsidePolygon(Vector3 point, Vector3[] polygon)
        {
            Vector2 point2D = new Vector2(point.x, point.z);
            bool inside = false;

            int count = polygon.Length;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Vector2 pi = new Vector2(polygon[i].x, polygon[i].z);
                Vector2 pj = new Vector2(polygon[j].x, polygon[j].z);

                if ((pi.y > point2D.y) != (pj.y > point2D.y) &&
                    (point2D.x < (pj.x - pi.x) * (point2D.y - pi.y) / (pj.y - pi.y) + pi.x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private void ClearPrevious()
        {
            if (_spawnedObstacles == null || _spawnedObstacles.Count == 0)
                return;
            
            for (int i = _spawnedObstacles.Count - 1; i >= 0; i--)
                DestroyImmediate(_spawnedObstacles[i].gameObject);
            
            _spawnedObstacles.Clear();
        }
    }
}
