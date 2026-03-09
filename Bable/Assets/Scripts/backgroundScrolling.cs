using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public string name;
    public Transform layerTransform;
    // 0 = Moves with camera (static)
    // 0.5 = Moves half as fast (middle)
    // 1 = Doesn't move at all (appears very close)
    [Range(0f, 1f)] public float parallaxEffect;

    [HideInInspector] public float startPosX;
    [HideInInspector] public float textureUnitSizeX;
}

public class backgroundScrolling : MonoBehaviour
{
    [SerializeField] private ParallaxLayer[] layers;
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                layer.startPosX = layer.layerTransform.position.x;

                SpriteRenderer sr = layer.layerTransform.GetComponent<SpriteRenderer>();
                // This is the actual width of your tiled sprite
                layer.textureUnitSizeX = sr.size.x;
            }
        }
    }

    private void LateUpdate()
    {
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform == null) continue;

            // 'temp' calculates how much of the distance has been "skipped"
            float temp = (cameraTransform.position.x * (1 - layer.parallaxEffect));

            // 'dist' calculates how far the object has travelled
            float dist = (cameraTransform.position.x * layer.parallaxEffect);

            layer.layerTransform.position = new Vector3(layer.startPosX + dist, layer.layerTransform.position.y, layer.layerTransform.position.z);

            // SNAPPING LOGIC
            if (temp > layer.startPosX + layer.textureUnitSizeX)
            {
                layer.startPosX += layer.textureUnitSizeX;
            }
            else if (temp < layer.startPosX - layer.textureUnitSizeX)
            {
                layer.startPosX -= layer.textureUnitSizeX;
            }
        }
    }
}