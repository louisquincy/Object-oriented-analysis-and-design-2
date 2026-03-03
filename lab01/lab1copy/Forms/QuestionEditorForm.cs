#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ExamResultBuilder.Models;
using ExamResultBuilder.Data;
using ExamResultBuilder.Utils;

namespace ExamResultBuilder
{
    public class QuestionEditorForm : Form
    {
        private ComboBox cmbTopic;
        private ListBox lstQuestions;
        private TextBox txtQuestion;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSave;
        private Button btnCancel;

        private List<Question> _allQuestions;
        private Question _selectedQuestion;

        public QuestionEditorForm()
        {
            InitializeComponent();
            LoadQuestions();
            LoadTopics();
        }

        private void InitializeComponent()
        {
            this.Text = Constants.TitleQuestionEditor;
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Основной макет: одна строка, две колонки
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 1;

            // Колонки: 55% и 45%
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            // Строка занимает 100%
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // === Левая ячейка (список вопросов) ===
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Padding = new Padding(10);

            Label lblTopic = new Label() { Text = "Тема:", AutoSize = true, Location = new Point(0, 0) };
            cmbTopic = new ComboBox() { Location = new Point(60, -2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTopic.SelectedIndexChanged += CmbTopic_SelectedIndexChanged;

            lstQuestions = new ListBox();
            lstQuestions.Location = new Point(0, 30);
            lstQuestions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstQuestions.DisplayMember = "Text";
            lstQuestions.SelectedIndexChanged += LstQuestions_SelectedIndexChanged;

            leftPanel.Controls.Add(lblTopic);
            leftPanel.Controls.Add(cmbTopic);
            leftPanel.Controls.Add(lstQuestions);

            // === Правая ячейка (редактирование и кнопки) ===
            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Padding = new Padding(10);

            // Вертикальный TableLayoutPanel для правой части
            TableLayoutPanel rightLayout = new TableLayoutPanel();
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.ColumnCount = 1;
            rightLayout.RowCount = 3;

            // Строки: первая для поля ввода (авторазмер), вторая для кнопок (авторазмер), третья тоже для кнопок (авторазмер)
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Поле ввода
            Label lblQuestion = new Label() { Text = "Вопрос:", AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 5) };
            txtQuestion = new TextBox();
            txtQuestion.Dock = DockStyle.Top;
            txtQuestion.Height = 60;
            txtQuestion.Multiline = true;

            // Панель для кнопок "Добавить/Изменить/Удалить"
            FlowLayoutPanel buttonPanel1 = new FlowLayoutPanel();
            buttonPanel1.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel1.Dock = DockStyle.Top;
            buttonPanel1.WrapContents = false;
            buttonPanel1.Margin = new Padding(0, 10, 0, 10);

            btnAdd = new Button() { Text = "Добавить", Width = 90, Height = 30, Margin = new Padding(0, 0, 5, 0) };
            btnEdit = new Button() { Text = "Изменить", Width = 90, Height = 30, Margin = new Padding(0, 0, 5, 0) };
            btnDelete = new Button() { Text = "Удалить", Width = 90, Height = 30, Margin = new Padding(0, 0, 5, 0) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            buttonPanel1.Controls.Add(btnAdd);
            buttonPanel1.Controls.Add(btnEdit);
            buttonPanel1.Controls.Add(btnDelete);

            // Панель для кнопок "Сохранить все" и "Отмена"
            FlowLayoutPanel buttonPanel2 = new FlowLayoutPanel();
            buttonPanel2.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel2.Dock = DockStyle.Top;
            buttonPanel2.WrapContents = false;

            btnSave = new Button() { Text = "Сохранить все", Width = 120, Height = 30, Margin = new Padding(0, 0, 5, 0) };
            btnCancel = new Button() { Text = "Отмена", Width = 90, Height = 30 };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            buttonPanel2.Controls.Add(btnSave);
            buttonPanel2.Controls.Add(btnCancel);

            // Добавляем всё в rightLayout
            rightLayout.Controls.Add(lblQuestion, 0, 0);
            rightLayout.Controls.Add(txtQuestion, 0, 0); // нужно аккуратно разместить в разные строки
            rightLayout.Controls.Add(buttonPanel1, 0, 1);
            rightLayout.Controls.Add(buttonPanel2, 0, 2);

            // Внимание: чтобы элементы были в разных строках, нужно правильно задать индексы строк.
            // Сейчас добавление в коллекцию идёт по порядку: сначала lblQuestion (row 0), потом txtQuestion (row 0) — они перекроются.
            // Нужно использовать SetRow для явного указания строки. Либо лучше создать отдельные панели для каждой строки, но проще использовать SetRow.

            // Очистим коллекцию и добавим с явными строками:
            rightLayout.Controls.Clear();

            // Строка 0: поле ввода (Label + TextBox)
            // Можно объединить Label и TextBox в одной ячейке, но проще сделать для Label отдельную строку, а TextBox - следующую.
            // Но так как у нас 3 строки, то:
            // строка 0: Label
            // строка 1: TextBox
            // строка 2: buttonPanel1
            // строка 3: buttonPanel2 — но у нас всего 3 строки, поэтому надо добавить ещё одну.

            // Переделаем: добавим 4 строки:
            rightLayout.RowCount = 4;
            rightLayout.RowStyles.Clear();
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // buttonPanel1
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // buttonPanel2

            rightLayout.Controls.Add(lblQuestion, 0, 0);
            rightLayout.Controls.Add(txtQuestion, 0, 1);
            rightLayout.Controls.Add(buttonPanel1, 0, 2);
            rightLayout.Controls.Add(buttonPanel2, 0, 3);

            rightPanel.Controls.Add(rightLayout);

            // Добавляем левую и правую панели в mainLayout
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainLayout);
        }

        // Далее идут методы LoadQuestions, LoadTopics, обработчики событий (без изменений)
        private void LoadQuestions()
        {
            _allQuestions = QuestionStorage.LoadQuestions();
        }

        private void LoadTopics()
        {
            var topics = _allQuestions.Select(q => q.TopicNumber).Distinct().OrderBy(t => t).ToList();
            cmbTopic.Items.Clear();
            foreach (var t in topics)
            {
                cmbTopic.Items.Add($"Тема {t}");
            }
            if (cmbTopic.Items.Count > 0) cmbTopic.SelectedIndex = 0;
        }

        private void CmbTopic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTopic.SelectedIndex < 0) return;
            int topicNumber = cmbTopic.SelectedIndex + 1;
            var questions = _allQuestions.Where(q => q.TopicNumber == topicNumber).ToList();
            lstQuestions.DataSource = null;
            lstQuestions.DataSource = questions;
            lstQuestions.DisplayMember = "Text";
            txtQuestion.Text = "";
            _selectedQuestion = null;
        }

        private void LstQuestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedQuestion = lstQuestions.SelectedItem as Question;
            if (_selectedQuestion != null)
            {
                txtQuestion.Text = _selectedQuestion.Text;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbTopic.SelectedIndex < 0) return;
            if (string.IsNullOrWhiteSpace(txtQuestion.Text)) return;

            int topicNumber = cmbTopic.SelectedIndex + 1;
            int newId = _allQuestions.Any() ? _allQuestions.Max(q => q.Id) + 1 : 1;
            var newQuestion = new Question(newId, txtQuestion.Text.Trim(), topicNumber);
            _allQuestions.Add(newQuestion);
            RefreshList();
            txtQuestion.Text = "";
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedQuestion == null) return;
            if (string.IsNullOrWhiteSpace(txtQuestion.Text)) return;

            _selectedQuestion.Text = txtQuestion.Text.Trim();
            RefreshList();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedQuestion == null) return;
            _allQuestions.Remove(_selectedQuestion);
            RefreshList();
            txtQuestion.Text = "";
            _selectedQuestion = null;
        }

        private void RefreshList()
        {
            if (cmbTopic.SelectedIndex < 0) return;
            int topicNumber = cmbTopic.SelectedIndex + 1;
            var questions = _allQuestions.Where(q => q.TopicNumber == topicNumber).ToList();
            lstQuestions.DataSource = null;
            lstQuestions.DataSource = questions;
            lstQuestions.DisplayMember = "Text";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            QuestionStorage.SaveQuestions(_allQuestions);
            MessageBox.Show("Вопросы сохранены.");
        }
    }
}