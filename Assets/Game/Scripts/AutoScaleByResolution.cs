using UnityEngine;

public class AutoScaleByResolution : MonoBehaviour
{
    [Header("Настройки масштаба")] [Tooltip("Масштаб по умолчанию при разрешении 1920x1080")] [SerializeField]
    private Vector3 defaultScale = new Vector3(1f, 1f, 1f);

    private Vector3 initialScale;
    [SerializeField] private float initialScreenWidth = 1920f;
    [SerializeField] private float initialScreenHeight = 1080f;
    
    private void Start()
    {
        initialScale = transform.localScale;
        AdjustScale();
    }
    private void Update()
    {
        AdjustScale();
    }
    private int _lastScreenWidth = -1;
    private int _lastScreenHeight = -1;
    private void AdjustScale()
    {
        int currentScreenWidth = Screen.width;
        int currentScreenHeight = Screen.height;
        if (_lastScreenWidth == currentScreenWidth && _lastScreenHeight == currentScreenHeight)
            return;
        _lastScreenWidth = currentScreenWidth;
        _lastScreenHeight = currentScreenHeight;
        float scaleX = currentScreenWidth / initialScreenWidth;
        float scaleY = currentScreenHeight / initialScreenHeight;
        float scale = Mathf.Min(scaleX, scaleY);
        transform.localScale = new Vector3(
            defaultScale.x * scale,
            defaultScale.y * scale,
            defaultScale.z
        );
    }
}