using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Question
{
    public int Id { get; set; }

    public int QuestionTypeId { get; set; }

    public int QuestionnaireId { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual QuestionType QuestionType { get; set; } = null!;

    public virtual Questionnaire Questionnaire { get; set; } = null!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>(); // Варианты ответов

}
