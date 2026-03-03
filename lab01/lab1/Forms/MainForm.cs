// === MainForm.cs ===
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ExamResultBuilder.Models;
using ExamResultBuilder.Builders;
using ExamResultBuilder.Results;
using ExamResultBuilder.Data;
using ExamResultBuilder.Utils;

namespace ExamResultBuilder
{
    public class MainForm : Form
    {
        // Левая панель с полями студента
        private TableLayoutPanel leftPanel;
        private Label lblLastName, lblFirstName, lblGroup;
        private TextBox txtLastName, txtFirstName, txtGroup;

        // DataGridView для лабораторных работ
        private DataGridView dataGridViewLabs;
        private BindingList<LabWork> _labWorks;

        // Панель с чекбоксами тем для быстрой отметки
        private GroupBox topicsGroup;
        private CheckBox chkTopic1, chkTopic2, chkTopic3;

        // Кнопки
        private Button btnCalculate;
        private Button btnExamTicket;
        private Button btnEditQuestions;

        // Панель вопросов
        private Panel questionsPanel;
        private ListBox lstQuestions;
        private BindingSource questionsBindingSource;

        // Нижняя панель с комментарием и кнопками
        private Panel bottomPanel;
        private Label lblComment;
        private TextBox txtComment;
        private Button btnSave;
        private Button btnHistory;

        // Данные
        private List<Student> _students;
        private List<Question> _generatedQuestions;
        private ExamResult _lastExamResult;

        public MainForm()
        {
            InitializeComponent();
            LoadStudentsFromFile();
            this.MinimumSize = new Size(900, 600);
        }

        private void InitializeComponent()
        {
            // ===== Левая панель с полями студента =====
            leftPanel = new TableLayoutPanel();
            leftPanel.ColumnCount = 1;
            leftPanel.RowCount = 6;
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Padding = new Padding(10);

            lblLastName = new Label() { Text = "Фамилия:", Anchor = AnchorStyles.Left, AutoSize = true };
            txtLastName = new TextBox() { Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            lblFirstName = new Label() { Text = "Имя:", Anchor = AnchorStyles.Left, AutoSize = true };
            txtFirstName = new TextBox() { Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            lblGroup = new Label() { Text = "Группа:", Anchor = AnchorStyles.Left, AutoSize = true };
            txtGroup = new TextBox() { Width = 150, Anchor = AnchorStyles.Top | AnchorStyles.Left };

            leftPanel.Controls.Add(lblLastName, 0, 0);
            leftPanel.Controls.Add(txtLastName, 0, 1);
            leftPanel.Controls.Add(lblFirstName, 0, 2);
            leftPanel.Controls.Add(txtFirstName, 0, 3);
            leftPanel.Controls.Add(lblGroup, 0, 4);
            leftPanel.Controls.Add(txtGroup, 0, 5);

            // ===== DataGridView с лабораторными работами =====
            dataGridViewLabs = new DataGridView();
            dataGridViewLabs.Dock = DockStyle.Fill;
            dataGridViewLabs.AutoGenerateColumns = false;
            dataGridViewLabs.AllowUserToAddRows = false;
            dataGridViewLabs.RowHeadersWidth = 30;

            DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
            idCol.DataPropertyName = nameof(LabWork.Id);
            idCol.HeaderText = "ID";
            idCol.Width = 40;

            DataGridViewTextBoxColumn titleCol = new DataGridViewTextBoxColumn();
            titleCol.DataPropertyName = nameof(LabWork.Title);
            titleCol.HeaderText = "Название работы";
            titleCol.Width = 690; // широкая колонка

            DataGridViewTextBoxColumn topicCol = new DataGridViewTextBoxColumn();
            topicCol.DataPropertyName = nameof(LabWork.TopicNumber);
            topicCol.HeaderText = "Тема";
            topicCol.Width = 60;

            DataGridViewCheckBoxColumn overdueCol = new DataGridViewCheckBoxColumn();
            overdueCol.DataPropertyName = nameof(LabWork.IsOverdue);
            overdueCol.HeaderText = "Просрочена";
            overdueCol.Width = 80;

            dataGridViewLabs.Columns.Add(idCol);
            dataGridViewLabs.Columns.Add(titleCol);
            dataGridViewLabs.Columns.Add(topicCol);
            dataGridViewLabs.Columns.Add(overdueCol);

            // Загружаем данные
            LoadSampleLabs();

            // Подписываемся на изменение ячеек, чтобы обновлять состояние чекбоксов тем
            dataGridViewLabs.CellValueChanged += DataGridViewLabs_CellValueChanged;
            dataGridViewLabs.CurrentCellDirtyStateChanged += DataGridViewLabs_CurrentCellDirtyStateChanged;

            // ===== Панель с чекбоксами тем =====
            topicsGroup = new GroupBox();
            topicsGroup.Text = "Быстрая отметка тем";
            topicsGroup.Dock = DockStyle.Fill;
            topicsGroup.Padding = new Padding(10);
            topicsGroup.AutoSize = true;
            topicsGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            chkTopic1 = new CheckBox() { Text = "Тема 1 (работы 1-3)", Location = new Point(10, 20), AutoSize = true };
            chkTopic2 = new CheckBox() { Text = "Тема 2 (работы 4-7)", Location = new Point(10, 45), AutoSize = true };
            chkTopic3 = new CheckBox() { Text = "Тема 3 (работы 8-10)", Location = new Point(10, 70), AutoSize = true };

            // Подписываемся на изменение чекбоксов
            chkTopic1.CheckedChanged += TopicCheckBox_CheckedChanged;
            chkTopic2.CheckedChanged += TopicCheckBox_CheckedChanged;
            chkTopic3.CheckedChanged += TopicCheckBox_CheckedChanged;

            topicsGroup.Controls.Add(chkTopic1);
            topicsGroup.Controls.Add(chkTopic2);
            topicsGroup.Controls.Add(chkTopic3);

            // Горизонтальная панель для кнопок
            TableLayoutPanel buttonPanel = new TableLayoutPanel();
            buttonPanel.ColumnCount = 2;
            buttonPanel.RowCount = 1;
            buttonPanel.AutoSize = true;
            buttonPanel.Location = new Point(10, 100);
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnCalculate = new Button() { Text = "Рассчитать (улучшить)", Width = 140, Height = 30, Anchor = AnchorStyles.None };
            btnExamTicket = new Button() { Text = "Экзаменационный билет", Width = 140, Height = 30, Anchor = AnchorStyles.None };

            btnCalculate.Click += new EventHandler(BtnCalculate_Click);
            btnExamTicket.Click += new EventHandler(BtnExamTicket_Click);

            buttonPanel.Controls.Add(btnCalculate, 0, 0);
            buttonPanel.Controls.Add(btnExamTicket, 1, 0);
            topicsGroup.Controls.Add(buttonPanel);

            // ===== Панель вопросов =====
            questionsPanel = new Panel();
            questionsPanel.Dock = DockStyle.Fill;
            questionsPanel.Padding = new Padding(10);

            lstQuestions = new ListBox();
            lstQuestions.Dock = DockStyle.Fill;
            lstQuestions.DisplayMember = "Text";
            questionsBindingSource = new BindingSource();
            questionsPanel.Controls.Add(lstQuestions);

            // ===== Нижняя панель с комментарием и кнопками =====
            bottomPanel = new Panel();
            bottomPanel.Height = 100;
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Padding = new Padding(10);

            lblComment = new Label() { Text = "Комментарий преподавателя:", AutoSize = true, Location = new Point(10, 10) };
            txtComment = new TextBox() { Multiline = true, Width = 400, Height = 50, Location = new Point(10, 30) };
            btnSave = new Button() { Text = "Сохранить результат", Width = 150, Height = 30, Location = new Point(420, 40) };
            btnHistory = new Button() { Text = "История", Width = 100, Height = 30, Location = new Point(580, 40) };

            btnEditQuestions = new Button() { Text = Constants.BtnEditQuestions, Width = 140, Height = 30, Location = new Point(690, 40) }; // подберите расположение
            btnEditQuestions.Click += BtnEditQuestions_Click;
            bottomPanel.Controls.Add(btnEditQuestions);

            btnSave.Click += new EventHandler(BtnSave_Click);
            btnHistory.Click += new EventHandler(BtnHistory_Click);

            bottomPanel.Controls.Add(lblComment);
            bottomPanel.Controls.Add(txtComment);
            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnHistory);

            // ===== Основной макет =====
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 3;
            mainLayout.RowCount = 2;

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); // левая панель (поля)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));   // таблица
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));   // чекбоксы + кнопки

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 281)); // высокая верхняя часть
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 80));   // нижняя часть (вопросы)

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(dataGridViewLabs, 1, 0);
            mainLayout.Controls.Add(topicsGroup, 2, 0);
            mainLayout.Controls.Add(questionsPanel, 0, 1);
            mainLayout.SetColumnSpan(questionsPanel, 3);

            this.Controls.Add(mainLayout);
            this.Controls.Add(bottomPanel);

            this.Text = "Экзаменационный калькулятор";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized; // полноэкранный режим
        }

        private void BtnEditQuestions_Click(object sender, EventArgs e)
        {
            using (var editor = new QuestionEditorForm())
            {
                editor.ShowDialog();
            }
        }

        // Загрузка примера лабораторных работ
        private void LoadSampleLabs()
        {
            var labs = new List<LabWork>
            {
                new LabWork(1, "1. Моделирование полёта тела в атмосфере", 1, false),
                new LabWork(2, "2. Метод конечных разностей для уравнения теплопроводности", 1, false),
                new LabWork(3, "3. Клеточные автоматы. Лесные пожары (GUI)", 1, false),
                new LabWork(4, "4. Базовый датчик случайных чисел", 2, false),
                new LabWork(5, "5. Моделирование случайных событий (GUI)", 2, false),
                new LabWork(6, "6. Имитационное моделирование дискретных случайных величин (GUI)", 2, false),
                new LabWork(7, "7. Марковская модель погоды", 2, false),
                new LabWork(8, "8. Пуассоновский поток. События на сервере", 3, false),
                new LabWork(9, "9. Система массового обслуживания M/M/1", 3, false),
                new LabWork(10, "10. Система массового обслуживания M/M/* с усложнениями", 3, false)
            };
            _labWorks = new BindingList<LabWork>(labs);
            dataGridViewLabs.DataSource = _labWorks;
        }

        private void LoadStudentsFromFile()
        {
            _students = DataStorage.LoadStudents();
        }

        // Обработка изменения чекбоксов тем (быстрая отметка)
        private void TopicCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;

            int topic = -1;
            if (checkBox == chkTopic1) topic = 1;
            else if (checkBox == chkTopic2) topic = 2;
            else if (checkBox == chkTopic3) topic = 3;

            if (topic == -1) return;

            // Обновляем все лабы этой темы
            foreach (var lab in _labWorks.Where(l => l.TopicNumber == topic))
            {
                lab.IsOverdue = checkBox.Checked;
            }
            // Обновляем отображение
            dataGridViewLabs.Refresh();
        }

        // Обновление состояния чекбоксов тем при ручном изменении в таблице
        private void UpdateTopicCheckBoxes()
        {
            chkTopic1.CheckedChanged -= TopicCheckBox_CheckedChanged;
            chkTopic2.CheckedChanged -= TopicCheckBox_CheckedChanged;
            chkTopic3.CheckedChanged -= TopicCheckBox_CheckedChanged;

            chkTopic1.Checked = _labWorks.Where(l => l.TopicNumber == 1).All(l => l.IsOverdue);
            chkTopic2.Checked = _labWorks.Where(l => l.TopicNumber == 2).All(l => l.IsOverdue);
            chkTopic3.Checked = _labWorks.Where(l => l.TopicNumber == 3).All(l => l.IsOverdue);

            chkTopic1.CheckedChanged += TopicCheckBox_CheckedChanged;
            chkTopic2.CheckedChanged += TopicCheckBox_CheckedChanged;
            chkTopic3.CheckedChanged += TopicCheckBox_CheckedChanged;
        }

        private void DataGridViewLabs_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridViewLabs.Columns["IsOverdue"]?.Index)
            {
                UpdateTopicCheckBoxes();
            }
        }

        private void DataGridViewLabs_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewLabs.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridViewLabs.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Кнопка "Рассчитать (улучшить)"
        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            // Создаём строителя (используем полное имя, чтобы избежать конфликта с пространством имён)
            var builder = new Builders.ExamResultBuilder();

            // Передаём ему лабы по темам
            builder.ProcessTopic(1, _labWorks.Where(l => l.TopicNumber == 1).ToList());
            builder.ProcessTopic(2, _labWorks.Where(l => l.TopicNumber == 2).ToList());
            builder.ProcessTopic(3, _labWorks.Where(l => l.TopicNumber == 3).ToList());

            // Получаем результат
            _lastExamResult = builder.Build();

            if (!_lastExamResult.GoesToExam)
            {
                MessageBox.Show("Нет просроченных работ — студент получает автомат!");
                lstQuestions.DataSource = null;
                _generatedQuestions = null;
                return;
            }

            // Генерируем вопросы по темам, в которых есть просрочки, в количестве _lastExamResult.QuestionCount
            _generatedQuestions = QuestionHelper.GetRandomQuestions(
                _lastExamResult.TopicsForExam,
                _lastExamResult.QuestionCount);

            questionsBindingSource.DataSource = _generatedQuestions;
            lstQuestions.DataSource = questionsBindingSource;
            lstQuestions.DisplayMember = "Text";
        }

        // Кнопка "Экзаменационный билет"
        private void BtnExamTicket_Click(object sender, EventArgs e)
        {
            // Билет: всегда по одному вопросу из каждой темы
            var allTopics = new List<int> { 1, 2, 3 };
            var questions = new List<Question>();

            foreach (int topic in allTopics)
            {
                var q = QuestionHelper.GetRandomQuestions(new List<int> { topic }, 1);
                if (q.Any())
                    questions.Add(q[0]);
                else
                    questions.Add(new Question(0, $"Нет вопросов по теме {topic}", topic));
            }
            _generatedQuestions = questions;
            // Для совместимости создаём ExamResult (но он не используется для билета)
            _lastExamResult = new ExamResult(true, allTopics, 3);

            questionsBindingSource.DataSource = _generatedQuestions;
            lstQuestions.DataSource = questionsBindingSource;
            lstQuestions.DisplayMember = "Text";
        }

        // Сохранение результата
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtGroup.Text))
            {
                MessageBox.Show("Заполните фамилию, имя и группу.");
                return;
            }

            string lastName = txtLastName.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string group = txtGroup.Text.Trim();
            string comment = txtComment.Text.Trim();

            var student = _students.FirstOrDefault(s =>
                s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.Group.Equals(group, StringComparison.OrdinalIgnoreCase));

            if (student == null)
            {
                student = new Student(lastName, firstName, group);
                _students.Add(student);
            }

            var session = new ExamSession
            {
                Date = DateTime.Now,
                Questions = _generatedQuestions?.ToList() ?? new List<Question>(),
                TeacherComment = comment
            };
            student.Sessions.Add(session);

            DataStorage.SaveStudents(_students);
            MessageBox.Show("Результат сохранён!");

            // Очистка полей
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtGroup.Text = "";
            txtComment.Text = "";
            // Сброс отметок о просрочках (по желанию можно оставить)
            foreach (var lab in _labWorks)
                lab.IsOverdue = false;
            dataGridViewLabs.Refresh();
            UpdateTopicCheckBoxes();
            lstQuestions.DataSource = null;
            _generatedQuestions = null;
            _lastExamResult = null;
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            HistoryForm historyForm = new HistoryForm(_students);
            historyForm.ShowDialog();
        }
    }
}