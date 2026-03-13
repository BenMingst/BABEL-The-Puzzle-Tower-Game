using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public string name;
    public Transform layerTransform;

    [Header("X Weight")]
    [Range(-1f, 2f)] public float parallaxWeightX;

    [Header("Y Weight")]
    [Range(-1f, 2f)] public float parallaxWeightY;

    [HideInInspector] public float initialYOffset;
}

public class ParallaxScrolling : MonoBehaviour
{
    [SerializeField] private ParallaxLayer[] layers;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                // record how far above cam ceiling objects start
                layer.initialYOffset = layer.layerTransform.position.y - cameraTransform.position.y;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                // calculate intended movement
                float moveX = deltaMovement.x * (1 - layer.parallaxWeightX);
                float moveY = deltaMovement.y * (1 - layer.parallaxWeightY);

                // apply movement
                layer.layerTransform.position += new Vector3(moveX, moveY, 0);

                // ceiling clamp; prevents player from seeing the ceiling layers when they climb
                float ceilingLine = cameraTransform.position.y + layer.initialYOffset;

                if (layer.layerTransform.position.y < ceilingLine)
                {
                    Vector3 clampedPos = layer.layerTransform.position;
                    clampedPos.y = ceilingLine;
                    layer.layerTransform.position = clampedPos;
                }
            }
        }

        lastCameraPosition = cameraTransform.position;
    }
}