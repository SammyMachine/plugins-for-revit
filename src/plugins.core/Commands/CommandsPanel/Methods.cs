namespace plugins.core
{
    using Autodesk.Revit.DB;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Form = System.Windows.Forms.Form;
    class Methods
    {

        public string CreateTextField(string formText)
        {
            // Создание формы для ввода текста
            Form userInputForm = new Form();
            userInputForm.Text = formText;
            userInputForm.Size = new System.Drawing.Size(500, 130);

            // Создание текстового поля для ввода
            TextBox userInputTextBox = new TextBox();
            userInputTextBox.Location = new System.Drawing.Point(20, 20);
            userInputTextBox.Size = new System.Drawing.Size(450, 20);

            // Создание кнопки для подтверждения ввода
            Button confirmButton = new Button();
            confirmButton.Text = "OK";
            confirmButton.Location = new System.Drawing.Point(20, 50);

            // Объявление переменной для хранения введенного пользователем текста
            string userInput = "";

            // Обработчик события нажатия кнопки
            confirmButton.Click += (sender, e) =>
            {
                // Получение введенного пользователем текста
                userInput = userInputTextBox.Text;

                // Закрытие формы после получения текста
                userInputForm.Close();
            };

            // Добавление элементов на форму
            userInputForm.Controls.Add(userInputTextBox);
            userInputForm.Controls.Add(confirmButton);

            // Отображение формы
            userInputForm.ShowDialog();

            // Возвращение введенного пользователем текста
            return userInput;
        }

        public int CreateChooseForm()
        {
            // Создание диалогового окна
            Form form = new Form();
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
            Form form = new Form();
            form.Text = "Выберите тип семейства";
            form.Size = new System.Drawing.Size(400, 150);
            form.StartPosition = FormStartPosition.CenterScreen;

            // Создание выпадающего списка для выбора семейства
            ComboBox comboBoxFamilyTypes = new ComboBox();
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

        public HashSet<ElementId> GetIntersectionsWithElements(List<Element> elements, Document doc)
        {
            HashSet<ElementId> intersections = new HashSet<ElementId>();

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
                    intersections.Add(intersectedElement.Id);
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
    }
}
