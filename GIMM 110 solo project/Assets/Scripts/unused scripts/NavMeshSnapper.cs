
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class NavMeshSnapper : MonoBehaviour
{
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
    }
}


