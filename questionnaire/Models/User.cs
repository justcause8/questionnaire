using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class User
{
    public int Id { get; set; }

    public int AccessLevelId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual AccessLevel AccessLevel { get; set; } = null!;

    public virtual ICollection<QuestionnaireHistory> QuestionnaireHistories { get; set; } = new List<QuestionnaireHistory>();

    public virtual ICollection<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
