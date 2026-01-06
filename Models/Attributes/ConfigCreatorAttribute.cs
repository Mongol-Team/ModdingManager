using System;

namespace Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConfigCreatorAttribute : Attribute
    {
        public ConfigCreatorType CreatorType { get; }

        public ConfigCreatorAttribute(ConfigCreatorType creatorType)
        {
            CreatorType = creatorType;
        }
    }
}

