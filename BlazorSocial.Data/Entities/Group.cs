using System.ComponentModel.DataAnnotations;
using BlazorSocial.Data;

namespace BlazorSocial.Data.Entities {
    public class Group : BaseEntity<GroupId> {
        [StringLength(50)]
        public string GroupName { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
