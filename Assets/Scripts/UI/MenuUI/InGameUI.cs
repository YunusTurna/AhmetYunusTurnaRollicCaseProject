using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using System;

public class InGameUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text levelText;

    [Header("Power Up References")]
    [SerializeField] private TMP_Text timerPowerUpCount;
    [SerializeField] private TMP_Text blockDestroyerPowerUpCount;

    public static event Action OnGameTimerStarted;
    public static event Action OnGameTimerStopped;

    private float remainingTime;
    private CancellationTokenSource timerCts;
    private GameInput gameInput;

    private void Awake()
    {
        gameInput = new GameInput();
        gameInput.Board.Enable();
    }

    public void SetInGameUI()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.currentLevelData == null) return;

        remainingTime = LevelManager.Instance.currentLevelData.TimeLimit;
        levelText.SetText($"Level {LevelManager.Instance.currentLevelData.LevelIndex}");
        UpdateTimerDisplay();
        UpdatePowerUpCounts();
        gameInput.Board.Click.performed += OnFirstClick;
    }

    private void OnFirstClick(InputAction.CallbackContext context)
    {
        gameInput.Board.Click.performed -= OnFirstClick;
        StartTimer();
        OnGameTimerStarted?.Invoke();
    }

    public void StartTimer()
    {
        StopTimer();
        timerCts = new CancellationTokenSource();
        RunTimer(timerCts.Token).Forget();
    }

    public void StopTimer()
    {
        timerCts?.Cancel();
        timerCts?.Dispose();
        timerCts = null;
        OnGameTimerStopped?.Invoke();
    }

    private async UniTaskVoid RunTimer(CancellationToken token)
    {
        while (remainingTime > 0 && !token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            
            remainingTime -= Time.deltaTime;
            remainingTime = Mathf.Max(0, remainingTime);
            UpdateTimerDisplay();
        }

        if (!token.IsCancellationRequested && remainingTime <= 0)
        {
            LevelManager.Instance?.OnLevelFailed?.Invoke();
            Debug.Log("Level Failed");  
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void UpdatePowerUpCounts()
    {
        if (PowerUpManager.Instance == null) return;

        if (timerPowerUpCount != null)
        {
            int timerPowerUps = PowerUpManager.Instance.GetCount(PowerUpType.TimeExtension);
            timerPowerUpCount.SetText($"{timerPowerUps}");
        }

        if (blockDestroyerPowerUpCount != null)
        {
            int blockDestroyerPowerUps = PowerUpManager.Instance.GetCount(PowerUpType.BlockDestroyer);
            blockDestroyerPowerUpCount.SetText($"{blockDestroyerPowerUps}");
        }
    }
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
        UpdateTimerDisplay();
    }
    private void OnDestroy()
    {
        StopTimer();
    }
}
