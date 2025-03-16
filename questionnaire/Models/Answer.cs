using questionnaire.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int? AnonymousId { get; set; } // Nullable для анонимных пользователей

    public int? UserId { get; set; } // Nullable для авторизованных пользователей

    public int QuestionId { get; set; }

    public string? Text { get; set; }

    public int? SelectOption { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Anonymou? Anonymous { get; set; } // Nullable

    public virtual User? User { get; set; } // Nullable

    public virtual Question Question { get; set; } = null!;

    // Навигационное свойство для варианта ответа
    public virtual QuestionOption? QuestionOption { get; set; }
}