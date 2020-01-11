using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderProxy : MonoBehaviour {
    [System.NonSerialized]
    public Shader targetShader;

    private static bool abLoading = false;

    [Tooltip("需要绑定的所有材质")]  
    public Material[] registerMats;

    [Tooltip("需要绑定的所有renderer")]
    public Renderer[] registerRenderers;

    /// <summary>
    /// 打包的shader路径
    /// </summary>
    public string SHADER_PATH = "TestShader";

    public static ShaderProxy current;
     
    public void LoadShader()
    {
        if (targetShader != null)
        {
            return;
        }

        AssetBundle.UnloadAllAssetBundles(true);

        var bundle = ABHelper.LoadBundle(SHADER_PATH.ToLower());

        if (bundle != null)
        {
            targetShader = bundle.LoadAsset<Shader>(SHADER_PATH.ToLower());

            //Debug.Log("Load shader."); 
        }
    } 
     
    private void Awake()
    {
        if (!Application.isEditor)
        {
            DontDestroyOnLoad(this);
        }

        if (!Application.isEditor)
        {
            StartCoroutine(LoadShaderAsync());
        }
    }

    private void OnEnable()
    {
        if (current != null)
        {
            enabled = false;

            return;
        }

        current = this;

    }

    private void OnDisable()
    {
        if (current == this)
        {
            current = null;
        }
    }

    public void UpdateShaders()
    {
        UpdateShaders(targetShader , false);
    }

    public void UpdateShaders(Shader shader , bool forceReplace)
    {
        if (targetShader == null)
        {
            return;
        }

        if (registerMats != null)
        {
            foreach (var m in registerMats)
            {
                //Debug.Log("shader:" + m.shader);

                MatHelper.ReplaceHiddenShader(m, shader , forceReplace);
            }
        }

        if (registerRenderers != null)
        {
            foreach (var r in registerRenderers)
            {
                foreach (var m in r.sharedMaterials)
                {
                    MatHelper.ReplaceHiddenShader(m , shader , forceReplace);
                }
            }
        }
    }

    private IEnumerator LoadShaderAsync()
    {
        AssetBundle.UnloadAllAssetBundles(true);

        yield return ABHelper.LoadBundleAsync(SHADER_PATH.ToLower() , (bundle)=> {
            targetShader = bundle.LoadAsset<Shader>(SHADER_PATH.ToLower());

            Debug.Log("LoadShaderAsync:" + targetShader);

            UpdateShaders();
        });
    }
}
