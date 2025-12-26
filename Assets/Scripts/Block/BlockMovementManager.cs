using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events;

public class BlockMovementManager : MonoBehaviour
{
    public static BlockMovementManager Instance { get; private set; }
    [Header("Dependencies")]
    public Camera mainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float snapDuration = 0.2f;
    [SerializeField] private Ease snapEase = Ease.OutBack;
    [SerializeField] private float wallPadding = 0.15f;

    [Header("Gate Settings")]
    [SerializeField] private float gateAlignDuration = 0.15f;
    [SerializeField] private float gateExitDuration = 0.4f;
    [SerializeField] private float exitDistanceMultiplier = 3.0f;

    private GameInput inputActions;
    private BlockData selectedBlockData;
    private Transform selectedVisualTransform;

    private Vector3 dragOffset;
    private Vector3 initialVisualPos;
    private Vector2Int currentGridPos;

    private Vector2 currentPointerScreenPosition;

    private Tween activeTween;

    private GridManager Grid => LevelManager.Instance != null ? LevelManager.Instance.Grid : null;

    public UnityEvent OnBlockDestroyed;

    void Awake()
    {
        inputActions = new GameInput();

        inputActions.Board.Click.started += OnDragStart;
        inputActions.Board.Click.canceled += OnDragEnd;
        inputActions.Board.Position.performed += OnPointerMove;
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void OnDestroy()
    {
        inputActions.Board.Click.started -= OnDragStart;
        inputActions.Board.Click.canceled -= OnDragEnd;
        inputActions.Board.Position.performed -= OnPointerMove;
        inputActions?.Dispose();
    }

    private void OnDragStart(InputAction.CallbackContext context)
    {
        if (Grid == null) return;

        currentPointerScreenPosition = inputActions.Board.Position.ReadValue<Vector2>();
        
        Vector3 hitPoint = GetWorldPos(currentPointerScreenPosition);

        if (hitPoint.x == float.NegativeInfinity) return;

        Vector2Int gridPos = Grid.WorldToGrid(hitPoint);
        BlockData hitBlock = Grid.GetBlockAt(gridPos);

        if (hitBlock == null) return;
        
        if (hitBlock.moveDirection == MoveDirection.None) return;

        activeTween?.Kill();

        selectedBlockData = hitBlock;
        currentGridPos = hitBlock.position;

        selectedVisualTransform = Grid.GetBlockTransform(hitBlock);
        
        if (selectedVisualTransform == null) return;

        dragOffset = selectedVisualTransform.position - hitPoint;
        initialVisualPos = selectedVisualTransform.position;
    }

    private void OnPointerMove(InputAction.CallbackContext context)
    {
        currentPointerScreenPosition = context.ReadValue<Vector2>();

        if (selectedBlockData != null)
        {
            ProcessMovementLogic();
            CheckGatePassageWhileDragging();
        }
    }

    private void OnDragEnd(InputAction.CallbackContext context)
    {
        if (selectedBlockData == null || selectedVisualTransform == null) return;

        Grid.UpdateBlockPosition(selectedBlockData, currentGridPos);

        SnapBlock(selectedVisualTransform, currentGridPos);

        ResetSelection();
    }

    private void ProcessMovementLogic()
    {
        if (selectedBlockData == null || selectedVisualTransform == null) return;
        
        Vector3 hitPoint = GetWorldPos(currentPointerScreenPosition);
        if (hitPoint.x == float.NegativeInfinity) return;

        Vector3 targetRawPos = hitPoint + dragOffset;

        (bool allowX, bool allowZ) = selectedBlockData.moveDirection switch
        {
            MoveDirection.Horizontal => (true, false),
            MoveDirection.Vertical => (false, true),
            MoveDirection.All => (true, true),
            _ => (false, false)
        };

        if (!allowX) targetRawPos.x = initialVisualPos.x;
        if (!allowZ) targetRawPos.z = initialVisualPos.z;

        Vector2Int targetGridPos = Grid.WorldToGrid(targetRawPos);

        UpdateGridPosition(targetGridPos, allowX, allowZ);
        ClampVisualPosition(targetRawPos, allowX, allowZ);
    }

    private void CheckGatePassageWhileDragging()
    {
        if (selectedBlockData == null) return;

        List<Vector2Int> blockCells = GridUtils.CalculateOccupiedCells(currentGridPos, selectedBlockData.shapeData, selectedBlockData.rotation, selectedBlockData.mirror);

        var dirs = GetAllowedDirections();

        foreach (var dir in dirs)
        {
            ColorGateData gate = FindGateTouchingBlock(blockCells, dir);

            if (gate == null) continue;
            if (gate.itemColor != selectedBlockData.itemColor) continue;
            if (!CheckGateExitPath(blockCells, dir)) continue;

            Grid.UpdateBlockPosition(selectedBlockData, currentGridPos);
            AnimateAlignAndPassThroughGate(dir, gate);
            ResetSelection();
            return;
        }
    }

    private List<Vector2Int> GetAllowedDirections()
    {
        var dirs = new List<Vector2Int>();
        if (selectedBlockData.moveDirection != MoveDirection.Vertical)
        {
            dirs.Add(Vector2Int.left);
            dirs.Add(Vector2Int.right);
        }
        if (selectedBlockData.moveDirection != MoveDirection.Horizontal)
        {
            dirs.Add(Vector2Int.up);
            dirs.Add(Vector2Int.down);
        }
        return dirs;
    }

    private void UpdateGridPosition(Vector2Int targetGridPos, bool allowX, bool allowZ)
    {
        Vector2Int tempPos = currentGridPos;

        if (allowX) tempPos.x = MoveAlongAxis(tempPos, targetGridPos.x, true);
        if (allowZ) tempPos.y = MoveAlongAxis(tempPos, targetGridPos.y, false);

        currentGridPos = tempPos;
    }

    private int MoveAlongAxis(Vector2Int currentPos, int targetVal, bool isXAxis)
    {
        int startVal = isXAxis ? currentPos.x : currentPos.y;
        int diff = targetVal - startVal;

        if (diff == 0) return startVal;

        int step = (int)Mathf.Sign(diff);
        int count = Mathf.Abs(diff);
        int resultVal = startVal;

        Vector2Int testPos = currentPos;

        for (int i = 0; i < count; i++)
        {
            int nextVal = resultVal + step;
            if (isXAxis) testPos.x = nextVal; else testPos.y = nextVal;

            if (!Grid.IsMoveValid(selectedBlockData, testPos)) break;

            resultVal = nextVal;
        }
        return resultVal;
    }

    private void ClampVisualPosition(Vector3 rawPos, bool allowX, bool allowZ)
    {
        Vector3 cellCenter = Grid.GridToWorld(currentGridPos);

        rawPos.x = allowX ? ClampAxis(rawPos.x, cellCenter.x, Vector2Int.left, Vector2Int.right) : cellCenter.x;
        rawPos.z = allowZ ? ClampAxis(rawPos.z, cellCenter.z, Vector2Int.down, Vector2Int.up) : cellCenter.z;
        rawPos.y = 1f;

        selectedVisualTransform.position = rawPos;
    }

    private float ClampAxis(float currentVal, float centerVal, Vector2Int negDir, Vector2Int posDir)
    {
        float limitNeg = centerVal - GetMoveLimit(negDir);
        float limitPos = centerVal + GetMoveLimit(posDir);
        return Mathf.Clamp(currentVal, limitNeg, limitPos);
    }

    private float GetMoveLimit(Vector2Int dir)
    {
        return Grid.IsMoveValid(selectedBlockData, currentGridPos + dir)
            ? Grid.GetCellSize() * 0.5f
            : wallPadding;
    }

    private ColorGateData FindGateTouchingBlock(List<Vector2Int> blockCells, Vector2Int dir)
    {
        foreach (var c in blockCells)
        {
            var gate = Grid.GetGateAt(c + dir);
            if (gate != null) return gate;
        }
        return null;
    }

    private bool CheckGateExitPath(List<Vector2Int> blockCells, Vector2Int exitDir)
    {
        int steps = Mathf.RoundToInt(exitDistanceMultiplier);

        for (int s = 1; s <= steps; s++)
        {
            foreach (var cell in blockCells)
            {
                Vector2Int checkPos = cell + exitDir * s;

                if (Grid.GetGateAt(checkPos) != null) continue;

                if (Grid.IsObstacle(checkPos)) return false;

                BlockData other = Grid.GetBlockAt(checkPos);
                if (other != null && other != selectedBlockData) return false;
            }
        }
        return true;
    }

    private void AnimateAlignAndPassThroughGate(Vector2Int exitDir, ColorGateData gate)
    {
        Transform visual = selectedVisualTransform;
        Vector2Int blockGridPos = currentGridPos;

        Grid.UnregisterBlock(selectedBlockData);
        selectedBlockData = null;

        Vector3 alignedPos = Grid.GridToWorld(blockGridPos);
        alignedPos.y = 1f;

        Vector3 exitTarget = alignedPos + new Vector3(exitDir.x, 0, exitDir.y) * Grid.GetCellSize() * exitDistanceMultiplier;

        Sequence seq = DOTween.Sequence();
        
        seq.Append(visual.DOMove(alignedPos, gateAlignDuration).SetEase(Ease.OutQuad));
        seq.Append(visual.DOMove(exitTarget, gateExitDuration).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            ReturnBlockToPool(visual);
            OnBlockDestroyed?.Invoke();
        });
    }

    private void ReturnBlockToPool(Transform block)
    {
        if (block == null) return;
        ObjectPoolManager.Instance.ReturnToPool(block.gameObject);
    }



    void SnapBlock(Transform visual, Vector2Int pos)
    {
        activeTween?.Kill();
        Vector3 w = Grid.GridToWorld(pos);
        w.y = 1f;
        activeTween = visual.DOMove(w, snapDuration).SetEase(snapEase);
    }

    void ResetSelection()
    {
        selectedBlockData = null;
        selectedVisualTransform = null;
    }

    Vector3 GetWorldPos(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, Vector3.up * 1f);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return new Vector3(float.NegativeInfinity, 0, 0);
    }
}
