using System;
using System.Collections.Generic;
using System.Data;

namespace questionnaire.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? RefreshToken {  get; set; }

    public DateTime? RefreshTokenExpirationTime {  get; set; }

    public virtual ICollection<QuestionnaireHistory> QuestionnaireHistories { get; set; } = new List<QuestionnaireHistory>();

    public virtual ICollection<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();
}
