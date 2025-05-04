using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    public float Health, MaxHealth;
    public float Width = 237.5f;
    public float Height = 40f;

    [SerializeField]
    private RectTransform healthBar;

    public void SetMaxHealth(int maxHealth) {
        MaxHealth = maxHealth;
    }

    public void SetHealth(int health) {
        Health = health;
        float newWidth = (Health / MaxHealth) * Width;
        Debug.Log(Health+" "+MaxHealth+" "+Width);

        healthBar.sizeDelta = new Vector2(newWidth, Height);
    }
}
