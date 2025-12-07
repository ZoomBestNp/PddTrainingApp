using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PddTrainingApp.Models;
using System.Text.Json;

namespace PddTrainingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportImportController : ControllerBase
    {
        private readonly PddTrainingDbContext _context;

        public ExportImportController(PddTrainingDbContext context)
        {
            _context = context;
        }

        [HttpGet("export/question/{id}")]
        public async Task<IActionResult> ExportQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Module)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .Include(q => q.CorrectOption)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                return NotFound("Вопрос не найден");

            var exportModel = new QuestionExportModel
            {
                QuestionId = question.QuestionId,
                ModuleName = question.Module?.Name ?? "Неизвестный модуль",
                Content = question.Content,
                DifficultyLevel = question.DifficultyLevel,
                Options = question.Options.Select(o => new OptionExportModel
                {
                    OptionText = o.OptionText,
                    OptionOrder = o.OptionOrder
                }).ToList(),
                CorrectAnswerIndex = question.Options
                    .OrderBy(o => o.OptionOrder)
                    .ToList()
                    .FindIndex(o => o.OptionId == question.Answer)
            };

            var json = JsonSerializer.Serialize(exportModel, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return Ok(exportModel);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportQuestion([FromBody] QuestionExportModel importModel)
        {
            if (string.IsNullOrWhiteSpace(importModel.Content))
                return BadRequest("Текст вопроса обязателен");

            if (importModel.Options == null || importModel.Options.Count < 2)
                return BadRequest("Необходимо минимум 2 варианта ответа");

            if (importModel.CorrectAnswerIndex < 0 || importModel.CorrectAnswerIndex >= importModel.Options.Count)
                return BadRequest("Некорректный индекс правильного ответа");

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Name == importModel.ModuleName);

            if (module == null)
            {
                module = new Module
                {
                    Name = importModel.ModuleName,
                    Description = $"Импортированный модуль: {importModel.ModuleName}"
                };
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
            }

            var question = new Question
            {
                ModuleId = module.ModuleId,
                Content = importModel.Content,
                DifficultyLevel = importModel.DifficultyLevel
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            Option correctOption = null;
            for (int i = 0; i < importModel.Options.Count; i++)
            {
                var option = new Option
                {
                    QuestionId = question.QuestionId,
                    OptionText = importModel.Options[i].OptionText,
                    OptionOrder = importModel.Options[i].OptionOrder
                };

                _context.Options.Add(option);
                await _context.SaveChangesAsync();

                if (i == importModel.CorrectAnswerIndex)
                {
                    correctOption = option;
                }
            }

            if (correctOption != null)
            {
                question.Answer = correctOption.OptionId;
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(ExportQuestion), new { id = question.QuestionId }, question);
        }

        [HttpGet("export/module/{moduleId}")]
        public async Task<IActionResult> ExportModuleQuestions(int moduleId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            if (module == null)
                return NotFound("Модуль не найден");

            var questions = await _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .ToListAsync();

            var exportData = new
            {
                ModuleId = module.ModuleId,
                ModuleName = module.Name,
                ModuleDescription = module.Description,
                ExportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                QuestionsCount = questions.Count,
                Questions = questions.Select(q => new
                {
                    q.QuestionId,
                    q.Content,
                    q.DifficultyLevel,
                    Options = q.Options.Select(o => new
                    {
                        o.OptionText,
                        o.OptionOrder
                    }).ToList(),
                    CorrectAnswerIndex = q.Options
                        .OrderBy(o => o.OptionOrder)
                        .ToList()
                        .FindIndex(o => o.OptionId == q.Answer)
                }).ToList()
            };

            return Ok(exportData);
        }
    }
}