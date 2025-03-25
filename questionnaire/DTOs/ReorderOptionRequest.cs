namespace questionnaire.DTOs
{
    public class ReorderOptionRequest
    {
        public int OptionId { get; set; } // ID варианта ответа
        public int NewOrder { get; set; } // Новый порядковый номер
    }
}
