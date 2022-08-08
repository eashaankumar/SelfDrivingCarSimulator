using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinitePlatformSpawner : MonoBehaviour
{
    [SerializeField]
    Platform platformPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerEnteredPlatformEnd(Platform p)
    {
        Platform nextP = Instantiate(platformPrefab);
        nextP.SetSpawner = this;
        nextP.transform.position = p.NextPlatform.position;
    }

    public void PlatformBehindCamera(Platform p)
    {
        Destroy(p.gameObject);
    }
}
