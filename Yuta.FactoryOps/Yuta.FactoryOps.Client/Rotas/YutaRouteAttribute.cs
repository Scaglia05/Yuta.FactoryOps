using System;

namespace Yuta.FactoryOps.Client.Rotas
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class YutaRouteAttribute : Attribute
    {
        public string Template { get; }

        public YutaRouteAttribute(string template)
        {
            Template = template.StartsWith("/") ? template : $"/{template.ToLower()}";
        }
    }
}