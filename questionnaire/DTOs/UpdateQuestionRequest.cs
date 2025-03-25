namespace questionnaire.DTOs
{
    public class UpdateQuestionRequest
    {
        public string Text { get; set; }
        public int? QuestionTypeId { get; set; }
        public List<string> Options { get; set; }
    }
}
