using UnityEngine;

namespace _Main.Scripts.Map
{
    public class CorneredZone : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(1,1,1,0.5f);
        
        [field: SerializeField] public Transform PointA {get; private set;}
        [field: SerializeField] public Transform PointB {get; private set;}

        public Vector3[] GetWorldCorners()
        {
            if (PointA == null || PointB == null)
                return new Vector3[0];

            Vector3 localA = transform.InverseTransformPoint(PointA.position);
            Vector3 localB = transform.InverseTransformPoint(PointB.position);

            Vector3 corner1 = new Vector3(localA.x, 0f, localA.z);
            Vector3 corner2 = new Vector3(localB.x, 0f, localA.z);
            Vector3 corner3 = new Vector3(localB.x, 0f, localB.z);
            Vector3 corner4 = new Vector3(localA.x, 0f, localB.z);

            return new Vector3[]
            {
                transform.TransformPoint(corner1),
                transform.TransformPoint(corner2),
                transform.TransformPoint(corner3),
                transform.TransformPoint(corner4)
            };
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() =>
            DrawGizmo();

        public void DrawGizmo()
        {
            Vector3[] corners = GetWorldCorners();
            if (corners.Length != 4)
                return;

            Gizmos.color = new Color(_color.r, _color.g, _color.b, _color.a);
            UnityEditor.Handles.color = Gizmos.color;
            UnityEditor.Handles.DrawAAConvexPolygon(corners);

            Gizmos.color = _color;
            for (int i = 0; i < 4; i++)
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
#endif
    }
}