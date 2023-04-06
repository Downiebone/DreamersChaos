using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class landMine : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float forces = 10f;
    List<GameObject> collidedPlayers = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && collidedPlayers.Find(o => o == other.gameObject) == null)
        {
            collidedPlayers.Add(other.gameObject);
            StartCoroutine(removeCollid(other.gameObject));

            var n = -transform.up;
            var v = other.gameObject.transform.parent.GetComponent<Rigidbody>().velocity;

            float d = Vector3.Dot(v, n);
            if (d > 0f) v -= n * d;

            other.gameObject.transform.parent.GetComponent<Rigidbody>().velocity = v;

            other.gameObject.transform.parent.GetComponent<Rigidbody>().AddForce(transform.up * forces, ForceMode.Impulse);
        }
    }

    IEnumerator removeCollid(GameObject GO)
    {
        Debug.Log("thing");
        yield return new WaitForSeconds(1f);
        collidedPlayers.Remove(GO);
    }
}