using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Rendering;

using Unity.Mathematics;
using Collider = Unity.Physics.Collider;

/*
public class PlantManager : MonoBehaviour {
    public GameObject stalkSegmentPrefab;
    public int plantCount;
    
    private void Start() {
        var entityManager = World.Active.EntityManager;
        
        Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(stalkSegmentPrefab, World.Active);
        entityManager.AddComponent<PhysicsJoint>(sourceEntity);

        BlobAssetReference<JointData> fixedJointData = JointData.CreateFixed();
        BlobAssetReference<JointData> ballAndSocketData = JointData.CreateBallAndSocket(new float3(), new float3());
        
        
        entityManager.AddComponentData<JointData>(sourceEntity, new JointData { });

        BlobAssetReference<Collider> segmentCapsuleCollider =
            entityManager.GetComponentData<PhysicsCollider>(sourceEntity).Value;

        for (int i = 0; i < plantCount; i++) {
            var baseSegment = entityManager.Instantiate(sourceEntity);
            entityManager.SetComponentData(baseSegment, new Translation { Value = new float3() });
            entityManager.SetComponentData(baseSegment, new Rotation { Value = quaternion.identity });
            entityManager.SetComponentData(baseSegment, new PhysicsCollider { Value = segmentCapsuleCollider });
            
            for (int j = 0; j < i; j++) {
                var nextSegment = entityManager.Instantiate(sourceEntity);
                entityManager.SetComponentData(nextSegment, new Translation { Value = new float3() });
                entityManager.SetComponentData(nextSegment, new Rotation { Value = quaternion.identity });
                entityManager.SetComponentData(nextSegment, new PhysicsCollider { Value = segmentCapsuleCollider });
            }
        }
    }
}
*/
