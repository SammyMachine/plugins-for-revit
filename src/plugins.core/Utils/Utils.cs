namespace plugins.core
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
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

        public string ShowFamilyTypeSelectionForm(Document doc)
        {
            // Создание диалогового окна
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Text = "Выберите тип семейства";
            form.Size = new System.Drawing.Size(400, 150);
            form.StartPosition = FormStartPosition.CenterScreen;

            // Создание выпадающего списка для выбора семейства
            System.Windows.Forms.ComboBox comboBoxFamilyTypes = new System.Windows.Forms.ComboBox();
            comboBoxFamilyTypes.Location = new System.Drawing.Point(20, 20);
            comboBoxFamilyTypes.Size = new System.Drawing.Size(200, 20);

            // Добавление семейств в выпадающий список
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> familyTypeIds = collector.OfClass(typeof(FamilySymbol)).ToElementIds();
            foreach (ElementId familyTypeId in familyTypeIds)
            {
                FamilySymbol familyType = doc.GetElement(familyTypeId) as FamilySymbol;
                comboBoxFamilyTypes.Items.Add(familyType.Name);
            }

            // Кнопка для подтверждения выбора
            Button buttonOK = new Button();
            buttonOK.Text = "OK";
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(20, 50);

            // Обработчик события нажатия кнопки OK
            buttonOK.Click += (sender, e) =>
            {
                form.Close();
            };

            // Добавление элементов на форму
            form.Controls.AddRange(new System.Windows.Forms.Control[] { comboBoxFamilyTypes, buttonOK });

            // Отображение диалогового окна
            form.ShowDialog();

            // Возврат выбранного типа семейства
            if (comboBoxFamilyTypes.SelectedItem != null)
            {
                return comboBoxFamilyTypes.SelectedItem.ToString();
            }
            else
            {
                return null;
            }
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
            }
            else if (resultDialog == 0)
            {
                TaskDialog.Show("Внимание", "Выбор отменен или окно закрыто без выбора.");
                return null;
            }

            return elements;
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

        //public string CreateTextField(string formText)
        //{
        //    // Создание формы для ввода текста
        //    Form userInputForm = new Form();
        //    userInputForm.Text = formText;
        //    userInputForm.Size = new System.Drawing.Size(500, 130);

        //    // Создание текстового поля для ввода
        //    System.Windows.Forms.TextBox userInputTextBox = new System.Windows.Forms.TextBox();
        //    userInputTextBox.Location = new System.Drawing.Point(20, 20);
        //    userInputTextBox.Size = new System.Drawing.Size(450, 20);

        //    // Создание кнопки для подтверждения ввода
        //    Button confirmButton = new Button();
        //    confirmButton.Text = "OK";
        //    confirmButton.Location = new System.Drawing.Point(20, 50);

        //    // Объявление переменной для хранения введенного пользователем текста
        //    string userInput = "";

        //    // Обработчик события нажатия кнопки
        //    confirmButton.Click += (sender, e) =>
        //    {
        //        // Получение введенного пользователем текста
        //        userInput = userInputTextBox.Text;

        //        // Закрытие формы после получения текста
        //        userInputForm.Close();
        //    };

        //    // Добавление элементов на форму
        //    userInputForm.Controls.Add(userInputTextBox);
        //    userInputForm.Controls.Add(confirmButton);

        //    // Отображение формы
        //    userInputForm.ShowDialog();

        //    // Возвращение введенного пользователем текста
        //    return userInput;
        //}
    }
}
