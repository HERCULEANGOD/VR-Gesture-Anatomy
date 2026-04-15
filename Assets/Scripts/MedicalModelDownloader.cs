using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class MedicalModelDownloader : MonoBehaviour
{
    public string[] modelUrls = {
        "http://lifesciencedb.jp/bp3d/download/heart.obj",
        "http://lifesciencedb.jp/bp3d/download/brain.obj",
        "http://lifesciencedb.jp/bp3d/download/lung.obj",
        "http://lifesciencedb.jp/bp3d/download/liver.obj"
    };

    public string savePath = "Assets/Models";
    public bool downloadOnStart = false;

    void Start()
    {
        if (downloadOnStart)
            StartCoroutine(DownloadModels());
    }

    IEnumerator DownloadModels()
    {
        foreach (string url in modelUrls)
        {
            yield return StartCoroutine(DownloadModel(url));
        }
    }

    IEnumerator DownloadModel(string url)
    {
        using (var client = new HttpClient())
        {
            Task<byte[]> downloadTask = client.GetByteArrayAsync(url);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.IsFaulted || downloadTask.IsCanceled)
            {
                Debug.LogError("Download failed: " + downloadTask.Exception?.GetBaseException().Message);
            }
            else
            {
                byte[] data = downloadTask.Result;
                string fileName = Path.GetFileName(url);
                string fullDirectory = GetFullSavePath();
                Directory.CreateDirectory(fullDirectory);
                string fullPath = Path.Combine(fullDirectory, fileName);
                File.WriteAllBytes(fullPath, data);
                Debug.Log("Downloaded: " + fileName);
            }
        }
    }

    string GetFullSavePath()
    {
        if (Path.IsPathRooted(savePath))
            return savePath;

        if (savePath.StartsWith("Assets"))
            return Path.Combine(Application.dataPath, savePath.Substring("Assets".Length).TrimStart('/', '\\'));

        return Path.Combine(Application.dataPath, savePath);
    }
}
