using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] Transform player = null;
    [SerializeField] float speed = 1;
    Vector3 lastPosition;
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = player.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = player.position;
        Vector3 delta = newPosition - lastPosition;
        transform.Translate(delta.x * speed, 0,0);
        lastPosition = newPosition;
    }
}
