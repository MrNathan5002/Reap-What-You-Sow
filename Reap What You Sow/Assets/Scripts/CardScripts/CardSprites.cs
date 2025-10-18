using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    // Drag your sprite from the Project window into this slot in the Inspector
    public Sprite newSprite;

    // (Optional) reference to a SpriteRenderer, or it will find one automatically
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Get the SpriteRenderer component on this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Apply the sprite at runtime
        if (newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning("[SpriteChanger] No sprite assigned in Inspector!");
        }
    }

    // Optional: change sprite dynamically (for example, with a key press)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeSprite(newSprite);
        }
    }

    public void ChangeSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}