using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Questionnaire
{
    public int Id { get; set; }

    public int TypeQuestionnaireId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsPublished { get; set; }

    public virtual ICollection<QuestionnaireHistory> QuestionnaireHistories { get; set; } = new List<QuestionnaireHistory>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual TypeQuestionnaire TypeQuestionnaire { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
