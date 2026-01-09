using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TodoApi.Models
{

    [Index(nameof(Email), IsUnique = true)]
    public class UsersModel
    {

        [Key]

        public int Id { get; set; }
        [StringLength(100)]
        public string? FirstName { get; set; }
        [StringLength(100)]
        public string? LastName { get; set; }
        [StringLength(200)]
        [Required]
        public string Email { get; set; } = null!;
        [StringLength(256)]
        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = null!;

    
    }
}
