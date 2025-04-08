using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform target;

    // Temporaly materials to know in wich state is the unit
    public Material idleMaterial;
    public Material followMaterial;
    public Material attackMaterial;

    public bool isPlayer;

    //Stats
    public float unitDamage = 10f;

    // When the unit get inside the collider the states animator starts
    private void OnTriggerEnter(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
        }
    }

    // When the unit stay inside the collider the states animator stand
    private void OnTriggerStay(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
        }
    }

    // When the unit get outside the collider the states animator stops
   private void OnTriggerExit(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && target != null)
        {
            target = null;
        }
    }

    // Debuging materials for changings states
    public void SetIdleMaterial()
    {
        if (!CompareTag("Enemy"))
        {
            GetComponent<Renderer>().material = idleMaterial;
        }
    }

     public void SetFollowMaterial()
    {
        GetComponent<Renderer>().material = followMaterial;
    }

     public void SetAttackMaterial()
    {
        GetComponent<Renderer>().material = attackMaterial;
    }

    // Debug Gizmos
    private void OnDrawGizmos()
    {
        // Follow Distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f*0.2f); // first number is the collider radius and second one is the unit scale

        // Attack Distante
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Stop Attack Distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}
