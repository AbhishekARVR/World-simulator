using System;
using System.Collections.Generic;
using UnityEngine;

#region Structs

public struct Vertex
{
    public Vertex(float mass, Vector3 position, bool isFixed)
    {
        this.mass = mass;
        this.position = position;
        velocity = new Vector3(0, 0, 0);
        this.isFixed = isFixed;
        weight = 1.0f / mass;
        estimpatedPosition = position;
    }
    public float mass;
    public Vector3 position;
    public Vector3 estimpatedPosition;
    public Vector3 velocity;
    public float weight;
    public bool isFixed;
}

public struct StretchConstraint
{
    public StretchConstraint(int vertex1, int vertex2, float initialDistance, float stiffness)
    {
        this.vertex1Index = vertex1;
        this.vertex2Index = vertex2;
        this.initialDistance = initialDistance;
        this.stiffness = stiffness;
    }
    public int vertex1Index;
    public int vertex2Index;
    public float initialDistance;
    public float stiffness;
}

public struct BendConstraint
{
    public BendConstraint(int vertex1, int vertex2, int vertex3, int vertex4, float initialAngle, float stiffness)
    {
        this.vertex1Index = vertex1;
        this.vertex2Index = vertex2;
        this.vertex3Index = vertex3;
        this.vertex4Index = vertex4;
        this.initialAngle = initialAngle;
        this.stiffness = stiffness;
    }
    public int vertex1Index;
    public int vertex2Index;
    public int vertex3Index;
    public int vertex4Index;
    public float initialAngle;
    public float stiffness;
}

#endregion

public class ClothSimulationHandler : MonoBehaviour
{
    #region Variable Declarations
    public int edgeCount = 5;
    public float mass = 1.0f;
    public float gravity = -9.81f;
    public Vector3 wind = new Vector3(0, 0, 0);
    private Vector3 gravityVector;
    public Transform[] sphereTransforms;
    public float dampingFactor = 0.01f;
    public List<StretchConstraint> stretchConstraints;
    public List<BendConstraint> bendConstraints;
    public int iterations;
    public float stretchStiffness;
    public float bendStiffness;

    private Vertex[] vertices;

    #endregion

    #region Monobehaviour Methods

    private void Awake()
    {
        CreateVertices();
        CreateSphereTransforms();
        CreateStretchConstraints();
        CreateBendConstraints();
        gravityVector = new Vector3(0, gravity, 0);
        UpdateSpheres();
    }

    void Update()
    {
        ApplyGravityAndWind();
        ApplyDamping();
        UpdateEstimatedPositions();
        for (int iter = 0; iter < iterations; iter++)
        {
            SolveStretchConstraints();
            SolveBendConstraints();
        }
        UpdatePositions();
        UpdateSpheres();
    }

    #endregion


    private void CreateBendConstraints()
    {
        bendConstraints = new List<BendConstraint>();

        for (int i = 0; i < edgeCount - 1; i++)
        {
            for (int j = 0; j < edgeCount - 1; j++)
            {
                int v0 = i * edgeCount + j;
                int v1 = (i + 1) * edgeCount + j;
                int v2 = (i + 1) * edgeCount + (j + 1);
                int v3 = i * edgeCount + (j + 1);
                AddBendConstraint(v0, v2, v1, v3);


                if (i < edgeCount - 2)
                {
                    int e1 = (i + 1) * edgeCount + j;
                    int e2 = (i + 1) * edgeCount + (j + 1);
                    int o1 = i * edgeCount + j;
                    int o2 = (i + 2) * edgeCount + j;
                    AddBendConstraint(e1, e2, o1, o2);
                }

                if (j < edgeCount - 2)
                {
                    int e1 = i * edgeCount + (j + 1);
                    int e2 = (i + 1) * edgeCount + (j + 1);
                    int o1 = i * edgeCount + j;
                    int o2 = i * edgeCount + (j + 2);
                    AddBendConstraint(e1, e2, o1, o2);
                }

            }
        }
    }

    private void AddBendConstraint(int edgeV1, int edgeV2, int opp1, int opp2)
    {
        Vector3 p1 = vertices[edgeV1].position;
        Vector3 p2 = vertices[edgeV2].position;
        Vector3 p3 = vertices[opp1].position;
        Vector3 p4 = vertices[opp2].position;

        Vector3 n1 = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        Vector3 n2 = Vector3.Cross(p2 - p1, p4 - p1).normalized;

        float d = Mathf.Clamp(Vector3.Dot(n1, n2), -1.0f, 1.0f);
        float restAngle = Mathf.Acos(d);

        bendConstraints.Add(
            new BendConstraint(edgeV1, edgeV2, opp1, opp2, restAngle, bendStiffness)
        );
    }

    private void CreateStretchConstraints()
    {
        stretchConstraints = new List<StretchConstraint>();
        for (int i = 0; i < edgeCount; i++)
        {
            for (int j = 0; j < edgeCount; j++)
            {
                int currentIndex = i * edgeCount + j;
                if (j < edgeCount - 1)
                {
                    int rightIndex = i * edgeCount + (j + 1);
                    float initialDistance = Vector3.Distance(vertices[currentIndex].position, vertices[rightIndex].position);
                    stretchConstraints.Add(new StretchConstraint(currentIndex, rightIndex, initialDistance, stretchStiffness));
                }
                if (i < edgeCount - 1)
                {
                    int downIndex = (i + 1) * edgeCount + j;
                    float initialDistance = Vector3.Distance(vertices[currentIndex].position, vertices[downIndex].position);
                    stretchConstraints.Add(new StretchConstraint(currentIndex, downIndex, initialDistance, stretchStiffness));
                }
                if (i < edgeCount - 1 && j < edgeCount - 1)
                {
                    int downRightIndex = (i + 1) * edgeCount + (j + 1);
                    float initialDistance = Vector3.Distance(vertices[currentIndex].position, vertices[downRightIndex].position);
                    stretchConstraints.Add(new StretchConstraint(currentIndex, downRightIndex, initialDistance, stretchStiffness));
                }
            }
        }
    }

    private void CreateSphereTransforms()
    {
        sphereTransforms = new Transform[edgeCount * edgeCount];
        for (int i = 0; i < edgeCount * edgeCount; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            sphereTransforms[i] = sphere.transform;
            sphereTransforms[i].parent = this.transform;
        }
    }

    private void CreateVertices()
    {
        vertices = new Vertex[edgeCount * edgeCount];
        for (int i = 0; i < edgeCount; i++)
        {
            for (int j = 0; j < edgeCount; j++)
            {
                vertices[i * edgeCount + j] = new Vertex(mass, new Vector3(i, 0, j), (i == 0 && j == 0) || (i == 0 && j == edgeCount - 1));
            }
        }
    }

    private void SolveBendConstraints()
    {
        for (int i = 0; i < bendConstraints.Count; i++)
        {
            BendConstraint constraint = bendConstraints[i];

            Vector3 p1 = vertices[constraint.vertex1Index].estimpatedPosition;
            Vector3 p2 = vertices[constraint.vertex2Index].estimpatedPosition;
            Vector3 p3 = vertices[constraint.vertex3Index].estimpatedPosition;
            Vector3 p4 = vertices[constraint.vertex4Index].estimpatedPosition;

            Vector3 n1 = Vector3.Cross(p2 - p1, p3 - p1).normalized;
            Vector3 n2 = Vector3.Cross(p2 - p1, p4 - p1).normalized;

            float d = Mathf.Clamp(Vector3.Dot(n1, n2), -1.0f, 1.0f);
            float currentAngle = Mathf.Acos(d);
            float angleDiff = currentAngle - constraint.initialAngle;

            Vector3 q3 = (Vector3.Cross(p2 - p1, n2) + (Vector3.Cross(n1, p2 - p1) * d)) / Vector3.Cross(p2 - p1, p3 - p1).magnitude;
            Vector3 q4 = (Vector3.Cross(p2 - p1, n1) + (Vector3.Cross(n2, p2 - p1) * d)) / Vector3.Cross(p2 - p1, p4 - p1).magnitude;
            Vector3 q2 = -((Vector3.Cross(p3 - p1, n2) + (Vector3.Cross(n1, p3 - p1) * d)) / Vector3.Cross(p2 - p1, p3 - p1).magnitude)
                         - ((Vector3.Cross(p4 - p1, n1) + (Vector3.Cross(n2, p4 - p1) * d)) / Vector3.Cross(p2 - p1, p4 - p1).magnitude);
            Vector3 q1 = -(q2 + q3 + q4);

            float w1 = vertices[constraint.vertex1Index].weight;
            float w2 = vertices[constraint.vertex2Index].weight;
            float w3 = vertices[constraint.vertex3Index].weight;
            float w4 = vertices[constraint.vertex4Index].weight;

            float wightDenominator = w1 * q1.sqrMagnitude + w2 * q2.sqrMagnitude + w3 * q3.sqrMagnitude + w4 * q4.sqrMagnitude;
            if (wightDenominator < 1e-6f) continue;

            float s = constraint.stiffness * Mathf.Sqrt(1 - d * d) * angleDiff / wightDenominator;

            if (!vertices[constraint.vertex1Index].isFixed)
            {
                vertices[constraint.vertex1Index].estimpatedPosition -= w1 * s * q1;
            }
            if (!vertices[constraint.vertex2Index].isFixed)
            {
                vertices[constraint.vertex2Index].estimpatedPosition -= w2 * s * q2;
            }
            if (!vertices[constraint.vertex3Index].isFixed)
            {
                vertices[constraint.vertex3Index].estimpatedPosition -= w3 * s * q3;
            }
            if (!vertices[constraint.vertex4Index].isFixed)
            {
                vertices[constraint.vertex4Index].estimpatedPosition -= w4 * s * q4;
            }
        }
    }

    private void SolveStretchConstraints()
    {
        for (int i = 0; i < stretchConstraints.Count; i++)
        {
            StretchConstraint constraint = stretchConstraints[i];

            Vector3 p1 = vertices[constraint.vertex1Index].estimpatedPosition;
            Vector3 p2 = vertices[constraint.vertex2Index].estimpatedPosition;

            Vector3 delta = p1 - p2;
            float currentDistance = delta.magnitude;

            Vector3 n = (p1 - p2) / currentDistance;
            float diff = currentDistance - stretchConstraints[i].initialDistance;
            float wightDenominator = vertices[constraint.vertex1Index].weight + vertices[constraint.vertex2Index].weight;
            Vector3 deltaMultiplier = (stretchConstraints[i].stiffness * diff / wightDenominator) * n;

            if (!vertices[constraint.vertex1Index].isFixed)
            {
                vertices[constraint.vertex1Index].estimpatedPosition -= vertices[constraint.vertex1Index].weight * deltaMultiplier;
            }
            if (!vertices[constraint.vertex2Index].isFixed)
            {
                vertices[constraint.vertex2Index].estimpatedPosition += vertices[constraint.vertex2Index].weight * deltaMultiplier;
            }
        }
    }

    private void ApplyDamping()
    {
        Vector3 centerOfMassPositionNumerator = new Vector3(0, 0, 0);
        float centerOfMassDenominator = 0.0f;
        Vector3 centerOfMassVelocityNumerator = new Vector3(0, 0, 0);

        for (int i = 0; i < vertices.Length; i++)
        {
            centerOfMassPositionNumerator += vertices[i].position * vertices[i].mass;
            centerOfMassVelocityNumerator += vertices[i].velocity * vertices[i].mass;
            centerOfMassDenominator += vertices[i].mass;
        }

        Vector3 centerOfMassPosition = centerOfMassPositionNumerator / centerOfMassDenominator;
        Vector3 centerOfMassVelocity = centerOfMassVelocityNumerator / centerOfMassDenominator;

        Vector3 L = new Vector3(0, 0, 0);
        float[,] I = new float[3, 3];

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].isFixed) continue;

            Vector3 r = vertices[i].position - centerOfMassPosition;
            L += Vector3.Cross(r, vertices[i].mass * vertices[i].velocity);

            float[,] rSkew = SkewSymmetricMatrix(r);
            float[,] rSkewT = TransposeMatrix(rSkew);
            float[,] inertiaTensor = MultiplyMatrix(rSkew, rSkewT);
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    I[row, col] += vertices[i].mass * inertiaTensor[row, col];
                }
            }
        }

        Vector3 angularVelocity = new Vector3(0, 0, 0);
        try
        {
            float[,] IInv = InverseMatrix3x3(I);
            angularVelocity.x = IInv[0, 0] * L.x + IInv[0, 1] * L.y + IInv[0, 2] * L.z;
            angularVelocity.y = IInv[1, 0] * L.x + IInv[1, 1] * L.y + IInv[1, 2] * L.z;
            angularVelocity.z = IInv[2, 0] * L.x + IInv[2, 1] * L.y + IInv[2, 2] * L.z;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to compute inverse of inertia tensor: " + e.Message);
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (!vertices[i].isFixed)
            {
                Vector3 r = vertices[i].position - centerOfMassPosition;
                Vector3 vRot = Vector3.Cross(angularVelocity, r);
                Vector3 vLinear = centerOfMassVelocity;
                Vector3 vGoal = vLinear + vRot;

                vertices[i].velocity -= (vGoal - vertices[i].velocity) * dampingFactor;
            }
        }
    }

    private void ApplyGravityAndWind()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (!vertices[i].isFixed)
            {
                vertices[i].velocity += (gravityVector + wind) * Time.deltaTime;
            }
        }
    }

    private void UpdateEstimatedPositions()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (!vertices[i].isFixed)
            {
                vertices[i].estimpatedPosition = vertices[i].position + vertices[i].velocity * Time.deltaTime;
            }

        }
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (!vertices[i].isFixed)
            {
                vertices[i].velocity = (vertices[i].estimpatedPosition - vertices[i].position) / Time.deltaTime;
                vertices[i].position = vertices[i].estimpatedPosition;
            }
        }
    }

    private void UpdateSpheres()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            sphereTransforms[i].position = vertices[i].position;
        }
    }

    private float[,] SkewSymmetricMatrix(Vector3 r)
    {
        float[,] skew = new float[3, 3];
        skew[0, 0] = 0f;
        skew[0, 1] = -r.z;
        skew[0, 2] = r.y;

        skew[1, 0] = r.z;
        skew[1, 1] = 0f;
        skew[1, 2] = -r.x;

        skew[2, 0] = -r.y;
        skew[2, 1] = r.x;
        skew[2, 2] = 0f;

        return skew;
    }

    private float[,] TransposeMatrix(float[,] matrix)
    {
        float[,] transpose = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                transpose[i, j] = matrix[j, i];
            }
        }

        return transpose;
    }

    private float[,] MultiplyMatrix(float[,] A, float[,] B)
    {
        float[,] result = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = 0f;
                for (int k = 0; k < 3; k++)
                {
                    result[i, j] += A[i, k] * B[k, j];
                }
            }
        }

        return result;
    }

    private float[,] InverseMatrix3x3(float[,] m)
    {
        float det =
            m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
            m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
            m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

        if (Math.Abs(det) < 1e-8) throw new Exception("Matrix is singular and cannot be inverted.");

        float[,] inv = new float[3, 3];

        inv[0, 0] = (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) / det;
        inv[0, 1] = -(m[0, 1] * m[2, 2] - m[0, 2] * m[2, 1]) / det;
        inv[0, 2] = (m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1]) / det;

        inv[1, 0] = -(m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) / det;
        inv[1, 1] = (m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0]) / det;
        inv[1, 2] = -(m[0, 0] * m[1, 2] - m[0, 2] * m[1, 0]) / det;

        inv[2, 0] = (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]) / det;
        inv[2, 1] = -(m[0, 0] * m[2, 1] - m[0, 1] * m[2, 0]) / det;
        inv[2, 2] = (m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0]) / det;

        return inv;
    }

}