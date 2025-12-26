using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BlockEffectController : MonoBehaviour
{
    private BlockData blockData;
    private MoveDirection originalMoveDirection;
    
    private float remainingTime;
    private CancellationTokenSource bombCts;
    
    private int remainingBlocks;
    public bool IsIceLocked { get; private set; }
    
    public UnityEvent OnBombExploded;
    public UnityEvent OnIceUnlocked;

    private BlockVisualController visualController;

    public void Initialize(BlockData data)
    {
        blockData = data;
        originalMoveDirection = data.moveDirection;
        visualController = GetComponent<BlockVisualController>();
        
        switch (data.effect.type)
        {
            case BlockEffectType.Bomb:
                InitBomb(data.effect.bombTimer);
                break;
            case BlockEffectType.Ice:
                InitIce(data.effect.iceBlocksToDestroy);
                break;
        }
    }

    void InitBomb(float timer)
    {
        remainingTime = timer;
        IsIceLocked = false;
        UpdateBombDisplay();
    }

    void InitIce(int blocksToDestroy)
    {
        remainingBlocks = blocksToDestroy;
        IsIceLocked = true;
        
        blockData.moveDirection = MoveDirection.None;
        UpdateIceDisplay();
    }

    void OnEnable()
    {
        InGameUI.OnGameTimerStarted += OnGameStarted;
        InGameUI.OnGameTimerStopped += OnGameStopped;
        
        if (BlockMovementManager.Instance != null)
            BlockMovementManager.Instance.OnBlockDestroyed.AddListener(OnAnyBlockDestroyed);
    }

    void OnDisable()
    {
        InGameUI.OnGameTimerStarted -= OnGameStarted;
        InGameUI.OnGameTimerStopped -= OnGameStopped;
        
        if (BlockMovementManager.Instance != null)
            BlockMovementManager.Instance.OnBlockDestroyed.RemoveListener(OnAnyBlockDestroyed);
        
        StopBombTimer();
    }

    void OnGameStarted()
    {
        if (blockData?.effect.type == BlockEffectType.Bomb)
            StartBombTimer();
    }

    void OnGameStopped()
    {
        StopBombTimer();
    }

    void StartBombTimer()
    {
        StopBombTimer();
        bombCts = new CancellationTokenSource();
        RunBombTimer(bombCts.Token).Forget();
    }

    void StopBombTimer()
    {
        bombCts?.Cancel();
        bombCts?.Dispose();
        bombCts = null;
    }

    private async UniTaskVoid RunBombTimer(CancellationToken token)
    {
        while (remainingTime > 0 && !token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            
            remainingTime -= Time.deltaTime;
            remainingTime = Mathf.Max(0, remainingTime);
            UpdateBombDisplay();
        }

        if (!token.IsCancellationRequested && remainingTime <= 0)
        {
            Explode();
        }
    }

    void OnAnyBlockDestroyed()
    {
        if (!IsIceLocked || blockData?.effect.type != BlockEffectType.Ice) return;

        remainingBlocks--;
        UpdateIceDisplay();

        if (remainingBlocks <= 0)
        {
            UnlockIce();
        }
    }

    void UpdateBombDisplay()
    {
        if (visualController != null)
            visualController.SetEffectText(Mathf.CeilToInt(Mathf.Max(0, remainingTime)).ToString());
    }

    void UpdateIceDisplay()
    {
        if (visualController != null)
            visualController.SetEffectText(Mathf.Max(0, remainingBlocks).ToString());
    }

    public void Defuse()
    {
        StopBombTimer();
        if (visualController != null)
            visualController.HideEffectText();
    }

    void Explode()
    {
        StopBombTimer();
        OnBombExploded?.Invoke();
        LevelManager.Instance.OnLevelFailed?.Invoke();
    }

   

    void UnlockIce()
    {
        IsIceLocked = false;
        
        if (blockData != null)
            blockData.moveDirection = originalMoveDirection;
        
        if (visualController != null)
        {
            visualController.HideEffectText();
            visualController.UpdateDirectionVisuals();
        }
        
        OnIceUnlocked?.Invoke();
    }

    public void ResetController()
    {
        StopBombTimer();
        IsIceLocked = false;
        
        if (visualController != null)
            visualController.HideEffectText();
    }
}
