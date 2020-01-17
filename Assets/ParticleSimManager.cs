using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class ParticleSimManager : MonoBehaviour {
    public ComputeShader particleSimShader;
    public Material particleDebugMaterial;
    public Mesh particleMesh;

    public uint particleCount = 1000;

    private ComputeBuffer drawArgs;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private ComputeBuffer particleBufferA;
    private ComputeBuffer particleBufferB;
    private bool pingPong;
    
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
    }

    private void Start() {
        particleBufferA = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 12);
        particleBufferB = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 12);
        
                 verletIndex = particleSimShader.FindKernel("Verlet");
        densityPressureIndex = particleSimShader.FindKernel("DensityPressure");
                  forceIndex = particleSimShader.FindKernel("Force");
                  
        drawArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = particleMesh.GetIndexCount(0);
        args[2] = particleMesh.GetIndexStart(0);
        args[3] = particleMesh.GetBaseVertex(0);
                                                                      
        var initialParticles = new GPUParticle[particleCount];
        var rand = new Random(283927);
        for (int i = 0; i < particleCount; i++) {
            initialParticles[i] = new GPUParticle();
            initialParticles[i].position = rand.NextFloat3(new float3(-10f, -10f, -10f), new float3(10f, 10f, 10f));
            initialParticles[i].velocity = rand.NextFloat3(new float3(-10f, -10f, -10f), new float3(10f, 10f, 10f));
            //Debug.Log($"{initialParticles[i].position} {initialParticles[i].velocity}");
        }
        
        particleBufferA.SetData(initialParticles);
    }

    private void PingPongBuffers() {
        particleSimShader.SetBuffer(verletIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(verletIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);

        particleSimShader.SetBuffer(densityPressureIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(densityPressureIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);

        particleSimShader.SetBuffer(forceIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(forceIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);
        
        pingPong = !pingPong;
    }

    private void Update() {
        PingPongBuffers();
        //particleSimShader.Dispatch(verletIndex, Mathf.CeilToInt(particleCount / 128f), 1, 1);
    }

    private void OnDestroy() {
        particleBufferA.Release();
        particleBufferB.Release();
    }

    private void LateUpdate() {
        args[1] = particleCount;
        drawArgs.SetData(args);
        
        particleDebugMaterial.SetPass(0);
        particleDebugMaterial.SetBuffer("_ParticleBuffer", particleBufferA);
        
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleDebugMaterial, new Bounds(Vector3.zero, Vector3.one * 1000f), drawArgs);
    }
}
