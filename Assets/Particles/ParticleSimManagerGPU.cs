using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class ParticleSimManagerGPU : MonoBehaviour {
    public ComputeShader particleSimShader;
    public Material particleDebugMaterial;
    public Mesh particleMesh;

    public uint particleCount = 1000;

    public float boundsSize;
    public float  particleMass;
    public float  wallDamping;
    public float  particleStiffness;
    public float  particleRestingDensity;
    public float  particleViscosity;
    [Range(0f, 0.5f)] public float smoothingLength;
    [Range(0f, 0.5f)] public float interactionLength; 
    public float  timeStep;
    public float4 gravity;

    private ComputeBuffer drawArgs;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    //private NativeList<GPUParticle> particleList;

    private ComputeBuffer particleBufferA;
    //private bool pingPong;

    private ComputeBuffer neighborInfoBuffer;
    private ComputeBuffer neighborListBuffer;
    
    private int          verletIndex;
    private int densityPressureIndex;
    private int           forceIndex;
    
    struct GPUParticle {
        public float3 position;
        public float  density;
        public float3 velocity;
        public float  pressure;
        public float3 force;
        //public float  temperature;
        public uint   type; // 0 = earth, 1 = water, 2 = fire, 3 = wood, 4 = metal
        public float  temperature;
        public float  remainingLifetime;
        public float  pad0;
        public float  pad1;
    }

    struct NeighborInfo {
        public int startIndex;
        public int count;
    }

    public static ParticleSimManagerGPU instance;

    private void Awake() {
        instance = this;
        
        particleBufferA = new ComputeBuffer(64 * 64 * 64, sizeof(float) * 16);
        neighborInfoBuffer = new ComputeBuffer(64 * 64* 64, sizeof(int) * 2);
        neighborListBuffer = new ComputeBuffer(64 * 64 * 64 * 32, sizeof(int));
        
                 verletIndex = particleSimShader.FindKernel("Verlet");
        densityPressureIndex = particleSimShader.FindKernel("DensityPressure");
                  forceIndex = particleSimShader.FindKernel("Force");
                  
        drawArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = particleMesh.GetIndexCount(0);
        args[2] = particleMesh.GetIndexStart(0);
        args[3] = particleMesh.GetBaseVertex(0);
        
        InitializeParticles();
    }

    private void OnGUI() {
        if (GUI.Button(new Rect(10f, 10f, 200f, 20f), "Reinit")) 
            InitializeParticles();
    }

    private void InitializeParticles() {
        var initialParticles = new GPUParticle[particleCount];
        
        var rand = new Random((uint)Mathf.RoundToInt(Time.time * 100000f + 1f));
        for (int i = 0; i < particleCount; i++) {
            initialParticles[i] = new GPUParticle();
            initialParticles[i].position = rand.NextFloat3(new float3(-1f, -1f, -1f), new float3(1f, 1f, 1f));
            //initialParticles[i].velocity = rand.NextFloat3(new float3(-1f, -1f, -1f), new float3(1f, 1f, 1f));
            initialParticles[i].density = particleRestingDensity;
            initialParticles[i].pressure = 1f;
            //Debug.Log($"{initialParticles[i].position} {initialParticles[i].velocity}");
        }
        
        particleBufferA.SetData(initialParticles);
        //particleBufferB.SetData(initialParticles);
    }
    
    [BurstCompile]
    private struct PopulateSpatialHashMapJob : IJobParallelFor {
        [ReadOnly] public float cellSize;
        [ReadOnly] public float boundsSize;
        
        [ReadOnly] public NativeArray<GPUParticle> particles;
        //[WriteOnly] public NativeMultiHashMap<int, int> particleSpatialHashMap;
        [WriteOnly] public NativeMultiHashMap<int, int>.ParallelWriter particleSpatialHashMapWriter;
        
        public void Execute(int i) {
            //var parallelHashMap = particleSpatialHashMap.AsParallelWriter();
            int hash = (int)math.hash(new int3(math.floor((particles[i].position /* - boundsSize*/) / cellSize)));
            particleSpatialHashMapWriter.Add(hash, i);
        }
    }

    [BurstCompile]
    private struct PopulateNeighborListArrays : IJobParallelFor {
        [ReadOnly] public float cellSize;
        [ReadOnly] public NativeArray<GPUParticle> particles;
        [ReadOnly] public NativeMultiHashMap<int, int> particleSpatialHashMap;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> neighborList;
        [WriteOnly] public NativeArray<NeighborInfo> neighborInfo;

        public void Execute(int i) {
            GPUParticle thisParticle = particles[i];
            int3 thisBucket = new int3(math.floor((particles[i].position /* - boundsSize*/) / cellSize));

            int neighbors = 0;
            
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    for (int z = -1; z <= 1; z++) {
                        int hash = (int) math.hash(thisBucket + new int3(x, y, z));
                        if (particleSpatialHashMap.ContainsKey(hash)) {
                            var enumerator = particleSpatialHashMap.GetValuesForKey(hash);
                            while (enumerator.MoveNext()) {
                                if (neighbors < 32) {
                                    neighborList[i * 32 + neighbors] = enumerator.Current;
                                    neighbors++;
                                }
                            }
                        }
                    }
                }
            }

            neighborInfo[i] = new NeighborInfo { startIndex = i * 32, count = neighbors};
        }
    }

    private void Update() {
        // copy particles back from GPU -- slow, but not enough time to implement every part of non-brute-force SPH on GPU
        /*
        particleList = new NativeList<GPUParticle>((int)particleCount, Allocator.TempJob);
        var particleManaged = new GPUParticle[particleCount];
        particleBufferA.GetData(particleManaged, 0, 0, (int)particleCount);
        for (int i = 0; i < particleCount; i++) {
            particleList.Add(particleManaged[i]);
        }
        particleManaged = null;
        */
        
        // loop over particles and make pending deletions, etc
        // TODO
        
        // construct spatial acceleration structure
        /*
        var particleSpatialHashMap = new NativeMultiHashMap<int, int>(particleList.Length, Allocator.TempJob);
        var neighborInfoNativeArray = new NativeArray<NeighborInfo>(particleList.Length, Allocator.TempJob);
        var neighborListNativeArray = new NativeArray<int>(particleList.Length * 32, Allocator.TempJob);
        
        var populateSpatialHashMapJob = new PopulateSpatialHashMapJob();
        populateSpatialHashMapJob.particleSpatialHashMapWriter = particleSpatialHashMap.AsParallelWriter();
        populateSpatialHashMapJob.boundsSize = boundsSize;
        populateSpatialHashMapJob.cellSize = smoothingLength;
        populateSpatialHashMapJob.particles = particleList;

        var updateSpatialHashMapHandle = populateSpatialHashMapJob.Schedule(particleList.Length, 64);
        
        var populateNeighborListArraysJob = new PopulateNeighborListArrays();
        populateNeighborListArraysJob.particles = particleList;
        populateNeighborListArraysJob.cellSize = smoothingLength;
        populateNeighborListArraysJob.neighborInfo = neighborInfoNativeArray;
        populateNeighborListArraysJob.neighborList = neighborListNativeArray;
        populateNeighborListArraysJob.particleSpatialHashMap = particleSpatialHashMap;

        var populateNeighborListArraysHandle = populateNeighborListArraysJob.Schedule(particleList.Length, 64, updateSpatialHashMapHandle);
        
        populateNeighborListArraysHandle.Complete();
        updateSpatialHashMapHandle.Complete();
        particleSpatialHashMap.Dispose();

        neighborInfoBuffer.SetData(neighborInfoNativeArray);
        neighborListBuffer.SetData(neighborListNativeArray);
        
        neighborInfoNativeArray.Dispose();
        neighborListNativeArray.Dispose();
        particleList.Dispose();
        */

        particleSimShader.SetBuffer(verletIndex, "neighborInfo", neighborInfoBuffer);
        particleSimShader.SetBuffer(densityPressureIndex, "neighborInfo", neighborInfoBuffer);
        particleSimShader.SetBuffer(forceIndex, "neighborInfo", neighborInfoBuffer);
        
        particleSimShader.SetBuffer(verletIndex, "neighborLists", neighborListBuffer);
        particleSimShader.SetBuffer(densityPressureIndex, "neighborLists", neighborListBuffer);
        particleSimShader.SetBuffer(forceIndex, "neighborLists", neighborListBuffer);
        
        //Debug.Log($"{particleCount}");
        particleSimShader.SetInt("particleCount", (int)particleCount);
        
        //PingPongBuffers();
        particleSimShader.SetBuffer(verletIndex, "particles", particleBufferA);
        particleSimShader.SetBuffer(densityPressureIndex, "particles", particleBufferA);
        particleSimShader.SetBuffer(forceIndex, "particles", particleBufferA);
        
        particleSimShader.SetFloat("pi", Mathf.PI);
        particleSimShader.SetFloat("particleMass", particleMass);
        particleSimShader.SetFloat("wallDamping", wallDamping);
        particleSimShader.SetFloat("particleStiffness", particleStiffness);
        particleSimShader.SetFloat("particleRestingDensity", particleRestingDensity);
        particleSimShader.SetFloat("particleViscosity", particleViscosity);
        particleSimShader.SetFloat("smoothingLength", smoothingLength);
        particleSimShader.SetFloat("interactionLength", interactionLength);
        particleSimShader.SetFloat("timeStep", timeStep);
        particleSimShader.SetFloat("boundsSize", boundsSize);
        particleSimShader.SetVector("gravity", gravity);
        
        particleSimShader.Dispatch(densityPressureIndex, Mathf.CeilToInt(particleCount / 128f), 1, 1);
        particleSimShader.Dispatch(forceIndex,           Mathf.CeilToInt(particleCount / 128f), 1, 1);
        particleSimShader.Dispatch(verletIndex,          Mathf.CeilToInt(particleCount / 128f), 1, 1);
    }

    private void OnDestroy() {
        particleBufferA.Release();
    }

    private void LateUpdate() {
        args[1] = particleCount;
        drawArgs.SetData(args);
        
        particleDebugMaterial.SetPass(0);
        //particleDebugMaterial.SetBuffer("_ParticleBuffer", pingPong ? particleBufferA : particleBufferB);
        particleDebugMaterial.SetBuffer("_ParticleBuffer", particleBufferA);
        
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleDebugMaterial, new Bounds(Vector3.zero, Vector3.one * 1000f), drawArgs);

        //var particlesManaged = new GPUParticle[particleCount];
        //particleBufferA.GetData(particlesManaged);
        //particleList = ;
    }

    public void SpawnParticle(float3 position, float3 velocity, HandController.Element element) {
        GPUParticle newParticle = new GPUParticle {position = position, velocity = velocity, type = (uint)element};
        particleBufferA.SetData(new[] { newParticle }, 0, (int)particleCount - 1, 1);
        particleCount++;
    }
}
