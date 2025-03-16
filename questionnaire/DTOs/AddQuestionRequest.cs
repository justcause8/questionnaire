namespace questionnaire.DTOs
{
    public class AddQuestionRequest
    {
        public string Text { get; set; } // Текст вопроса
        public int QuestionType { get; set; } // Тип вопроса
        public List<string>? Options { get; set; } // Варианты ответов (для закрытых и множественных вопросов)
    }
}