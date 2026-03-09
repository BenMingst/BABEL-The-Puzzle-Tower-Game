using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public string name;
    public Transform layerTransform;
    [Range(0f, 1f)] public float parallaxEffect;

    [HideInInspector] public float startPosX;
    [HideInInspector] public float startPosY;
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
                layer.startPosY = layer.layerTransform.position.y - cameraTransform.position.y;

                SpriteRenderer sr = layer.layerTransform.GetComponent<SpriteRenderer>();
                layer.textureUnitSizeX = sr.size.x;
            }
        }
    }

    private void LateUpdate()
    {
        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform == null) continue;

            float temp = (cameraTransform.position.x * (1 - layer.parallaxEffect));
            float dist = (cameraTransform.position.x * layer.parallaxEffect);

            float targetY = cameraTransform.position.y + layer.startPosY;

            layer.layerTransform.position = new Vector3(layer.startPosX + dist, targetY, layer.layerTransform.position.z);

            // snapping logic
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