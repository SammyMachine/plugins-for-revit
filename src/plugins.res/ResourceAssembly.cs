namespace plugins.res
{
    using System.Reflection;
    public static class ResourceAssembly
    {
        public static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
        public static string GetNamespace()
        {
            return typeof(ResourceAssembly).Namespace + ".";
        }
    }
}