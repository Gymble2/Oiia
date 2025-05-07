using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referência ao Transform do personagem
    public Vector3 offset; // Distância entre a câmera e o personagem

    void LateUpdate()
    {
        if (player != null)
        {
            // Move a câmera com base na posição do personagem, mantendo o offset
            transform.position = player.position + offset;
        }
    }
}
