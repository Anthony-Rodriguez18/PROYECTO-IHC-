using UnityEngine;

public class BalaTrap : MonoBehaviour
{
    [Header("Vida de la bala")]
    public float lifeTime = 5f;

    private void Start()
    {
        // Por si no choca con nada, se destruye sola
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Si choca con una pared -> destruir bala
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // 2) Si choca con el jugador -> matar jugador + destruir bala
        if (other.CompareTag("Player"))
        {
            PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.KillPlayer();   // método que vamos a añadir
            }

            Destroy(gameObject);
        }

        // 3) Con cualquier otra cosa, la bala la puedes dejar pasar
        //    (no hacemos nada)
    }
}
