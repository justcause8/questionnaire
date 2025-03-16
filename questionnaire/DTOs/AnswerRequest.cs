namespace questionnaire.DTOs
{
    public class AnswerRequest
    {
        public string? AnswerText { get; set; } // Текстовый ответ (для открытых вопросов)

        public int? SelectedOption { get; set; } // Выбранный вариант ответа (для закрытых вопросов)

        public List<int>? SelectedOptions { get; set; } // Выбранные варианты ответа (для множественного выбора)

        public int? ScaleValue { get; set; } // Значение шкалы (для шкальных вопросов)
    }
}
