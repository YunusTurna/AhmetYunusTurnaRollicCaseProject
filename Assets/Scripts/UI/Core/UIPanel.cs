using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private string panelId;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private CanvasGroup canvasGroup;
    private Tween currentTween;

    public string PanelId => panelId;
    public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show(bool instant = false)
    {
        currentTween?.Kill();
        gameObject.SetActive(true);

        if (instant)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;
            return;
        }

        if (useScaleAnimation)
        {
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            currentTween = DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one, fadeDuration).SetEase(showEase))
                .Join(canvasGroup.DOFade(1f, fadeDuration * 0.5f))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else
        {
            canvasGroup.alpha = 0f;
            currentTween = canvasGroup.DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
    }

    public void Hide(bool instant = false)
    {
        currentTween?.Kill();

        if (canvasGroup == null) return;
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (instant)
        {
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
            return;
        }

        if (useScaleAnimation)
        {
            currentTween = DOTween.Sequence()
                .Append(transform.DOScale(Vector3.zero, fadeDuration * 0.7f).SetEase(hideEase))
                .Join(canvasGroup.DOFade(0f, fadeDuration * 0.5f))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                });
        }
        else
        {
            currentTween = canvasGroup.DOFade(0f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(panelId))
            panelId = gameObject.name;
    }
#endif
}
