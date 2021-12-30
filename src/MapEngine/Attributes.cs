using System;

namespace MapEngine
{
    [AttributeUsage(AttributeTargets.Method)] public class OnAddComponentAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class OnRemoveComponentAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class BeforeMapChangeAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public class AfterMapChangeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        public float Priority { get; }
        public PriorityAttribute(float priority) => Priority = priority;
    }
}
