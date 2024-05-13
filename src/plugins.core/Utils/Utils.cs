namespace plugins.core
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Form = System.Windows.Forms.Form;
    class Utils
    {

        public int CreateDialogWindow(string formText)
        {
            Form form = new Form();
            form.Text = formText;
            form.Size = new System.Drawing.Size(500, 130);

            RadioButton radioButtonSelected = new RadioButton();
            radioButtonSelected.Text = "Применить операцию к выбранным объектам";
            radioButtonSelected.Location = new System.Drawing.Point(20, 20);
            radioButtonSelected.Width = 350;
            radioButtonSelected.Checked = true;
            RadioButton radioButtonAll = new RadioButton();
            radioButtonAll.Text = "Применить операцию ко всем объектам заданного типа";
            radioButtonAll.Location = new System.Drawing.Point(20, 50);
            radioButtonAll.Width = 350;

            Button buttonOK = new Button();
            buttonOK.Text = "OK";
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(400, 35);

            form.Controls.AddRange(new System.Windows.Forms.Control[] { radioButtonSelected, radioButtonAll, buttonOK });

            // Отображение диалогового окна
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Определение выбора пользователя
                if (radioButtonSelected.Checked)
                {
                    return 1; // 1, если выбрано "Применить операцию к выбранным объектам"
                }
                else if (radioButtonAll.Checked)
                {
                    return 2; // 2, если выбрано "Применить операцию ко всем объектам заданного типа"
                }
            }
            else return 0; // 0, если нажата кнопка "Отмена" или окно закрыто без выбора

            return 0;
        }

        public int CreateChooseForm()
        {
            // Создание диалогового окна
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Выберите тип элемента";
            form.Size = new System.Drawing.Size(350, 150);
            form.StartPosition = FormStartPosition.CenterScreen;

            // Добавление радиокнопок для выбора
            RadioButton radioButtonWalls = new RadioButton();
            radioButtonWalls.Text = "Стены";
            radioButtonWalls.Location = new System.Drawing.Point(20, 20);
            radioButtonWalls.Checked = true; // По умолчанию выбрано "Стены"
            RadioButton radioButtonColumns = new RadioButton();
            radioButtonColumns.Text = "Колонны";
            radioButtonColumns.Location = new System.Drawing.Point(20, 50);

            // Кнопка для подтверждения выбора
            Button buttonOK = new Button();
            buttonOK.Text = "OK";
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(150, 20);

            // Добавление элементов на форму
            form.Controls.AddRange(new System.Windows.Forms.Control[] { radioButtonWalls, radioButtonColumns, buttonOK });

            // Отображение диалогового окна
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Определение выбора пользователя
                if (radioButtonWalls.Checked)
                {
                    return 1; // 1, если выбрано "Стены"
                }
                else if (radioButtonColumns.Checked)
                {
                    return 2; // 2, если выбрано "Колонны"
                }
            }

            return 0; // 0, если нажата кнопка "Отмена" или окно закрыто без выбора
        }

        public List<Element> GetIntersectionsWithElements(List<Element> elements, Document doc)
        {
            List<Element> intersections = new List<Element>();

            foreach (Element element in elements)
            {
                BoundingBoxXYZ bb = element.get_BoundingBox(doc.ActiveView);
                Outline outline = new Outline(bb.Min, bb.Max);
                BoundingBoxIntersectsFilter bbfilter = new BoundingBoxIntersectsFilter(outline);

                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                List<ElementId> ids_exclude = new List<ElementId>();
                ids_exclude.Add(element.Id);

                foreach (Element intersectedElement in collector.Excluding(ids_exclude).WherePasses(bbfilter))
                {
                    intersections.Add(intersectedElement);
                }
            }

            return intersections;
        }

        public bool CutGeometry(Document doc, Element item1, Element item2)
        {
            try
            {
                SolidSolidCutUtils.AddCutBetweenSolids(doc, item1, item2);
                return true;
            }
            catch
            {
                try
                {
                    InstanceVoidCutUtils.AddInstanceVoidCut(doc, item1, item2);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public List<Element> GetElementsByType(string elementType, UIDocument uiDoc, Document doc)
        {
            Utils methods = new Utils();
            List<Element> elements = new List<Element>();
            int resultDialog = methods.CreateDialogWindow("Выберите объекты для совмещения");

            if (resultDialog == 1 || resultDialog == 2)
            {
                try
                {
                    IList<Reference> selectedElements = new List<Reference>();
                    if (elementType == "walls")
                        selectedElements = uiDoc.Selection.PickObjects(ObjectType.Element, new WallSelectionFilter(), "Выберите объекты");
                    else if (elementType == "columns")
                        selectedElements = uiDoc.Selection.PickObjects(ObjectType.Element, new ColumnSelectionFilter(), "Выберите объекты");
                    else if (elementType == "holes")
                        selectedElements = uiDoc.Selection.PickObjects(ObjectType.Element, new HoleSelectionFilter(), "Выберите объекты");
                    foreach (var reference in selectedElements)
                    {
                        elements.Add(doc.GetElement(reference));
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                    return null;
                }

                if (resultDialog == 2)
                {
                    if (elementType != "holes")
                    {
                        string type = elements[0].LookupParameter("Тип").AsValueString();
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        if (elementType == "walls")
                            collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType();
                        else if (elementType == "columns")
                            collector.OfCategory(BuiltInCategory.OST_Columns).WhereElementIsNotElementType();

                        elements = new List<Element>();
                        foreach (var element in collector)
                        {
                            Parameter typeParam = element.LookupParameter("Тип");
                            if (typeParam != null && typeParam.AsValueString() == type)
                            {
                                elements.Add(element);
                            }
                        }
                    }
                    else
                    {
                        elements = FindFamilyElements(doc, elements[0]);
                    }
                }
            }
            else if (resultDialog == 0)
            {
                TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                return null;
            }

            return elements;
        }

        public List<Element> FindFamilyElements(Document doc, Element element)
        {
            try
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                FamilySymbol family = familyInstance.Symbol;
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, family.Id);
                List<Element> familyInstances = new List<Element>(collector.WherePasses(filter).ToElements());
                return familyInstances;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return null;
            }
        }

        public class WallSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }

        public class ColumnSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }

        public class HoleSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }
    }
}
