using UnityEngine;
using UnityEngine.Rendering;

public class HzbInstance : MonoBehaviour
{
    public ComputeShader cullingComputeShader;
    public Mesh mesh;
    public Material drawMat;
    public static RenderTexture HZB_Depth;
    public Texture testDepth;
    
    //以下为测试开关的临时代码 实际工程不会用到  所以用性能低下的OnGUI 写了下
    public bool useHzb = false;
    public bool updateCulling = true;
    public bool computeshaderMode = true;

    ComputeBuffer bufferWithArgs;
    private uint[] args;
    private int CSCullingID;
    private Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    ComputeBuffer posBuffer;
    static int staticRandomID = 0;
   
    
    void Start () {
        //测试  16万棵草 computeshader 模式
        int count = 400*400;
        var terrain=	FindObjectOfType<Terrain>();
        Vector3[] posList = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            int x = i % 400;
            int z = i / 400;
            posList[i] = new Vector3(x*2f+ StaticRandom(),0,z*2f+ StaticRandom());
            posList[i].y = terrain.SampleHeight(posList[i]);
        }

 
        args = new uint[] { mesh.GetIndexCount(0), 0, 0, 0, 0 };
        bufferWithArgs = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        bufferWithArgs.SetData(args);
        CSCullingID = cullingComputeShader.FindKernel("CSCulling");

        posBuffer = new ComputeBuffer(count, 4*3);
        posBuffer.SetData(posList);
        var posVisibleBuffer = new ComputeBuffer(count, 4*3);
        cullingComputeShader.SetBuffer(CSCullingID, "bufferWithArgs", bufferWithArgs);
        cullingComputeShader.SetBuffer(CSCullingID, "posAllBuffer", posBuffer);
        cullingComputeShader.SetBuffer(CSCullingID, "posVisibleBuffer", posVisibleBuffer);
        cullingComputeShader.SetFloat("ReversedZ",SystemInfo.usesReversedZBuffer?1.0f:0.0f);
        drawMat.SetBuffer("posVisibleBuffer", posVisibleBuffer);
        drawMat.EnableKeyword("_GPUGRASS");
 
    }
    
    void Update() {
        if (computeshaderMode == false) return;
        if (updateCulling)
        {
            culling();
        }
        Graphics.DrawMeshInstancedIndirect(mesh, 0, drawMat, bounds, bufferWithArgs, 0, null, ShadowCastingMode.Off, false);
    }
    
    void culling() {
        cullingComputeShader.SetFloat("useHzb", useHzb ? 1 : 0);
        args[1] = 0;
        bufferWithArgs.SetData(args);
        if (HZB_Depth != null)
        {
            cullingComputeShader.SetTexture(CSCullingID, "HZB_Depth", HZB_Depth);
			 
        }
		 
        cullingComputeShader.SetVector("cmrPos", Camera.main.transform.position);
        cullingComputeShader.SetVector("cmrDir", Camera.main.transform.forward);
        cullingComputeShader.SetFloat("cmrHalfFov", Camera.main.fieldOfView/2);
        var m = GL.GetGPUProjectionMatrix( Camera.main.projectionMatrix,false) * Camera.main.worldToCameraMatrix;

        //高版本 可用  computeShader.SetMatrix("matrix_VP", m); 代替 下面数组传入
        /*
        float[] mlist = new float[] {
            m.m00,m.m10,m.m20,m.m30,
            m.m01,m.m11,m.m21,m.m31,
            m.m02,m.m12,m.m22,m.m32,
            m.m03,m.m13,m.m23,m.m33
        };
        
        cullingComputeShader.SetFloats("matrix_VP", mlist);
        */
        cullingComputeShader.SetMatrix("matrix_VP",m);
        cullingComputeShader.Dispatch(CSCullingID, 400 / 16, 400 / 16, 1);
    }

    
    float StaticRandom() {
        float v = 0;
        v = Mathf.Abs( Mathf.Sin(staticRandomID)) * 1000+  Mathf.Abs(Mathf.Cos(staticRandomID*0.1f)) * 100 ;
        v -= (int)v;
		 
        staticRandomID++;
        return v;
    }


    private void OnDisable()
    {
        drawMat.DisableKeyword("_GPUGRASS");
    }

    private void OnGUI()
    {
        /*
        if (GUILayout.Button("computeShaderMode :" + (computeshaderMode? "on" :"off") ))
        {
            computeshaderMode = !computeshaderMode;
            var terrain = FindObjectOfType<Terrain>();
            if (computeshaderMode)
            {
                terrain.detailObjectDistance = 0;
            }
            else
            {
                terrain.detailObjectDistance = 150;
            }
        }
        */
    }
}
