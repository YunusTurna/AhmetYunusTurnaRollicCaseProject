using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform root;
    [Header("Prefabs")]
    public GameObject groundPlanePrefab;
    public GameObject obstaclePrefab;

    [Header("Block Spawn Settings")]
    [SerializeField] private float blockSpawnHeight = 0f;
    [SerializeField] private float gateSpawnHeight = 0f;

    private readonly List<GameObject> spawnedPooledObjects = new();
    private GameObject currentGroundPlane;

    public void GenerateLevel(LevelData levelData, GridManager gridManager)
    {
        ClearLevel();

        if (root == null)
            root = transform;

        gridManager.Initialize(levelData.Width, levelData.Height, levelData.CellSize, new List<Vector2Int>(levelData.Obstacles));

        SpawnGroundPlane(levelData, gridManager);

        for (int x = 0; x < levelData.Width; x++)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (levelData.VoidCells.Contains(pos)) continue;

                if (levelData.Obstacles.Contains(pos) && obstaclePrefab != null)
                {
                    Vector3 obstaclePos = gridManager.GridToWorld(pos);
                    obstaclePos.y = blockSpawnHeight;
                    
                    GameObject obstacle = ObjectPoolManager.Instance.GetFromPool(
                        obstaclePrefab,
                        obstaclePos,
                        Quaternion.identity,
                        root
                    );
                    spawnedPooledObjects.Add(obstacle);
                }
            }
        }

        foreach (var gateData in levelData.ColorGates)
        {
            if (gateData.gateData == null) continue;

            Quaternion rotation = GridUtils.GetRotationQuaternion(gateData.rotation);
            Vector3 rotatedOffset = rotation * gateData.gateData.visualOffset;
            Vector3 worldPos = gridManager.GridToWorld(gateData.position) + rotatedOffset;
            worldPos.y = gateSpawnHeight;

            GameObject gate = ObjectPoolManager.Instance.GetFromPool(
                gateData.gateData.prefab,
                worldPos,
                rotation,
                root
            );
            
            gate.transform.localScale = GridUtils.GetMirrorScale(gateData.mirror);
            
            ApplyColor(gate, gateData.itemColor);
            spawnedPooledObjects.Add(gate);

            gridManager.RegisterGate(gateData);
        }

        foreach (var sourceBlock in levelData.Blocks)
        {
            if (sourceBlock.shapeData == null) continue;

            BlockData runtimeBlock = new BlockData
            {
                position = sourceBlock.position,
                itemColor = sourceBlock.itemColor,
                rotation = sourceBlock.rotation,
                mirror = sourceBlock.mirror,
                shapeData = sourceBlock.shapeData,
                moveDirection = sourceBlock.moveDirection,
                effect = new BlockEffectData
                {
                    type = sourceBlock.effect.type,
                    bombTimer = sourceBlock.effect.bombTimer,
                    iceBlocksToDestroy = sourceBlock.effect.iceBlocksToDestroy
                }
            };

            Quaternion rotation = GridUtils.GetRotationQuaternion(runtimeBlock.rotation);
            Vector3 rotatedOffset = rotation * runtimeBlock.shapeData.visualOffset;
            Vector3 worldPos = gridManager.GridToWorld(runtimeBlock.position) + rotatedOffset;
            worldPos.y = blockSpawnHeight;

            GameObject block = ObjectPoolManager.Instance.GetFromPool(
                runtimeBlock.shapeData.prefab,
                worldPos,
                rotation,
                root
            );
            
            block.transform.localScale = GridUtils.GetMirrorScale(runtimeBlock.mirror);
            
            ApplyColor(block, runtimeBlock.itemColor);
            spawnedPooledObjects.Add(block);

            var visualController = block.GetComponent<BlockVisualController>();
            if (visualController != null)
                visualController.Initialize(runtimeBlock);

            if (runtimeBlock.effect.type != BlockEffectType.None)
            {
                var effectController = block.GetComponent<BlockEffectController>();
                if (effectController != null)
                    effectController.Initialize(runtimeBlock);
            }

            gridManager.RegisterBlock(runtimeBlock, block.transform);
        }
    }

    void SpawnGroundPlane(LevelData levelData, GridManager gridManager)
    {
        if (groundPlanePrefab == null) return;

        float planeWidth = levelData.Width * levelData.CellSize;
        float planeHeight = levelData.Height * levelData.CellSize;

        if (currentGroundPlane == null)
        {
            currentGroundPlane = Instantiate(groundPlanePrefab, root);
        }
        
        currentGroundPlane.SetActive(true);
        currentGroundPlane.transform.localPosition = Vector3.zero;
        currentGroundPlane.transform.localScale = new Vector3(planeWidth / 10f, 1f, planeHeight / 10f);
    }

    public void ClearLevel()
    {
        foreach (var obj in spawnedPooledObjects)
        {
            if (obj != null)
                ObjectPoolManager.Instance.ReturnToPool(obj);
        }
        spawnedPooledObjects.Clear();

        if (currentGroundPlane != null)
        {
            currentGroundPlane.SetActive(false);
        }
    }

    void ApplyColor(GameObject obj, GridItemColor colorEnum)
    {
        Color targetColor = GridUtils.GetUnityColor(colorEnum);
        foreach (var rend in obj.GetComponentsInChildren<Renderer>()) rend.material.color = targetColor;
    }
}
