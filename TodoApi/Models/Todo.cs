using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class Todo
    {

        [Key]

        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        //public bool IsCompleted { get; set; }
    }
}
