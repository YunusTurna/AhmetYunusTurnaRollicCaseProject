using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;

public class BlockDestroyerPowerUp : BasePowerUp
{
    [Header("Block Destroyer Settings")]
    [SerializeField] private float shrinkDuration = 0.3f;
    [SerializeField] private Ease shrinkEase = Ease.InBack;

    public UnityEvent OnBlockDestroyed;
    public UnityEvent OnNoBlocksToDestroy;

    public override PowerUpType Type => PowerUpType.BlockDestroyer;

    private GridManager Grid => LevelManager.Instance != null ? LevelManager.Instance.Grid : null;

    protected override void Execute()
    {
        if (Grid == null)
        {
            Debug.LogWarning("GridManager not found!");
            return;
        }

        BlockData randomBlock = Grid.GetRandomBlock();

        if (randomBlock == null)
        {
            OnNoBlocksToDestroy?.Invoke();
            Debug.Log("No blocks to destroy!");
            return;
        }

        DestroyBlock(randomBlock);
    }

    private void DestroyBlock(BlockData block)
    {
        Transform visual = Grid.GetBlockTransform(block);

        if (visual == null)
        {
            Grid.UnregisterBlock(block);
            OnBlockDestroyed?.Invoke();
            BlockMovementManager.Instance?.OnBlockDestroyed?.Invoke();
            return;
        }

        visual.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(shrinkEase)
            .OnComplete(() =>
            {
                Grid.UnregisterBlock(block);
                ReturnBlockToPool(visual);
                OnBlockDestroyed?.Invoke();
                BlockMovementManager.Instance?.OnBlockDestroyed?.Invoke();
                Debug.Log($"Random block destroyed! Remaining power-ups: {CurrentCount}");
            });
    }

    private void ReturnBlockToPool(Transform block)
    {
        if (block == null) return;
        ObjectPoolManager.Instance.ReturnToPool(block.gameObject);
    }
}
