namespace plugins.core
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SplittingWallsAndColumnsCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = uiApp.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            Methods methods = new Methods();

            int chooseFlag = methods.CreateChooseForm();

            string userInput = "";
            bool isWall = false;

            if (chooseFlag == 1)
            {
                userInput = methods.CreateTextField("Введите тип стен для разрезания");
                isWall = true;
            }
            else if (chooseFlag == 2)
            {
                userInput = methods.CreateTextField("Введите тип колонн для разрезания");
                isWall = false;
            }
            else if (chooseFlag == 0)
            {
                // Обработка случая отмены или закрытия окна без выбора
                TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                return Result.Cancelled;
            }

            string type = userInput;

            Transaction transactionFirst = new Transaction(doc, "Get elements");
            transactionFirst.Start();
            // Выбор категории элементов в зависимости от типа
            BuiltInCategory category = isWall ? BuiltInCategory.OST_Walls : BuiltInCategory.OST_Columns;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(category).WhereElementIsNotElementType().ToElements();

            // Фильтрация элементов по выбранному типу
            List<Element> selectedElements = new List<Element>();

            foreach (Element e in collector)
            {
                Parameter typeParam = e.LookupParameter("Тип");
                if (typeParam != null && typeParam.AsValueString() == type)
                {
                    selectedElements.Add(e);
                }
            }


            // Получение списка уровней
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc).OfClass(typeof(Level));
            List<Level> levels = new List<Level>(levelCollector.ToElements().Cast<Level>());
            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));
            transactionFirst.Commit();

            // Начало транзакции
            Transaction transactionSecond = new Transaction(doc, "Copy Elements");
            transactionSecond.Start();

            foreach (Element selectedElement in selectedElements)
            {
                List<ElementId> copiedElements = new List<ElementId>();

                // Получение параметров уровней для элементов
                string baseConstraintParamName = isWall ? "Зависимость снизу" : "Зависимость снизу";
                string topConstraintParamName = isWall ? "Зависимость сверху" : "Зависимость сверху";

                Parameter baseConstraintParam = selectedElement.LookupParameter(baseConstraintParamName);
                Parameter topConstraintParam = selectedElement.LookupParameter(topConstraintParamName);

                ElementId baseLevelId = baseConstraintParam.AsElementId();
                ElementId topLevelId = topConstraintParam.AsElementId();

                int baseLevelIndex = levels.FindIndex(level => level.Id == baseLevelId);
                int topLevelIndex = levels.FindIndex(level => level.Id == topLevelId);

                while (baseLevelIndex < topLevelIndex)
                {
                    try
                    {
                        // Копирование элемента
                        ICollection<ElementId> copiedElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, new XYZ(0, 0, 0));
                        foreach (ElementId copiedElementId in copiedElementIds)
                        {
                            Element copiedElement = doc.GetElement(copiedElementId);

                            // Установка параметров уровней для скопированного элемента
                            Parameter baseConstraintParamCopy = copiedElement.LookupParameter(baseConstraintParamName);
                            baseConstraintParamCopy.Set(levels[baseLevelIndex].Id);

                            Parameter topConstraintParamCopy = copiedElement.LookupParameter(topConstraintParamName);
                            topConstraintParamCopy.Set(levels[baseLevelIndex + 1].Id);

                            copiedElements.Add(copiedElementId);
                        }
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Error", e.Message);
                    }

                    baseLevelIndex++;
                }

                // Удаление исходного элемента
                doc.Delete(selectedElement.Id);
            }

            // Завершение транзакции
            transactionSecond.Commit();
            return Result.Succeeded;
        }

        public static string GetPath()
        {
            return typeof(SplittingWallsAndColumnsCommand).Namespace + "." + nameof(SplittingWallsAndColumnsCommand);
        }
    }
}
