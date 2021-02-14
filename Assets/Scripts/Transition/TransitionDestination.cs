using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionDestination : MonoBehaviour
{
    // 传送点
    public enum DestinationTag
    {
        ENTER, A, B, C
    }

    public DestinationTag destinationTag;

    // 能不能传送
    private bool canTrans;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}
