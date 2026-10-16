using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
   private void Awake()
   {
      transform.position = new Vector3(0f, 0.5f, 0f);
   }
}
