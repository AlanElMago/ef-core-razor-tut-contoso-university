using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(SchoolContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Student> Students { get;set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            // using System;
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Student> studentsIQ = from s in _context.Students select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                // Remove ToUpper when working with SQL Server
                studentsIQ = studentsIQ.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper())
                    || s.FirstMidName.ToUpper().Contains(searchString.ToUpper()));
            }

            studentsIQ = sortOrder switch 
            {
                "name_desc" => studentsIQ.OrderByDescending(s => s.LastName),
                "Date"      => studentsIQ.OrderBy(s => s.EnrollmentDate),
                "date_desc" => studentsIQ.OrderByDescending(s => s.EnrollmentDate),
                _           => studentsIQ.OrderBy(s => s.LastName)
            };

            var pageSize = _configuration.GetValue("PageSize", 4);
            Students = await PaginatedList<Student>.CreateAsync(studentsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        }
    }
}
