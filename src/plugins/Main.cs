namespace plugins
{
    using Autodesk.Revit.UI;

    public class Main : IExternalApplication
    {
        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            var ui = new SetupInterface();
            ui.Initialize(application);
            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
