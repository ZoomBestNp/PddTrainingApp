using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class Question
{
    public int QuestionId { get; set; }
    public int ModuleId { get; set; }
    public string Content { get; set; } = null!;
    public int? Answer { get; set; } 
    public int DifficultyLevel { get; set; }

    public virtual Module Module { get; set; } = null!;
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    public virtual Option? CorrectOption { get; set; }
}