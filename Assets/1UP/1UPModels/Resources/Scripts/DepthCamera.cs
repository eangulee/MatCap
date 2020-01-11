using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class DepthCamera : MonoBehaviour
{
    public Transform targetTransform;
    Transform prevTargetTranform;

    public bool renderChildren;
    bool prevRenderChild;
    public int shadowIndex = 0;
    [Range(0, 1)]
    public float shadowOffset = 1.0f;

    public bool followTarget;
    private Vector3 posOffset = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;

    public LayerMask layerMask;
    private LayerMask prevMask;
    private CommandBuffer cbuffer = null;
    private RenderTexture shadowRT = null;
    private Camera cam = null;

    private Dictionary<string, int> layerDict = new Dictionary<string, int>();

    public enum TexSize : int
    {
        s512 = 512,
        s256 = 256,
        s128 = 128,
    }
    public TexSize texWidth = TexSize.s128;
    private int prevWidth;
    public TexSize texHeight = TexSize.s128;
    private int prevHeight;
    

    void OnEnable()
    {
        cam = this.GetComponent<Camera>();
        if (cam == null) return;
        shadowRT = new RenderTexture(
            (int)texWidth, (int)texHeight, 16, RenderTextureFormat.Depth);
        cam.targetTexture = new RenderTexture(
            (int)texWidth, (int)texHeight, 16, RenderTextureFormat.Depth);

        cbuffer = new CommandBuffer();
        cbuffer.name = cam.name;

        cbuffer.SetRenderTarget(shadowRT);
        cbuffer.ClearRenderTarget(true, true, Color.blue);
        RenderCamera();
        cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cbuffer);

        prevTargetTranform = targetTransform;
        prevRenderChild = renderChildren;
        prevMask = layerMask;
        prevWidth = (int)texWidth;
        prevHeight = (int)texHeight;
    }

    void OnDisable()
    {
        if (cbuffer == null) return;
        if (cam == null) return;
        if (targetTransform == null) return;
        //移除事件，清理资源
        cam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cbuffer);
        cbuffer.Clear();
        shadowRT.Release();
        var targetRenderer = targetTransform.GetComponent<Renderer>();
        ResetProj(targetRenderer);

        foreach (Transform child in targetTransform)
        {
            var renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                ResetProj(renderer);
        }
    }

    void Start()
    {
        if (targetTransform == null) return;
        prevPosition = targetTransform.position;
        posOffset = targetTransform.position - prevPosition;
    }

    void ResetProj(Renderer renderer)
    {
        if (renderer == null || shadowRT == null) return;
        if (renderer.sharedMaterial == null) return;
        renderer.sharedMaterial.SetFloat("_ShadowOffset" + shadowIndex.ToString(), 0.0f);
    }

    void UpdateProj(Renderer renderer)
    {
        if (renderer == null || shadowRT == null) return;
        if (!renderer.isVisible) return;
        var layer = 1 << renderer.gameObject.layer;
        if (layer != (layer & layerMask) || !cam.isActiveAndEnabled)
        {
            ResetProj(renderer);
            return;
        }
        Matrix4x4 proj =
        GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 lightProj = proj * cam.worldToCameraMatrix;
        renderer.sharedMaterial.SetFloat("_ShadowOffset" + shadowIndex.ToString(), shadowOffset);
        renderer.sharedMaterial.SetMatrix("_ShadowProj" + shadowIndex.ToString(), lightProj);
        renderer.sharedMaterial.SetTexture("_ShadowTex" + shadowIndex.ToString(), shadowRT);
    }

    void RenderToCommandBuffer(Renderer renderer)
    {
        if (renderer == null || shadowRT == null) return;
        if (!renderer.isVisible) return;
        var layer = 1 << renderer.gameObject.layer;

        if (!layerDict.ContainsKey(renderer.gameObject.name))
        {
            layerDict.Add(renderer.gameObject.name, layer);
        }
        else
        {
            layerDict[renderer.gameObject.name] = layer;
        }

        if (layer != (layer & layerMask))
        {
            ResetProj(renderer);
            return;
        }
        Matrix4x4 proj =
        GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 lightProj = proj * cam.worldToCameraMatrix;
        renderer.sharedMaterial.SetFloat("_ShadowOffset" + shadowIndex.ToString(), shadowOffset);
        renderer.sharedMaterial.SetMatrix("_ShadowProj" + shadowIndex.ToString(), lightProj);
        renderer.sharedMaterial.SetTexture("_ShadowTex" + shadowIndex.ToString(), shadowRT);
        cbuffer.DrawRenderer(renderer, renderer.sharedMaterial);
    }

    void RenderCamera()
    {
        if (cbuffer == null || targetTransform == null) return;
        var targetRenderer = targetTransform.GetComponent<Renderer>();
        RenderToCommandBuffer(targetRenderer);

        if (renderChildren)
        {
            foreach (Transform child in targetTransform)
            {
                var renderers = child.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    RenderToCommandBuffer(renderer);
                }
            }
        }
    }

    bool checkLayerUpdate(Renderer renderer)
    {
        if (renderer == null) return false;
        var layer = 1 << renderer.gameObject.layer;
        if (!layerDict.ContainsKey(renderer.gameObject.name))
        {
            layerDict.Add(renderer.gameObject.name, layer);
            return true;
        }

        if (layerDict[renderer.gameObject.name] != layer)
        {
            layerDict[renderer.gameObject.name] = layer;
            return true;
        }

        return false;
    }

    private void OnPreCull()
    {
        var c = cam;
        bool updateBuffer = false;
        if (targetTransform == null) return;
        if (followTarget)
        {
            posOffset = targetTransform.position - prevPosition;
            transform.position += posOffset;
            prevPosition = targetTransform.position;
        }
        if (prevTargetTranform != targetTransform)
        {
            updateBuffer = true;
            if (prevTargetTranform != null)
            {
                var prevRenderer = prevTargetTranform.GetComponent<Renderer>();
                UpdateProj(prevRenderer);

                foreach (Transform child in prevTargetTranform)
                {
                    var renderers = child.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                        UpdateProj(renderer);
                }
            }
            prevTargetTranform = targetTransform;
        }

        if (prevRenderChild != renderChildren)
        {
            updateBuffer = true;
            prevRenderChild = renderChildren;
        }

        if (prevMask.value != layerMask.value)
        {
            updateBuffer = true;
            prevMask = layerMask;
        }

        bool updateTexsize = false;
        if (prevWidth != (int)texWidth)
        {
            prevWidth = (int)texWidth;
            updateTexsize = true;
        }

        if (prevHeight != (int)texHeight)
        {
            prevHeight = (int)texHeight;
            updateTexsize = true;
        }

        bool updateLayer = false;
        var targetRenderer = targetTransform.GetComponent<Renderer>();
        updateLayer = checkLayerUpdate(targetRenderer);

        if (!updateLayer && renderChildren)
        {
            foreach (Transform child in targetTransform)
            {
                var renderers = child.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    updateLayer = checkLayerUpdate(renderer);
                    if (updateLayer) break;
                }
                if (updateLayer) break;
            }
        }

        updateBuffer = updateBuffer | updateTexsize | updateLayer;

        if (updateBuffer)
        {
            //Debug.Log("UpdateBuffer");
            cam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cbuffer);
            cbuffer.Clear();
            if (updateTexsize)
            {
                shadowRT.Release();
                shadowRT = new RenderTexture(
                    (int)texWidth, (int)texHeight, 
                    16, RenderTextureFormat.Depth);
            }
            cbuffer.SetRenderTarget(shadowRT);
            cbuffer.ClearRenderTarget(true, true, Color.blue);
            RenderCamera();
            cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cbuffer);
        }
        UpdateProj(targetRenderer);

        
        foreach (Transform child in targetTransform)
        {
            var renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                UpdateProj(renderer);
        }
    }
}
