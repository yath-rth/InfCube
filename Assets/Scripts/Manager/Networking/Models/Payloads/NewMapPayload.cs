using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NewMapPayload
{
    public Queue<PathInfo> extension;

    public NewMapPayload(Queue<PathInfo> path)
    {
        extension = path;
    }
}