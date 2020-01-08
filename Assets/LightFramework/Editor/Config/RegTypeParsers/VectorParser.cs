using System.Collections.Generic;
using System.Text;
using LightFramework.Editor.Config;
using LightFramework.Base;

namespace LightFramework.Config
{
    public class VectorParser : IRegTypeParser
    {
        public string TypeName { get; set; } = "vector";

        public string PackData(object data, ConfigEditor.Variable variable = null)
        {
            var split = ',';
            StringBuilder content = new StringBuilder();
            var vectorList = new List<float>();
            if (data is Vector3)
            {
                Vector3 vector3 = (Vector3) data;
                vectorList.Add(vector3.x);
                vectorList.Add(vector3.y);
                vectorList.Add(vector3.z);
            }

            if (data is Vector2)
            {
                Vector2 vector2 = (Vector2) data;
                vectorList.Add(vector2.x);
                vectorList.Add(vector2.y);
            }

            if (data is Vector4)
            {
                Vector4 vector4 = (Vector4) data;
                vectorList.Add(vector4.x);
                vectorList.Add(vector4.y);
                vectorList.Add(vector4.z);
                vectorList.Add(vector4.w);
            }

            for (int i = 0; i < vectorList.Count; i++)
            {
                content.Append(vectorList[i]).Append(split);
            }

            content.Remove(content.Length - 1, 1);
            return content.ToString();
        }

        public void UnPackData(object item, string block, ConfigEditor.Variable variable)
        {
            var list = block.Split(',');
            object value = null;
            switch (list.Length)
            {
                case 3:
                    Vector3 vector3 = new Vector3(int.Parse(list[0]), int.Parse(list[0]), int.Parse(list[0]));
                    value = vector3;
                    break;
                case 2:
                    Vector4 vector4 = new Vector4(int.Parse(list[0]), int.Parse(list[1]), int.Parse(list[2]),
                        int.Parse(list[3]));
                    value = vector4;
                    break;
                case 4:
                    Vector2 vector2 = new Vector2(int.Parse(list[0]), int.Parse(list[1]));
                    value = vector2;
                    break;
            }
            item.SetValue(variable.Name, value);
        }
    }
}