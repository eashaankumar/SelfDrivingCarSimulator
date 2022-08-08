using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public InfinitePlatformSpawner spawner;

    [SerializeField]
    Transform nextPlatform;

    bool done;

    public Transform NextPlatform
    {
        get { return nextPlatform; }
    }


    public InfinitePlatformSpawner SetSpawner
    {
        set { spawner = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        done = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player") && spawner && !done)
        {
            done = true;
            spawner.PlayerEnteredPlatformEnd(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        print(other.gameObject.tag);
        if (other.gameObject.tag.Equals("MainCamera") && spawner)
        {
            spawner.PlatformBehindCamera(this);
        }
    }
}
