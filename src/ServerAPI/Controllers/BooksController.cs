using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Model;

namespace ServerAPI.Controllers
{
    public class BooksController : ODataController
    {
        private readonly BookStoreContext _db;

        public BooksController(BookStoreContext context)
        {
            _db = context;
            if (context.Books.Any()){ return;}
            foreach (var book in DataSource.GetBooks())
            {
                context.Books.Add(book);
                context.Presses.Add(book.Press);
            }

            context.SaveChanges();
        }

        [EnableQuery]
        public IActionResult Get(int? key)
        {
            if (key.HasValue)
            {
                return Ok(_db.Books.FirstOrDefault(c => c.Id == key));
            }
            return Ok(_db.Books);
        }

//        [EnableQuery]
//        public IActionResult Get(int key)
//        {
//            return Ok(_db.Books.FirstOrDefault(c => c.Id == key));
//        }

        [EnableQuery]
        public IActionResult Post([FromBody] Book book)
        {
            _db.Books.Add(book);
            _db.SaveChanges();
            return Created(book);
        }

    }
}