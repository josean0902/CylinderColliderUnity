using UnityEngine;

[ExecuteAlways]
public class CylinderCollider : MonoBehaviour
{
    public enum CylinderDirection { XAxis, YAxis, ZAxis }

    [field: SerializeField] public bool IsTrigger { get; set; }
    [field: SerializeField] public bool ProvidesContacts { get; set; }
    [field: SerializeField] public PhysicsMaterial Material { get; set; }

    [field: SerializeField] public Vector3 Center { get; set; }
    [field: SerializeField, Min(0.01f)] public float Radius { get; set; } = 1f;
    [field: SerializeField, Min(0.01f)] public float Height { get; set; } = 1f;
    [field: SerializeField, Range(3, 32)] public int BoxColliderCount { get; set; } = 3;
    [field: SerializeField] public CylinderDirection Direction { get; set; } = CylinderDirection.YAxis;

    [SerializeField, HideInInspector]
    private bool autoRebuild;

    private GameObject colliderParent;

    // --------- Gizmos cache ---------

    private Vector3[] gizmoPoints;

    private void EnsureGizmoArray(int count)
    {
        if (gizmoPoints == null || gizmoPoints.Length != count)
            gizmoPoints = new Vector3[count];
    }

    // --------- Gizmos ---------

    private void OnDrawGizmosSelected()
    {
        if (BoxColliderCount < 3 || Radius <= 0f || Height <= 0f || autoRebuild)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;

        int pointCount = BoxColliderCount * 2;
        EnsureGizmoArray(pointCount);

        float angleStep = 360f / pointCount;
        float halfHeight = Height * 0.5f;

        int heightAxis = (int)Direction;
        int widthAxis = (heightAxis + 1) % 3;
        int lengthAxis = (heightAxis + 2) % 3;

        Vector3 verticalAxis = Vector3.zero;
        verticalAxis[heightAxis] = 1f;

        float radOffset = angleStep * 0.5f * Mathf.Deg2Rad;

        for (int i = 0; i < pointCount; i++)
        {
            float rad = angleStep * i * Mathf.Deg2Rad + radOffset;

            Vector3 p = Vector3.zero;
            p[heightAxis] = Center[heightAxis];
            p[widthAxis] = Radius * Mathf.Cos(rad) + Center[widthAxis];
            p[lengthAxis] = Radius * Mathf.Sin(rad) + Center[lengthAxis];

            gizmoPoints[i] = p;
        }

        for (int i = 0; i < pointCount; i++)
        {
            int next = (i + 1) % pointCount;
            DrawSegment(gizmoPoints[i], gizmoPoints[next], verticalAxis, halfHeight);
        }
    }

    private static void DrawSegment(Vector3 a, Vector3 b, Vector3 axis, float halfHeight)
    {
        Gizmos.DrawLine(a + axis * halfHeight, b + axis * halfHeight);
        Gizmos.DrawLine(a - axis * halfHeight, b - axis * halfHeight);
        Gizmos.DrawLine(a + axis * halfHeight, a - axis * halfHeight);
    }

    // --------- Collider generation ---------

    [ContextMenu("CleanColliders")]
    public void CleanCollider()
    {
        if (!colliderParent) 
            return;

        if (Application.isPlaying)
            Destroy(colliderParent);
        else
            DestroyImmediate(colliderParent);
    }

    [ContextMenu("BuildColliders")]
    public void BuildCollider()
    {
        if (BoxColliderCount < 3 || Radius <= 0f || Height <= 0f)
        {
            Debug.LogWarning("Invalid CylinderCollider parameters", this);
            return;
        }

        CleanCollider();

        EnsureColliderParent();

        float radStep = Mathf.PI / BoxColliderCount;
        float halfRad = radStep * 0.5f;

        Vector3 size = GetBoxSize(halfRad);
        Vector3 axis = Vector3.zero;
        axis[(int)Direction] = 1f;

        for (int i = 0; i < BoxColliderCount; i++)
        {
            float angleDeg = Mathf.Rad2Deg * radStep * i;
            CreateBoxCollider($"BoxCollider_{i + 1}", angleDeg, size, axis);
        }
    }

    private void EnsureColliderParent()
    {
        if (colliderParent == null)
        {
            colliderParent = transform.Find("CylinderCollider")?.gameObject;

            if (colliderParent == null)
            {
                colliderParent = new GameObject("CylinderCollider");
                colliderParent.transform.SetParent(transform, false);
            }
        }
    }

    private void CreateBoxCollider(string name, float angle, Vector3 size, Vector3 axis)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(colliderParent.transform, false);
        go.transform.localPosition = Center;
        go.transform.localRotation = Quaternion.AngleAxis(angle, axis);

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.size = size;
        box.isTrigger = IsTrigger;
        box.providesContacts = ProvidesContacts;
        box.material = Material;
    }

    private Vector3 GetBoxSize(float halfAngle)
    {
        Vector3 size = Vector3.zero;

        size[(int)Direction] = Height;
        size[((int)Direction + 1) % 3] = Radius * Mathf.Cos(halfAngle) * 2f;
        size[((int)Direction + 2) % 3] = Radius * Mathf.Sin(halfAngle) * 2f;

        return size;
    }
}