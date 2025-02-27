using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int AnonymousId { get; set; }

    public int QuestionId { get; set; }

    public string Text { get; set; } = null!;

    public int? SelectOption { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Anonymou Anonymous { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
