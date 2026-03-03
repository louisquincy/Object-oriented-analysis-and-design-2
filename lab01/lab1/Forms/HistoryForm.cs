// === HistoryForm.cs ===
#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ExamResultBuilder.Models;
using ExamResultBuilder.Data;

namespace ExamResultBuilder
{
    public partial class HistoryForm : Form
    {
        private List<Student> _students;
        private TextBox txtSearch;
        private ListBox lstStudents;
        private ListBox lstSessions;
        private ListBox lstQuestions;
        private TextBox txtComment;
        private Button btnSaveComment;
        private Label lblSelectedSessionInfo;

        public HistoryForm(List<Student> students)
        {
            _students = students;
            InitializeComponent();
            LoadStudents();
            this.WindowState = FormWindowState.Maximized; // полноэкранный режим
        }

        private void InitializeComponent()
        {
            this.txtSearch = new TextBox();
            this.lstStudents = new ListBox();
            this.lstSessions = new ListBox();
            this.lstQuestions = new ListBox();
            this.txtComment = new TextBox();
            this.btnSaveComment = new Button();
            this.lblSelectedSessionInfo = new Label();
            this.SuspendLayout();

            // txtSearch
            this.txtSearch.Location = new Point(12, 12);
            this.txtSearch.Size = new Size(200, 20);
            this.txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.txtSearch.TextChanged += new EventHandler(this.txtSearch_TextChanged);

            // lstStudents
            this.lstStudents.Location = new Point(12, 40);
            this.lstStudents.Size = new Size(200, 250);
            this.lstStudents.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.lstStudents.DisplayMember = "FullName";
            this.lstStudents.SelectedIndexChanged += new EventHandler(this.lstStudents_SelectedIndexChanged);

            // lstSessions
            this.lstSessions.Location = new Point(220, 12);
            this.lstSessions.Size = new Size(150, 250);
            this.lstSessions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.lstSessions.DisplayMember = "Date";
            this.lstSessions.SelectedIndexChanged += new EventHandler(this.lstSessions_SelectedIndexChanged);

            // lstQuestions
            this.lstQuestions.Location = new Point(380, 12);
            this.lstQuestions.Size = new Size(300, 150);
            this.lstQuestions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.lstQuestions.DisplayMember = "Text";

            // txtComment
            this.txtComment.Location = new Point(380, 170);
            this.txtComment.Size = new Size(300, 80);
            this.txtComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.txtComment.Multiline = true;
            this.txtComment.ReadOnly = false; // разрешаем редактирование

            // btnSaveComment
            this.btnSaveComment.Location = new Point(380, 260);
            this.btnSaveComment.Size = new Size(150, 30);
            this.btnSaveComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnSaveComment.Text = "Сохранить комментарий";
            this.btnSaveComment.Click += new EventHandler(this.btnSaveComment_Click);

            // lblSelectedSessionInfo
            this.lblSelectedSessionInfo.AutoSize = true;
            this.lblSelectedSessionInfo.Location = new Point(220, 270);
            this.lblSelectedSessionInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // HistoryForm
            this.ClientSize = new Size(694, 341);
            this.Controls.Add(this.lblSelectedSessionInfo);
            this.Controls.Add(this.btnSaveComment);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.lstQuestions);
            this.Controls.Add(this.lstSessions);
            this.Controls.Add(this.lstStudents);
            this.Controls.Add(this.txtSearch);
            this.Name = "HistoryForm";
            this.Text = "История студентов";
            this.MinimumSize = new Size(600, 350);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadStudents()
        {
            lstStudents.DataSource = null;
            lstStudents.DataSource = _students;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string filter = txtSearch.Text.ToLower();
            var filtered = _students.Where(s =>
                s.LastName.ToLower().Contains(filter) ||
                s.FirstName.ToLower().Contains(filter) ||
                s.Group.ToLower().Contains(filter) ||
                s.FullName.ToLower().Contains(filter)).ToList();

            lstStudents.DataSource = null;
            lstStudents.DataSource = filtered;
        }

        private void lstStudents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstStudents.SelectedItem is Student student)
            {
                lstSessions.DataSource = null;
                lstSessions.DataSource = student.Sessions;
                lstQuestions.DataSource = null;
                txtComment.Text = "";
                lblSelectedSessionInfo.Text = "";
            }
        }

        private void lstSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSessions.SelectedItem is ExamSession session)
            {
                var bindingSource = new BindingSource();
                bindingSource.DataSource = session.Questions;
                lstQuestions.DataSource = bindingSource;
                lstQuestions.DisplayMember = "Text";
                txtComment.Text = session.TeacherComment;
                lblSelectedSessionInfo.Text = $"Дата: {session.Date:g}";
            }
        }

        private void btnSaveComment_Click(object sender, EventArgs e)
        {
            if (lstSessions.SelectedItem is ExamSession session)
            {
                session.TeacherComment = txtComment.Text;
                DataStorage.SaveStudents(_students);
                MessageBox.Show("Комментарий сохранён.");
            }
            else
            {
                MessageBox.Show("Сначала выберите сессию.");
            }
        }
    }
}