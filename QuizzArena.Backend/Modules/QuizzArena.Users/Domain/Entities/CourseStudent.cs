using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Domain.Entities
{
    internal class CourseStudent
    {
        public Guid Id { get; set; }
        // FK User
        public Guid StudentId { get; set; }
        // FK Course
        public Guid CourseId { get; set; }
    }
}
