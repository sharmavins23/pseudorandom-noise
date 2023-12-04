using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour {
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor {
        [ReadOnly] public NativeArray<float3x4> positions;
        [WriteOnly] public NativeArray<uint4> hashes;

        public SmallXXHash4 hash;
        public float3x4 domainTRS;

        float4x3 TransformPositions(float3x4 trs, float4x3 p) => float4x3(
            trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x,
            trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y,
            trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z
        );

        public void Execute(int i) {
            float4x3 p = TransformPositions(domainTRS, transpose(positions[i]));
            int4 u = (int4)floor(p.c0);
            int4 v = (int4)floor(p.c1);
            int4 w = (int4)floor(p.c2);

            hashes[i] = hash.Eat(u).Eat(v).Eat(w);
        }
    }

    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material material;
    [SerializeField, Range(4, 512)] int resolution = 16;
    [SerializeField]
    SpaceTRS domain = new() {
        scale = 8f
    };
    [SerializeField, Range(-0.5f, 0.5f)] float displacement = 0.1f;
    [SerializeField] int seed = 23;
    public enum Shape { Plane, Sphere, Torus, SpiralStar }
    static Shapes.ScheduleDelegate[] shapeJobs = {
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Torus>.ScheduleParallel,
        Shapes.Job<Shapes.SpiralStar>.ScheduleParallel
    };
    [SerializeField] Shape shape;
    [SerializeField] bool pulsing;

    NativeArray<uint4> hashes;
    NativeArray<float3x4> positions, normals;
    ComputeBuffer hashesBuffer, positionsBuffer, normalsBuffer;
    MaterialPropertyBlock propertyBlock;
    bool isDirty;
    Bounds bounds;

    static readonly int hashesID = Shader.PropertyToID("_Hashes"),
        positionsID = Shader.PropertyToID("_Positions"),
        normalsID = Shader.PropertyToID("_Normals"),
        configID = Shader.PropertyToID("_Config");

    void OnEnable() {
        isDirty = true; // Update something visualized!

        int length = resolution * resolution;
        length = length / 4 + (length & 1);
        hashes = new NativeArray<uint4>(length, Allocator.Persistent);
        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length * 4, sizeof(uint));
        positionsBuffer = new ComputeBuffer(length * 4, sizeof(float) * 3);
        normalsBuffer = new ComputeBuffer(length * 4, sizeof(float) * 3);

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesID, hashesBuffer);
        propertyBlock.SetBuffer(positionsID, positionsBuffer);
        propertyBlock.SetBuffer(normalsID, normalsBuffer);
        propertyBlock.SetVector(configID, new Vector4(
            resolution,
            1f / resolution,
            displacement
        ));
    }

    void OnDisable() {
        hashes.Dispose();
        positions.Dispose();
        hashesBuffer.Release();
        positionsBuffer.Release();
        normalsBuffer.Release();
        hashesBuffer = null;
        positionsBuffer = null;
        normalsBuffer = null;
    }

    void OnValidate() {
        if (hashesBuffer != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    void Update() {
        if (isDirty || transform.hasChanged) {
            isDirty = false;
            transform.hasChanged = false;
            // Re-calculate our bounds
            bounds = new Bounds(
                transform.position,
                float3(2f * cmax(abs(transform.lossyScale)) + displacement)
            );
            // Re-run our hash jobs to update the positions
            JobHandle handle = shapeJobs[(int)shape](
                positions,
                normals,
                resolution,
                transform.localToWorldMatrix,
                default
            );
            new HashJob {
                positions = positions,
                hashes = hashes,
                hash = SmallXXHash.Seed(seed),
                domainTRS = domain.Matrix
            }.ScheduleParallel(hashes.Length, resolution, handle).Complete();
            hashesBuffer.SetData(hashes.Reinterpret<uint>(4 * sizeof(uint)));
            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * sizeof(float)));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * sizeof(float)));
        }

        // Pulsing displacement with time!
        if (pulsing) {
            displacement = 0.5f * sin(Time.time);
            propertyBlock.SetVector(configID, new Vector4(
                resolution,
                1f / resolution,
                displacement
            ));
        }

        Graphics.DrawMeshInstancedProcedural(
            instanceMesh,
            0,
            material,
            bounds,
            resolution * resolution,
            propertyBlock
        );
    }
}
