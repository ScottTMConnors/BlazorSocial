using Ardalis.SmartEnum;

namespace BlazorSocial.Data.Entities;

public class PostType : SmartEnum<PostType>
{
    public static readonly PostType Text = new(nameof(Text), 1);
    public static readonly PostType Image = new(nameof(Image), 2);
    public static readonly PostType Video = new(nameof(Video), 3);
    public static readonly PostType Link = new(nameof(Link), 4);

    private PostType(string name, int value) : base(name, value)
    {
    }
}
