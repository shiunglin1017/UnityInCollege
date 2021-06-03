using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D[] colliders = transform.parent.GetComponentsInChildren<Collider2D>();
        for(int i = 0;i<colliders.Length; i++)
        {
            colliders[i].isTrigger = true;
        }
    }
}
