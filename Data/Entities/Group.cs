using System.ComponentModel.DataAnnotations;

namespace BlazorSocial.Data.Entities {
    public class Group {
        [Key]
        public string Id { get; set; }
        [StringLength(50)]
        public string GroupName { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
