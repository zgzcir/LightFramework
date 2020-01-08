namespace LightFramework.Base
{
    [System.Serializable]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }    
        public void Fill(UnityEngine.Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public static Vector3 zero => new Vector3(0,0,0);
        public static Vector3 one => new Vector3(1,1,1);
        public UnityEngine.Vector3 UnityVector3 => new UnityEngine.Vector3(x, y, z);
    }
    [System.Serializable]
    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z,float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }    
        public void Fill(UnityEngine.Vector4 v4)
        {
            x = v4.x;
            y = v4.y;
            z = v4.z;
            w = v4.w;
        }

        public UnityEngine.Vector4 UnityVector4 => new UnityEngine.Vector4(x, y, z,w);
    }
    [System.Serializable]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }    
        public void Fill(UnityEngine.Vector2 v2)
        {
            x = v2.x;
            y = v2.y;
        }

        public UnityEngine.Vector2 UnityVector2 => new UnityEngine.Vector3(x, y);
    }
}