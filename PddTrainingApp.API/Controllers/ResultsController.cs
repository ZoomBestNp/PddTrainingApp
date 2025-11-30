using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PddTrainingApp.Models;

namespace PddTrainingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly PddTrainingDbContext _context;

        public ResultsController(PddTrainingDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results
                .Include(r => r.Question)
                .ThenInclude(q => q.Module)
                .Include(r => r.User)
                .ToListAsync();
        }

  
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Question)
                .ThenInclude(q => q.Module)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

       
        [HttpPost]
        public async Task<ActionResult<Result>> CreateResult(ResultCreateRequest request)
        {
            // Проверяем существование пользователя и вопроса
            if (!await _context.Users.AnyAsync(u => u.UserId == request.UserId))
            {
                return BadRequest("Пользователь не существует");
            }

            if (!await _context.Questions.AnyAsync(q => q.QuestionId == request.QuestionId))
            {
                return BadRequest("Вопрос не существует");
            }

            var result = new Result
            {
                UserId = request.UserId,
                QuestionId = request.QuestionId,
                Date = DateTime.Now,
                IsCorrect = request.IsCorrect
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

        
            await _context.Entry(result)
                .Reference(r => r.Question)
                .Query()
                .Include(q => q.Module)
                .LoadAsync();

            await _context.Entry(result)
                .Reference(r => r.User)
                .LoadAsync();

            return CreatedAtAction(nameof(GetResult), new { id = result.ResultId }, result);
        }

    
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, Result result)
        {
            if (id != result.ResultId)
            {
                return BadRequest();
            }

            _context.Entry(result).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id))
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
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

      
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Result>>> GetUserResults(int userId)
        {
            return await _context.Results
                .Where(r => r.UserId == userId)
                .Include(r => r.Question)
                .ThenInclude(q => q.Module)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        private bool ResultExists(int id)
        {
            return _context.Results.Any(e => e.ResultId == id);
        }
    }

    public class ResultCreateRequest
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
    }
}