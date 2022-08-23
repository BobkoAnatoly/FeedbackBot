using System.ComponentModel.DataAnnotations;

namespace Model.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string Text { get; set; }
        [Range(1,5)]
        public int Rating { get; set; }
        public DateTime CreationDate { get; set; }

        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
    }
}
