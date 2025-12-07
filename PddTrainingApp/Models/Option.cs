using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class Option
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public int OptionOrder { get; set; }

    public virtual Question Question { get; set; } = null!;
}