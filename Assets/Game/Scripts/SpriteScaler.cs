using UnityEngine;

public class SimpleScreenScaler : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    public bool scaleOnStart = true;

    void Start()
    {
        if (scaleOnStart)
        {
            ApplyScale();
        }
    }

    public void ApplyScale()
    {
  
        float scaleX = (float)Screen.width / referenceResolution.x;
        float scaleY = (float)Screen.height / referenceResolution.y;

     
        float finalScale = Mathf.Min(scaleX, scaleY);


        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}