using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform target;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && target != null)
        {
            target = null;
        }
    }
}
