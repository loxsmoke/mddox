using MdDox.Localization.Interfaces;

namespace MdDox.Localization
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class LocalizedStringsZhCn : ILocalizedStrings
    {
        public string CultureName => "zh-cn";

        public string VersionPrefix => ".v";
        public string DefaultTitleFormat => "{assembly} {version} API 文档";
        public string AllTypes => "所有类型";
        public string CreatedBy => "Created by ";
        public string CreatedByOn => " on ";
        public string CommandLine => "Command line: ";
        public string Values => "值";
        public string Name => "名称";
        public string Summary => "摘要";
        public string BaseClass => "基类：";
        public string Type => "类型";
        public string Properties => "属性";
        public string Constructors => "构造函数";
        public string Methods => "方法";
        public string Returns => "返回";
        public string Fields => "字段";
        public string Parameter => "参数";
        public string Description => "描述";
        public string Enum => "枚举";
        public string Interface => "接口";
        public string Struct => "结构体";
        public string Class => "类";
        public string Record => "记录";
        public string Namespace => "命名空间：";
        public string Examples => "示例";
        public string Remarks => "备注";
    }
}
