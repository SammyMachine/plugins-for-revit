namespace plugins.core
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

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
            Utils methods = new Utils();
            int chooseFlag = methods.CreateChooseForm();
            bool isWall = false;
            List<Element> selectedElements = new List<Element>();
            if (chooseFlag == 1)
            {
                selectedElements = methods.GetElementsByType("walls", uiDoc, doc);
                isWall = true;
            }
            else if (chooseFlag == 2)
            {
                selectedElements = methods.GetElementsByType("columns", uiDoc, doc);
                isWall = false;
            }
            else if (chooseFlag == 0)
            {
                TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                return Result.Cancelled;
            }
            Dictionary<Element, List<Level>> intersectingLevelsMap = methods.GetIntersectingLevels(selectedElements, doc);
            foreach (var kvp in intersectingLevelsMap)
            {
                List<Level> levels = kvp.Value;
                levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));
            }


            // Начало транзакции
            Transaction transactionSecond = new Transaction(doc, "Copy Elements");
            transactionSecond.Start();

            foreach (var kvp in intersectingLevelsMap)
            {
                Element selectedElement = kvp.Key;
                List<Level> levels = kvp.Value;

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
