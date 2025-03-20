using System;
using System.Collections.Generic;

class Comment
{
    public string Name { get; set; }
    public string Text { get; set; }

    public Comment(string name, string text)
    {
        Name = name;
        Text = text;
    }
}

class Video
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Length { get; set; } // Length in seconds
    private List<Comment> Comments { get; set; }

    public Video(string title, string author, int length)
    {
        Title = title;
        Author = author;
        Length = length;
        Comments = new List<Comment>();
    }

    public void AddComment(Comment comment)
    {
        Comments.Add(comment);
    }

    public int GetNumberOfComments()
    {
        return Comments.Count;
    }

    public List<Comment> GetComments()
    {
        return Comments;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create videos
        Video video1 = new Video("C# Basics", "Bundu Kallon", 600);
        Video video2 = new Video("Advanced C# Techniques", "Jane Smith", 1200);
        Video video3 = new Video("C# Design Patterns", "Alice Johnson", 900);

        // Add comments to video1
        video1.AddComment(new Comment("User1", "Great explanation!"));
        video1.AddComment(new Comment("User2", "Very helpful, thanks!"));
        video1.AddComment(new Comment("User3", "Can you make a video on LINQ?"));

        // Add comments to video2
        video2.AddComment(new Comment("User4", "This is exactly what I needed."));
        video2.AddComment(new Comment("User5", "Clear and concise."));
        video2.AddComment(new Comment("User6", "Loved the examples!"));

        // Add comments to video3
        video3.AddComment(new Comment("User7", "Design patterns made easy."));
        video3.AddComment(new Comment("User8", "Thanks for the detailed explanation."));
        video3.AddComment(new Comment("User9", "Looking forward to more videos!"));

        // Store videos in a list
        List<Video> videos = new List<Video> { video1, video2, video3 };

        // Display video details
        foreach (Video video in videos)
        {
            Console.WriteLine($"Title: {video.Title}");
            Console.WriteLine($"Author: {video.Author}");
            Console.WriteLine($"Length: {video.Length} seconds");
            Console.WriteLine($"Number of Comments: {video.GetNumberOfComments()}");
            Console.WriteLine("Comments:");
            foreach (Comment comment in video.GetComments())
            {
                Console.WriteLine($"- {comment.Name}: {comment.Text}");
            }
            Console.WriteLine();
        }
    }
}