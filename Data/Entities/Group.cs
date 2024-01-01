using System.ComponentModel.DataAnnotations;

namespace BlazorSocial.Data.Entities {
    public class Group {
        [Key]
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
