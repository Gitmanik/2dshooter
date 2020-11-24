#if UNITY_EDITOR
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

class AutoUploadBuild
{
    [PostProcessBuild(9999)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        pathToBuiltProject = pathToBuiltProject.Remove(pathToBuiltProject.Length - pathToBuiltProject.LastIndexOf("\\"));
        Debug.Log(pathToBuiltProject);
        ZipFile.CreateFromDirectory(pathToBuiltProject, pathToBuiltProject + "/../build.zip");
    }
}
#endif