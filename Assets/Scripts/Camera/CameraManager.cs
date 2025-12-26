using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Camera Settings")]
    [SerializeField] private float cameraAngle = 55f;

    [Tooltip("Multiplier for visible area (1.0 = exact grid size, 1.5 = 50% extra space around grid)")]
    [Range(1f, 3f)]
    [SerializeField] private float viewSizeMultiplier = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

 
    public void SetUpCamera()
    {
        LevelData currentLevelData = LevelManager.Instance?.currentLevelData;
        if (currentLevelData == null) return;
        FocusOnGrid(currentLevelData.Width, currentLevelData.Height, currentLevelData.CellSize);
    }

    public void FocusOnGrid(int width, int height, float cellSize)
    {
        float gridWidth = width * cellSize;
        float gridHeight = height * cellSize;
        float viewWidth = gridWidth * viewSizeMultiplier;
        float viewHeight = gridHeight * viewSizeMultiplier;
        float aspect = mainCamera.aspect;
        float fov = mainCamera.fieldOfView;
        float fovRad = fov * Mathf.Deg2Rad;

        float distanceForHeight = (viewHeight / 2f) / Mathf.Tan(fovRad / 2f);
        float distanceForWidth = (viewWidth / 2f) / (Mathf.Tan(fovRad / 2f) * aspect);

        float distance = Mathf.Max(distanceForHeight, distanceForWidth);

        float angleRad = cameraAngle * Mathf.Deg2Rad;
        float cameraHeight = distance * Mathf.Sin(angleRad);
        float cameraBack = distance * Mathf.Cos(angleRad);
        mainCamera.transform.position = new Vector3(0, cameraHeight, -cameraBack);
        mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    public void SetViewSizeMultiplier(float multiplier)
    {
        viewSizeMultiplier = Mathf.Clamp(multiplier, 1f, 3f);
    }

    public float GetViewSizeMultiplier() => viewSizeMultiplier;
}
