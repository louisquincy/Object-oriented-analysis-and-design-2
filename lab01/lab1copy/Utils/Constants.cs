namespace ExamResultBuilder.Utils
{
    public static class Constants
    {
        // Названия тем (используются в чекбоксах и заголовках)
        public static readonly string[] TopicNames = new[]
        {
            "Тема 1: Моделирование полёта и теплопроводность",
            "Тема 2: Случайные числа и события",
            "Тема 3: Системы массового обслуживания"
        };

        // Сообщения
        public const string MsgNoOverdue = "Нет просроченных работ — студент получает автомат!";
        public const string MsgFillStudentData = "Заполните фамилию, имя и группу.";
        public const string MsgSaveSuccess = "Результат сохранён!";
        public const string MsgCommentSaved = "Комментарий сохранён.";
        public const string MsgSelectSession = "Сначала выберите сессию.";

        // Заголовки форм
        public const string TitleMain = "Экзаменационный калькулятор";
        public const string TitleHistory = "История студентов";
        public const string TitleQuestionEditor = "Редактор вопросов";

        // Подписи кнопок и полей
        public const string BtnCalculate = "Улучшение оценки";
        public const string BtnExamTicket = "Экзаменационный билет";
        public const string BtnSaveResult = "Сохранить результат";
        public const string BtnHistory = "История";
        public const string BtnEditQuestions = "Управление вопросами";
    }
}