using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Units
{
    public class UnitPathPredictor : MonoBehaviour
    {
        [SerializeField] private Color _validColor = Color.green;
        [SerializeField] private Color _invalidColor = Color.red;
        [SerializeField] private float _segmentStep = 0.5f;
        [SerializeField] private LineRenderer _lineRenderer;

        public void DrawPath(Vector3[] corners, float maxDistance)
        {
            if (corners == null || corners.Length < 2)
            {
                _lineRenderer.positionCount = 0;
                return;
            }

            List<Vector3> resampledPoints = new List<Vector3>();
            float totalLength = 0f;
            float validLength = 0f;

            resampledPoints.Add(corners[0]);
            Vector3 previousPoint = corners[0];

            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 start = previousPoint;
                Vector3 end = corners[i];
                float segmentLength = Vector3.Distance(start, end);
                Vector3 direction = (end - start).normalized;

                float covered = 0f;
                while (covered + _segmentStep < segmentLength)
                {
                    covered += _segmentStep;
                    Vector3 point = start + direction * covered;
                    resampledPoints.Add(point);
                    totalLength += _segmentStep;

                    if (validLength + _segmentStep <= maxDistance)
                        validLength += _segmentStep;
                }

                float remaining = segmentLength - covered;
                if (remaining > 0.01f)
                {
                    resampledPoints.Add(end);
                    totalLength += remaining;
                    if (validLength + remaining <= maxDistance)
                        validLength += remaining;
                }

                previousPoint = end;
            }

            _lineRenderer.positionCount = resampledPoints.Count;
            _lineRenderer.SetPositions(resampledPoints.ToArray());

            float validPercent = Mathf.Clamp01(validLength / totalLength);
            ApplySplitGradient(validPercent);
        }

        private void ApplySplitGradient(float percent)
        {
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(_validColor, 0f),
                    new GradientColorKey(_validColor, percent),
                    new GradientColorKey(_invalidColor, percent),
                    new GradientColorKey(_invalidColor, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );

            _lineRenderer.colorGradient = gradient;
        }

        public void ClearPath()
        {
            _lineRenderer.positionCount = 0;
        }
    }
}