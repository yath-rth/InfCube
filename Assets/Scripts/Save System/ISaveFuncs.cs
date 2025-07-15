using UnityEngine;
using System.Collections.Generic;
using System.IO;

public interface ISaveFuncs
{
    string id { get; }
    void LoadData(object data);
    object SaveData(); 
}