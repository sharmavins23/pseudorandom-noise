using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour {
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor {
        [WriteOnly] public NativeArray<uint> hashes;

        public void Execute(int i) {
            hashes[i] = (uint)i;
        }
    }

    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material material;
    [SerializeField, Range(4, 512)] int resolution = 123;

    NativeArray<uint> hashes;
    ComputeBuffer hashesBuffer;
    MaterialPropertyBlock propertyBlock;

    static int hashesID = Shader.PropertyToID("_Hashes");
    static int configID = Shader.PropertyToID("_Config");

    void OnEnable() {
        int length = resolution * resolution;
        hashes = new NativeArray<uint>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length, sizeof(uint));

        // Compute all the hashes for grid items as a job
        new HashJob {
            hashes = hashes
        }.ScheduleParallel(hashes.Length, resolution, default).Complete();
        hashesBuffer.SetData(hashes);

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesID, hashesBuffer);
        propertyBlock.SetVector(configID, new Vector4(resolution, 1f / resolution));
    }

    void OnDisable() {
        hashes.Dispose();
        hashesBuffer.Release();
        hashesBuffer = null;
    }

    void OnValidate() {
        if (hashesBuffer != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    void Update() {
        Graphics.DrawMeshInstancedProcedural(
            instanceMesh,
            0,
            material,
            new Bounds(Vector3.zero, Vector3.one),
            hashes.Length,
            propertyBlock
        );
    }
}
