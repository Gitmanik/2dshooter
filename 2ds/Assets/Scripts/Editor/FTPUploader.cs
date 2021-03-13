using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using System.Net;

public class FTPUploader : EditorWindow
{
    private const string AssetLocation = "Assets/FTPSettings.asset";

    private static FTPUploaderData data;

    private static SerializedObject serializedObject;

    [MenuItem("Gitmanik/FTP Uploader")]
    private static void Init()
    {
        FTPUploader window = (FTPUploader)GetWindow(typeof(FTPUploader));
        window.Show();
    }

    [InitializeOnLoadMethod]
    private static void OnLoad()
    {
        data = AssetDatabase.LoadAssetAtPath<FTPUploaderData>(AssetLocation);
        if (!data)
        {
            data = CreateInstance<FTPUploaderData>();
            AssetDatabase.CreateAsset(data, AssetLocation);
            AssetDatabase.Refresh();
        }
        serializedObject = new SerializedObject(data);
    }

    private void OnGUI()
    {
        serializedObject.Update();
        string targetFilename = data.filename.Replace("{date}", DateTime.Today.ToString("dd-MM-yyyy"));

        EditorGUILayout.LabelField("Gitmanik FTP Uploader", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(new GUIContent($"Address: {data.address}", "Remote address ending with '/'"));
        EditorGUILayout.LabelField($"Username: {data.username}");
        EditorGUILayout.LabelField($"Password: {new string('*', data.password.Length)}");

        EditorGUILayout.LabelField("Filename: ");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("filename"), GUIContent.none);
        EditorGUILayout.LabelField(targetFilename);
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("Build and Upload"))
        {
            BuildPlayerOptions b = GetBuildPlayerOptions();

            string title  = $"Gitmanik FTP Uploader: {targetFilename}";
            EditorUtility.DisplayProgressBar(title, $"Building Project", 0.1f);
            BuildPipeline.BuildPlayer(b);

            EditorUtility.DisplayProgressBar(title, $"Compressing to Zip", 0.5f);

            string zipFile = Path.Combine(Application.dataPath, "../" + targetFilename);
            File.Delete(zipFile);
            ZipFile.CreateFromDirectory(Path.GetDirectoryName(b.locationPathName), zipFile);

            EditorUtility.DisplayProgressBar(title, $"Uploading to FTP", 0.9f);

            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(data.username, data.password);
                client.UploadFile("ftp://" + data.address + targetFilename, WebRequestMethods.Ftp.UploadFile, zipFile);
            }
            File.Delete(zipFile);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Gitmanik FTP Uploader", $"Finished uploading: {targetFilename}", "OK");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private BuildPlayerOptions GetBuildPlayerOptions(bool askForLocation = false,BuildPlayerOptions defaultOptions = new BuildPlayerOptions()){
        MethodInfo method = typeof(BuildPlayerWindow.DefaultBuildMethods).GetMethod(
            "GetBuildPlayerOptionsInternal",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (BuildPlayerOptions)method.Invoke(
            null,
            new object[] { askForLocation, defaultOptions });
    }
}

public class FTPUploaderData : ScriptableObject
{
    public string address;
    public string username, password;
    public string filename;
}