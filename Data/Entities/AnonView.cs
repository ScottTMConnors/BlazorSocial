using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    //Identical to view, but without user being signed in
    [PrimaryKey(nameof(PostId), nameof(IPAddress))]
    public class AnonView {
        [ForeignKey("Post")]
        public string PostId { get; set; }
        public DateTime ViewDate { get; set; }
        public string? IPAddress { get; set; }
        public int TimesViewed { get; set; }
        public Post? Post { get; set; }
    }
}
