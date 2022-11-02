using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HzbDepthTexMaker : MonoBehaviour
{
    public RenderTexture hzbDepth;
    public Shader hzbShader;
    private Material hzbMat;
    public bool stopMpde;
    void Start() 
    {
        hzbMat = new Material(hzbShader);
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        hzbDepth = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RHalf);
        hzbDepth.autoGenerateMips = false;

        hzbDepth.useMipMap = true;
        hzbDepth.filterMode = FilterMode.Point;
        hzbDepth.Create();
        HzbInstance.HZB_Depth = hzbDepth;
        
    }
    
    void OnDestroy()
    {
        hzbDepth.Release();
        Destroy(hzbDepth);
    }
    
    int ID_DepthTexture =Shader.PropertyToID("_DepthTexture");
    int ID_InvSize = Shader.PropertyToID("_InvSize");
    
    void Update()
    {
        if (stopMpde)
        {
            return;
        }
        
        int w = hzbDepth.width;
        int h = hzbDepth.height;
        int level = 0;

        RenderTexture lastRt = null;
       
        RenderTexture tempRT;

        while (h > 8)
        {
            tempRT = RenderTexture.GetTemporary(w, h, 0, hzbDepth.format);
            tempRT.filterMode = FilterMode.Point;
            if (lastRt == null)
            {
                Graphics.Blit(Shader.GetGlobalTexture("_CameraDepthTexture"), tempRT);
            }
            else
            {
                hzbMat.SetVector(ID_InvSize, new Vector4(2.0f / w, 2.0f / h, 0, 0));    //src原图尺寸为wh的两倍，用于采样的
                hzbMat.SetTexture(ID_DepthTexture, lastRt);
                Graphics.Blit(null, tempRT, hzbMat);    //如果把lastRt放进这里作为第一个参数，则只需要将_DepthTexture 替换为 _MainTex 
                RenderTexture.ReleaseTemporary(lastRt);
            }
            Graphics.CopyTexture(tempRT, 0, 0, hzbDepth, 0, level);
            lastRt = tempRT;

            w /= 2;
            h /= 2;
            level++;
        }
        RenderTexture.ReleaseTemporary(lastRt);
    }
}
