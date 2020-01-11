using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveScene : UnityEditor.AssetModificationProcessor
{
    static public void OnWillSaveAssets(string[] names)
    {
        if (ShaderProxy.current == null || ShaderProxy.current.targetShader == null)
        {
            return; 
        }

        var shader = Shader.Find(ShaderProxy.current.targetShader.name);

        ShaderProxy.current.UpdateShaders(shader , true);
    }
}

[UnityEditor.InitializeOnLoad]
public class ExcuteInEditorLoad {

    static ExcuteInEditorLoad()
    {
        EditorApplication.update += OnUpdate;
    }

    private static void OnUpdate()
    {
        if (ShaderProxy.current == null)
        {
            return;
        }

        ShaderProxy.current.LoadShader();

        ShaderProxy.current.UpdateShaders();
    }
}
