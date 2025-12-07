using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PddTrainingApp.Models;
using System.Linq;

namespace PddTrainingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly PddTrainingDbContext _context;

        public QuestionsController(PddTrainingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions
                .Include(q => q.Module)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .Include(q => q.CorrectOption)
                .ToListAsync();
        }

        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestionsByModule(int moduleId)
        {
            return await _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Include(q => q.Module)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .Include(q => q.CorrectOption)
                .ToListAsync();
        }

        [HttpGet("random/{count}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetRandomQuestions(int count)
        {
            var questions = await _context.Questions
                .Include(q => q.Module)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .Include(q => q.CorrectOption)
                .OrderBy(q => EF.Functions.Random())
                .Take(count)
                .ToListAsync();

            return questions;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Module)
                .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                .Include(q => q.CorrectOption)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPost]
        public async Task<ActionResult<Question>> CreateQuestion(QuestionCreateRequest request)
        {
            // Проверка существования модуля
            if (!await _context.Modules.AnyAsync(m => m.ModuleId == request.ModuleId))
            {
                return BadRequest("Указанный модуль не существует");
            }

            // Создание вопроса
            var question = new Question
            {
                ModuleId = request.ModuleId,
                Content = request.Content,
                DifficultyLevel = request.DifficultyLevel
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync(); // Получаем QuestionId

            // Добавление вариантов ответов
            Option correctOption = null;
            for (int i = 0; i < request.Options.Count; i++)
            {
                var option = new Option
                {
                    QuestionId = question.QuestionId,
                    OptionText = request.Options[i],
                    OptionOrder = i + 1
                };

                _context.Options.Add(option);
                await _context.SaveChangesAsync(); // Получаем OptionId

                if (i == request.CorrectAnswerIndex)
                {
                    correctOption = option;
                }
            }

            // Установка правильного ответа
            if (correctOption != null)
            {
                question.Answer = correctOption.OptionId;
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionId }, question);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, QuestionUpdateRequest request)
        {
            if (id != request.QuestionId)
            {
                return BadRequest("ID в пути не совпадает с ID вопроса");
            }

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            // Проверка существования модуля
            if (!await _context.Modules.AnyAsync(m => m.ModuleId == request.ModuleId))
            {
                return BadRequest("Указанный модуль не существует");
            }

            // Обновление основных полей
            question.ModuleId = request.ModuleId;
            question.Content = request.Content;
            question.DifficultyLevel = request.DifficultyLevel;

            // Обновление вариантов ответов
            _context.Options.RemoveRange(question.Options);

            Option correctOption = null;
            for (int i = 0; i < request.Options.Count; i++)
            {
                var option = new Option
                {
                    QuestionId = question.QuestionId,
                    OptionText = request.Options[i],
                    OptionOrder = i + 1
                };

                _context.Options.Add(option);
                await _context.SaveChangesAsync();

                if (i == request.CorrectAnswerIndex)
                {
                    correctOption = option;
                }
            }

            // Установка правильного ответа
            if (correctOption != null)
            {
                question.Answer = correctOption.OptionId;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }

    public class QuestionCreateRequest
    {
        public int ModuleId { get; set; }
        public string Content { get; set; } = null!;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswerIndex { get; set; }
        public int DifficultyLevel { get; set; } = 1;
    }

    public class QuestionUpdateRequest
    {
        public int QuestionId { get; set; }
        public int ModuleId { get; set; }
        public string Content { get; set; } = null!;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswerIndex { get; set; }
        public int DifficultyLevel { get; set; } = 1;
    }
}