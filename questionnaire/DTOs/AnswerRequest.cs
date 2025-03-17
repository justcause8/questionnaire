namespace questionnaire.DTOs
{
    public class AnswerRequest
    {
        public string? AnswerText { get; set; } // Текстовый ответ (для открытых вопросов)

        public int? AnswerClose { get; set; } // Выбранный вариант ответа (для закрытых вопросов)

        public List<int>? AnswerMultiple { get; set; } // Выбранные варианты ответа (для множественного выбора)

        public int? AnswerScale { get; set; } // Значение шкалы (для шкальных вопросов)
    }
}
