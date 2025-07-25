using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Map
{
    public class SpawnZone : CorneredZone
    {
        public Vector3[] GetPositions(int count, float size)
        {
            Vector3[] corners = GetWorldCorners();
            if (corners.Length != 4 || count <= 0)
                return new Vector3[0];

            // Получаем стороны прямоугольника
            Vector3 origin = corners[0]; // нижний левый
            Vector3 right = (corners[1] - corners[0]).normalized;
            Vector3 forward = (corners[3] - corners[0]).normalized;

            float width = Vector3.Distance(corners[0], corners[1]);
            float depth = Vector3.Distance(corners[0], corners[3]);

            int maxPerRow = Mathf.FloorToInt(width / size);
            int maxRows = Mathf.FloorToInt(depth / size);
            int maxCapacity = maxPerRow * maxRows;

            if (maxCapacity == 0)
                return new Vector3[0];

            int spawnCount = Mathf.Min(count, maxCapacity);

            List<Vector3> positions = new List<Vector3>(spawnCount);
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxPerRow; col++)
                {
                    if (positions.Count >= spawnCount)
                        break;

                    Vector3 offset = right * (col * size + size / 2f) + forward * (row * size + size / 2f);
                    positions.Add(origin + offset);
                }
            }

            return positions.ToArray();
        }
    }
}