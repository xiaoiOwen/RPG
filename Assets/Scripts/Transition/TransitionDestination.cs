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


}
