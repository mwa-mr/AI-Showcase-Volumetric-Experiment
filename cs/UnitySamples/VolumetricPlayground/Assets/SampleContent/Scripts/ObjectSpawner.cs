using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ObjectToSpawn
    {
        public GameObject prefab;
        public Vector3 scale = Vector3.one;
        public int count;
        public Color color = Color.white;
    }
    [SerializeField] private List<ObjectToSpawn> ObjectsToSpawn;
    [SerializeField] private float spawnRadius = 10;
    [SerializeField] private float velocity = 10;

    void Start()
    {
        foreach (var objectToSpawn in ObjectsToSpawn)
        {
            for (int i = 0; i < objectToSpawn.count; i++)
            {
                var position = transform.position + Random.insideUnitSphere * spawnRadius;
                var obj = Instantiate(objectToSpawn.prefab, position, Quaternion.identity, transform);
                obj.transform.localScale = objectToSpawn.scale;
                if (obj.GetComponent<Renderer>() != null)
                {
                    obj.GetComponent<Renderer>().material.color = objectToSpawn.color;
                }
                if (obj.GetComponent<Rigidbody>() != null)
                {
                    obj.GetComponent<Rigidbody>().velocity = Random.insideUnitSphere * velocity;
                }
            }
        }
    }
}
