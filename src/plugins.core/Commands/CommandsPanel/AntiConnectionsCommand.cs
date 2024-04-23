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

            Methods methods = new Methods();

            string userInput = methods.CreateTextField("Введите тип стен для совмещения");
            string type = userInput;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType();

            List<Element> wallElements = new List<Element>();
            foreach (Wall wall in collector)
            {
                Parameter typeParam = wall.LookupParameter("Тип");
                if (typeParam != null && typeParam.AsValueString() == type)
                {
                    wallElements.Add(wall);
                }
            }

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

