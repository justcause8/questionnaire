using questionnaire.Models;

public partial class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public int Order { get; set; }

    // Навигационное свойство
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Question Question { get; set; } = null!;
}