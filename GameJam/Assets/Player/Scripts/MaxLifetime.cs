using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxLifetime : MonoBehaviour
{
    [SerializeField] public float TimeLeft;

    // Update is called once per frame
    void FixedUpdate()
    {
        TimeLeft -= Time.fixedDeltaTime;
        if(TimeLeft <= 0)
            Destroy(gameObject);
    }
}
