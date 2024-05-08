namespace plugins.core
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System.Collections.Generic;
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AntiConnectionsCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = uiApp.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            Utils methods = new Utils();
            List<Element> wallElements = methods.GetElementsByType("walls", uiDoc, doc);

            Transaction transaction = new Transaction(doc, "Update Walls");
            transaction.Start();
            foreach (Wall wall in wallElements)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (WallUtils.IsWallJoinAllowedAtEnd(wall, i))
                        WallUtils.DisallowWallJoinAtEnd(wall, i);
                    else
                        WallUtils.AllowWallJoinAtEnd(wall, i);
                }
            }
            transaction.Commit();
            return Result.Succeeded;
        }

        public static string GetPath()
        {
            return typeof(AntiConnectionsCommand).Namespace + "." + nameof(AntiConnectionsCommand);
        }

    }
}

