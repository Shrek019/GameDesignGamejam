using UnityEngine;
using UnityEngine.AI;

public class Wall : MonoBehaviour
{
    void Start()
    {
        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = GetComponent<Collider>().bounds.size;
    }
}
