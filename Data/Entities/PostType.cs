using System.ComponentModel.DataAnnotations;

namespace BlazorSocial.Data.Entities {
    public class PostType {
        [Key]
        public int Id { get; set; }
        public string TypeName { get; set; }
    }
}
