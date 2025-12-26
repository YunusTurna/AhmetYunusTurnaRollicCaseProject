using UnityEngine;
using System.Collections.Generic;

public abstract class GridItemSO : ScriptableObject
{
    [Header("Common Settings")]
    public string id;
    
    [Header("Generator Settings")]
    public GameObject unitPrefab; 
    public GameObject prefab;

    public Vector3 visualOffset = Vector3.zero; 

    [Header("Shape Configuration")]
    [HideInInspector]
    public bool[] layoutGrid = new bool[25]; 
    
    [HideInInspector]
    public List<Vector2Int> baseOffsets = new List<Vector2Int>();
}