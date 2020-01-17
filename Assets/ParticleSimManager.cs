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

    public float  particleMass;
    public float  particleRadius;
    public float  particleStiffness;
    public float  particleRestingDensity;
    public float  particleViscosity;
    public float  smoothingLength;
    public float  timeStep;
    public float4 gravity;

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
        var rand = new Random((uint)gameObject.GetInstanceID());
        for (int i = 0; i < particleCount; i++) {
            initialParticles[i] = new GPUParticle();
            initialParticles[i].position = rand.NextFloat3(new float3(-1f, -1f, -1f), new float3(1f, 1f, 1f));
            initialParticles[i].velocity = rand.NextFloat3(new float3(-1f, -1f, -1f), new float3(1f, 1f, 1f));
            initialParticles[i].density = particleRestingDensity;
            initialParticles[i].pressure = 1f;
            //Debug.Log($"{initialParticles[i].position} {initialParticles[i].velocity}");
        }
        
        particleBufferA.SetData(initialParticles);
        //particleBufferB.SetData(initialParticles);
    }

    private void PingPongBuffers() {
        pingPong = !pingPong;
        
        particleSimShader.SetBuffer(verletIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(verletIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);

        particleSimShader.SetBuffer(densityPressureIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(densityPressureIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);

        particleSimShader.SetBuffer(forceIndex, "lastParticles", pingPong ? particleBufferA : particleBufferB);
        particleSimShader.SetBuffer(forceIndex, "nextParticles", pingPong ? particleBufferB : particleBufferA);
    }

    private void Update() {
        //Debug.Log($"{particleCount}");
        particleSimShader.SetInt("particleCount", (int)particleCount);
        
        //PingPongBuffers();
        particleSimShader.SetBuffer(verletIndex, "particles", particleBufferA);
        particleSimShader.SetBuffer(densityPressureIndex, "particles", particleBufferA);
        particleSimShader.SetBuffer(forceIndex, "particles", particleBufferA);
        
        particleSimShader.SetFloat("pi", Mathf.PI);
        particleSimShader.SetFloat("particleMass", particleMass);
        particleSimShader.SetFloat("particleRadius", particleRadius);
        particleSimShader.SetFloat("particleStiffness", particleStiffness);
        particleSimShader.SetFloat("particleRestingDensity", particleRestingDensity);
        particleSimShader.SetFloat("particleViscosity", particleViscosity);
        particleSimShader.SetFloat("smoothingLength", smoothingLength);
        particleSimShader.SetFloat("timeStep", timeStep);
        particleSimShader.SetVector("gravity", gravity);
        
        particleSimShader.Dispatch(densityPressureIndex, Mathf.CeilToInt(particleCount / 128f), 1, 1);
        particleSimShader.Dispatch(forceIndex,           Mathf.CeilToInt(particleCount / 128f), 1, 1);
        particleSimShader.Dispatch(verletIndex,          Mathf.CeilToInt(particleCount / 128f), 1, 1);
    }

    private void OnDestroy() {
        particleBufferA.Release();
        particleBufferB.Release();
    }

    private void LateUpdate() {
        args[1] = particleCount;
        drawArgs.SetData(args);
        
        particleDebugMaterial.SetPass(0);
        //particleDebugMaterial.SetBuffer("_ParticleBuffer", pingPong ? particleBufferA : particleBufferB);
        particleDebugMaterial.SetBuffer("_ParticleBuffer", particleBufferA);
        
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleDebugMaterial, new Bounds(Vector3.zero, Vector3.one * 1000f), drawArgs);
    }
}
