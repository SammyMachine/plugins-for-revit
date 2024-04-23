namespace plugins.core
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using plugins.core;
    using System.Collections.Generic;
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CuttingHolesCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = uiApp.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            Methods methods = new Methods();

            // Показать форму для выбора типа семейства
            string selectedFamilyType = methods.ShowFamilyTypeSelectionForm(doc);
            if (selectedFamilyType == null)
            {
                // Обработка отмены или закрытия окна без выбора
                TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                return Result.Cancelled;
            }

            // Получение выбранных элементов
            List<Element> selectedElements = new List<Element>(new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .ToElements());

            // Получение списка пересечений
            HashSet<ElementId> intersectionIds = methods.GetIntersectionsWithElements(selectedElements, doc);

            // Создание списка для хранения элементов
            List<Element> intersectingElements = new List<Element>();

            // Нахождение элементов по их идентификаторам
            foreach (ElementId elementId in intersectionIds)
            {
                Element element = doc.GetElement(elementId);
                intersectingElements.Add(element);
            }

            // Начало транзакции
            Transaction transaction = new Transaction(doc, "Cut Geometry");
            transaction.Start();

            List<bool> cutResults = new List<bool>();

            // Выполнение операции вырезания геометрии
            foreach (Element item1 in intersectingElements)
            {
                foreach (Element item2 in selectedElements)
                {
                    cutResults.Add(methods.CutGeometry(doc, item1, item2));
                }
            }

            // Завершение транзакции
            transaction.Commit();

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            return typeof(CuttingHolesCommand).Namespace + "." + nameof(CuttingHolesCommand);
        }
    }
}
