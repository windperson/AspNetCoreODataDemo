using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerAPI.Model;

namespace ServerAPI.Controllers
{
    public class BooksController : ODataController
    {
        private readonly BookStoreContext _db;

        public BooksController(BookStoreContext context)
        {
            _db = context;
            if (context.Books.Any())
            {
                return;
            }

            foreach (var book in DataSource.GetBooks())
            {
                context.Books.Add(book);
                context.Presses.Add(book.Press);
            }

            context.SaveChanges();
        }

        #region OData CRUD

        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] int? key)
        {
            if (key.HasValue)
            {
                return Ok(await _db.Books.FindAsync(key));
            }

            return Ok(_db.Books);
        }

        [EnableQuery]
        public async Task<IActionResult> Post([FromBody] Book book)
        {
            await _db.Books.AddAsync(book);
            await _db.SaveChangesAsync();
            return Created(book);
        }

        [EnableQuery]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<Book> book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _db.Books.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            book.Patch(entity);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _db.Books.AnyAsync(x => x.Id == key))
                {
                    throw;
                }

                return NotFound();
            }

            return Updated(entity);
        }

        [EnableQuery]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] Book updatedBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != updatedBook.Id)
            {
                return BadRequest();
            }

            _db.Entry(updatedBook).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException )
            {
                if (await _db.Books.AnyAsync(x => x.Id == key))
                {
                    throw;
                }

                return NotFound();
            }

            return Updated(updatedBook);
        }

        [EnableQuery]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var target = await _db.FindAsync<Book>(key);
            if (target == null)
            {
                return NotFound();
            }

            _db.Books.Remove(target);
            await _db.SaveChangesAsync();

            return StatusCode((int) HttpStatusCode.NoContent);
        }
        #endregion
    }
}