using UnityEngine;

public class GravityWell : MonoBehaviour
{
    public float Power = 10;
    public float DistanceMultiplier = 1;

    public Transform Target;

    private void Start()
    {
        if (Target == null)
        {
            Target = transform;
        }
    }

    void Update()
    {
        foreach (var child in GetComponentsInChildren<Rigidbody>())
        {
            var direction = Target.position - child.transform.position;
            var distance = direction.magnitude;
            var force = direction.normalized;
            child.AddForce(force * Power * (distance - distance * DistanceMultiplier));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Bump();
        }
    }

    public void Bump()
    {
        foreach (var child in GetComponentsInChildren<Rigidbody>())
        {
            child.AddForce(Random.insideUnitSphere * Power, ForceMode.Impulse);
        }
    }
}
