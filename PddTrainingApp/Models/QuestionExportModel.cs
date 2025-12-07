using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PddTrainingApp.Models
{
    public class QuestionExportModel
    {
        public int QuestionId { get; set; }
        public string ModuleName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int DifficultyLevel { get; set; }
        public List<OptionExportModel> Options { get; set; } = new List<OptionExportModel>();
        public int CorrectAnswerIndex { get; set; }
        public string ExportDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class OptionExportModel
    {
        public string OptionText { get; set; } = null!;
        public int OptionOrder { get; set; }
    }
}