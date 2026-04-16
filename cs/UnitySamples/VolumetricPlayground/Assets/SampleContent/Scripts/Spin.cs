using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float speed = 30.0f;
    // Start is called before the first frame update

    Rigidbody rb;
    void Start()
    {
        if (transform.GetComponent<Rigidbody>() != null)
        {
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = Vector3.one * speed;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (rb == null)
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime);
        }
    }
}
