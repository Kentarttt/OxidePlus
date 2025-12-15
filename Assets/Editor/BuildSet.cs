using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class BuildSet
{
    [MenuItem("Tools/Set Build Time")]
    private static void SetBuildTime()
    {
        string path = Application.dataPath + "/Resources/build.txt";
        StreamWriter writeFile = new StreamWriter(path, false);
        writeFile.WriteLine(DateTime.Now);
        writeFile.Close();
    }
}