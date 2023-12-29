using BlazorSocial.Data.Entities;

namespace BlazorSocial.Data.ComponentObjects {
    public class ViewPostObject : Post {
        public string AuthorName { get; set; }
        public string DateString { get; set; }
    }
}
