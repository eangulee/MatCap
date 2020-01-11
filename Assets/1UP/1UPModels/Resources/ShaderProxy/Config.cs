using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System;

[System.Serializable]
public static class Config
{
    public static string ABROOT_WWWPATH_;

    public static string ABROOT_IOPATH_;

    static Config()
    {
        //Debug.Log("test platform:" + Application.platform);

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                ABROOT_IOPATH_ = string.Concat(Application.streamingAssetsPath, "/", "AssetBundles/StandaloneWindows/");

                ABROOT_WWWPATH_ = string.Concat("file:///", ABROOT_IOPATH_);
                break;

            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:

                ABROOT_IOPATH_ = string.Concat(Application.streamingAssetsPath, "/", "AssetBundles/StandaloneOSXUniversal/");

                ABROOT_WWWPATH_ = string.Concat("file:///", ABROOT_IOPATH_);

                break;

            case RuntimePlatform.Android:
                ABROOT_WWWPATH_ = string.Concat(Application.streamingAssetsPath, "/", "AssetBundles/Android/");
                break;
            case RuntimePlatform.IPhonePlayer:
                ABROOT_WWWPATH_ = string.Concat(Application.streamingAssetsPath, "/", "AssetBundles/iOS/");
                break;
            default:
                break;
        }

        //Debug.Log("ABROOT_IOPATH_" + ABROOT_IOPATH_);

        //Debug.Log("ABROOT_WWWPATH_" + ABROOT_WWWPATH_);
    }
}

public static class MatHelper
{
    public static void ReplaceHiddenShader(Material targetMat, Shader shader, bool forceReplace = false)
    {
        if (shader == null || targetMat == null)
        {
            return;
        }

        if (
            (forceReplace || targetMat.shader.name == "Hidden/InternalErrorShader")
            ||
            (targetMat.shader.name == shader.name
            && targetMat.shader != shader)
            )
        {
            targetMat.shader = shader;
        }
    }
}


public static class ABHelper
{
    public static IEnumerator LoadBundleAsync(string abName, Action<AssetBundle> callback)
    {
        abName = Config.ABROOT_WWWPATH_ + abName;

        //Debug.Log("ReplaceShader:" + abName);

        var request = UnityWebRequestAssetBundle.GetAssetBundle(abName);

        yield return request.SendWebRequest();

        Debug.LogFormat("ab path:{0} error:{1}", abName, request.error);

        if (!request.isNetworkError && !request.isHttpError)
        {
            var bundle = DownloadHandlerAssetBundle.GetContent(request);

            if (bundle == null)
            {
                yield break;
            }

            if (callback != null)
            {
                callback(bundle);
            }

            //Debug.Log("刷新材质shader");
        }
    }

    public static AssetBundle LoadBundle(string abName)
    {
        if (string.IsNullOrEmpty(abName))
        {
            return null;
        }

        abName = Config.ABROOT_IOPATH_ + abName;

        string extension = Path.GetExtension(abName);

        string bundleName = abName.Substring(0, abName.Length - extension.Length);

        AssetBundle bundle = null;

        if (!File.Exists(abName))
        {
            return null;
        }

        // Load the bundle
        bundle = AssetBundle.LoadFromFile(abName);
        if (null == bundle)
        {
            return null;
        }

        // Load the bundle's assets
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string name in assetNames)
        {
            bundle.LoadAsset(name);
        }

        return bundle;
    }
}