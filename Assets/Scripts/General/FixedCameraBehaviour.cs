using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FixedCameraBehaviour : MonoBehaviour
{
    Transform player;
    CinemachineVirtualCamera activeCam;

    // Start is called before the first frame update
    void Start()
    {
        activeCam = transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
        player = GameObject.FindWithTag("Player").transform;

        if (player == null)
            Debug.Log("GameObject with Player tag not found");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            activeCam.Priority = 1;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(player.tag))
        {
            activeCam.Priority = 0;
        }
    }
}
