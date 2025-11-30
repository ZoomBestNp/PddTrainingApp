using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PddTrainingApp.Models;

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
                .ToListAsync();
        }


        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestionsByModule(int moduleId)
        {
            return await _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Include(q => q.Module)
                .ToListAsync();
        }


        [HttpGet("random/{count}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetRandomQuestions(int count)
        {
            var questions = await _context.Questions
                .Include(q => q.Module)
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
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }


        [HttpPost]
        public async Task<ActionResult<Question>> CreateQuestion(Question question)
        {
            // Проверка существования модуля
            if (!await _context.Modules.AnyAsync(m => m.ModuleId == question.ModuleId))
            {
                return BadRequest("Указанный модуль не существует");
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionId }, question);
        }

   
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, Question question)
        {
            if (id != question.QuestionId)
            {
                return BadRequest("ID в пути не совпадает с ID вопроса");
            }

    
            if (!await _context.Modules.AnyAsync(m => m.ModuleId == question.ModuleId))
            {
                return BadRequest("Указанный модуль не существует");
            }

            _context.Entry(question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

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
}