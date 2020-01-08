using LightFramework.Editor.Config;

namespace LightFramework.Config
{
    public interface IRegTypeParser
    {
        string TypeName { get; set; }
        string PackData(object data,ConfigEditor.Variable variable=null);
        void UnPackData(object item, string block, ConfigEditor.Variable variable);
    }
}