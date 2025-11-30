using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class Result
{
    public int ResultId { get; set; }

    public int UserId { get; set; }

    public int QuestionId { get; set; }

    public DateTime Date { get; set; }

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
