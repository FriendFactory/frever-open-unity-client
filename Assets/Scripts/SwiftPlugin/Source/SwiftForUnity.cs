using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SwiftPlugin.Source
{
    public static class SwiftForUnity
    {
        public static Vector4[] centerTransform4X4 = new Vector4[4];
        public static bool lastFrameCapturedVerts = false;
        public static uint lastGyroCalulatedRotation;
        #region Declare external C interface

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _sayHelloToUnity();
    [DllImport("__Internal")]
    private static extern void _viewDidLoad();
    [DllImport("__Internal")]
    private static extern bool _getFaceVertices( [In, Out] float[] outValues, int length, [In, Out] float[] outCenterValues, int Centerlength );
    [DllImport("__Internal")]
    private static extern uint _getLastRotation( );
#endif

        #endregion

        #region Wrapped methods and properties

        public static string HelloFromSwift()
        {
#if UNITY_IOS && !UNITY_EDITOR
       return _sayHelloToUnity();
#else
            return "No Swift found!";
#endif
        }
        public static void viewDidLoad()
        {
#if UNITY_IOS && !UNITY_EDITOR
        _viewDidLoad();
#endif
        }
        
        public static Vector3[] faceVertices
        {
            get
            {
                int vertexCount = 468;
                float[] verts = new float[vertexCount*3];
                
                int centerCount = 4;
                float[] centerTran = new float[centerCount*4];
                
#if UNITY_IOS && !UNITY_EDITOR
                lastFrameCapturedVerts = _getFaceVertices( verts, verts.Length, centerTran, centerTran.Length );
                lastGyroCalulatedRotation = _getLastRotation();
#endif
                Vector3[] vertices = new Vector3[vertexCount];
                for (int index = 0; index < vertices.Length; ++index)
                {
                    int coeffIndex = index * 3;
                    vertices[index] = new Vector3(verts[coeffIndex], verts[coeffIndex + 1], verts[coeffIndex + 2]);
                }
                
                for (int i = 0; i < centerTransform4X4.Length; ++i)
                {
                    int coeffIndex = i * 4; 
                    // on iOS seems like the x and y locations are swapped from Swift 4x4 to here so swapping them (also the y axis is showing -1 insead of 1, but not currently doing anything about that)
                    centerTransform4X4[i] = new Vector4(centerTran[coeffIndex + 1],centerTran[coeffIndex], centerTran[coeffIndex + 2], centerTran[coeffIndex + 3]);
                }

                return vertices;
            }
        }
        
        #endregion
    }
} 
