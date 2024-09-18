using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PersonsController(AppDbContext context)
    {
        _context = context;
    }

    // a. GET: api/persons?firstName=abc&lastName=def
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetPersons([FromQuery] string firstName, [FromQuery] string lastName)
    {
        var query = _context.Persons.AsQueryable();

        if (!string.IsNullOrEmpty(firstName))
        {
            query = query.Where(p => p.FirstName.ToLower().StartsWith(firstName.ToLower()) || p.FirstName.ToLower().EndsWith(firstName.ToLower()));
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            query = query.Where(p => p.LastName.ToLower().StartsWith(lastName.ToLower()) || p.LastName.ToLower().EndsWith(lastName.ToLower()));
        }

        return await query.ToListAsync();
    }


    // b. GET: api/persons/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetPerson(Guid id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
            return NotFound();

        return person;
    }

    // c. POST: api/persons
    [HttpPost]
    public async Task<ActionResult<Person>> PostPerson(Person person)
    {
        if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
            return BadRequest("FirstName and LastName cannot be empty.");

        person.Id = Guid.NewGuid();
        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
    }

    // d. PUT: api/persons/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPerson(Guid id, Person person)
    {
        if (id != person.Id)
            return BadRequest();

        _context.Entry(person).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Persons.Any(p => p.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // e. DELETE: api/persons/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePerson(Guid id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
            return NotFound();

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
