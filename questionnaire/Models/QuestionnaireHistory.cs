using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class QuestionnaireHistory
{
    public int Id { get; set; }

    public int QuestionnaireId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CompletedAt { get; set; }

    public virtual Questionnaire Questionnaire { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
