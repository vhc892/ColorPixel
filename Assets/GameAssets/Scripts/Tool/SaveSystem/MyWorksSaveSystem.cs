using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyWorksSaveSystem : BaseSaveSystem<MyWorkList>
{
    protected override string FileName => "my_work.json";
}

[System.Serializable]
public class MyWorkList
{
    public List<int> list = new List<int>();
}
