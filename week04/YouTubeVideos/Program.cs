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
    public int Views { get; private set; }
    public int Likes { get; private set; }
    private List<Comment> Comments { get; set; }

    public Video(string title, string author, int length)
    {
        Title = title;
        Author = author;
        Length = length;
        Views = 0;
        Likes = 0;
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

    public void UpdateViews()
    {
        Views++;
    }

    public void UpdateLikes()
    {
        Likes++;
    }

    public void DisplayDetails()
    {
        Console.WriteLine($"Title: {Title}");
        Console.WriteLine($"Author: {Author}");
        Console.WriteLine($"Length: {Length} seconds");
        Console.WriteLine($"Views: {Views}");
        Console.WriteLine($"Likes: {Likes}");
        Console.WriteLine($"Number of Comments: {GetNumberOfComments()}");
        Console.WriteLine("Comments:");
        foreach (Comment comment in Comments)
        {
            Console.WriteLine($"- {comment.Name}: {comment.Text}");
        }
        Console.WriteLine();
    }
}

class User
{
    public string Username { get; set; }
    public List<Video> UploadedVideos { get; private set; }
    public List<Video> WatchedVideos { get; private set; }

    public User(string username)
    {
        Username = username;
        UploadedVideos = new List<Video>();
        WatchedVideos = new List<Video>();
    }

    public void UploadVideo(Video video)
    {
        UploadedVideos.Add(video);
    }

    public void WatchVideo(Video video)
    {
        WatchedVideos.Add(video);
        video.UpdateViews();
    }

    public void SearchVideo(string title, List<Video> videos)
    {
        foreach (Video video in videos)
        {
            if (video.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                video.DisplayDetails();
            }
        }
    }
}

class YouTubeSystem
{
    private List<Video> Videos { get; set; }

    public YouTubeSystem()
    {
        Videos = new List<Video>();
    }

    public void AddVideo(Video video)
    {
        Videos.Add(video);
    }

    public void SearchVideo(string title)
    {
        foreach (Video video in Videos)
        {
            if (video.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                video.DisplayDetails();
            }
        }
    }

    public void DisplayTrending()
    {
        Console.WriteLine("Trending Videos:");
        foreach (Video video in Videos)
        {
            if (video.Views > 100) // Example threshold for trending
            {
                video.DisplayDetails();
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create a YouTube system
        YouTubeSystem system = new YouTubeSystem();

        // Create users
        User user1 = new User("Bundu");
        User user2 = new User("Jane");

        // Create videos
        Video video1 = new Video("C# Basics", "Bundu Kallon", 600);
        Video video2 = new Video("Advanced C# Techniques", "Jane Smith", 1200);
        Video video3 = new Video("C# Design Patterns", "Alice Johnson", 900);

        // Users upload videos
        user1.UploadVideo(video1);
        user2.UploadVideo(video2);
        user2.UploadVideo(video3);

        // Add videos to the system
        system.AddVideo(video1);
        system.AddVideo(video2);
        system.AddVideo(video3);

        // Users watch videos
        user1.WatchVideo(video2);
        user1.WatchVideo(video3);
        user2.WatchVideo(video1);

        // Add comments to videos
        video1.AddComment(new Comment("User1", "Great explanation!"));
        video1.AddComment(new Comment("User2", "Very helpful, thanks!"));
        video2.AddComment(new Comment("User3", "Loved the examples!"));
        video3.AddComment(new Comment("User4", "Design patterns made easy."));

        // Display all videos
        Console.WriteLine("All Videos:");
        system.SearchVideo(""); // Display all videos

        // Display trending videos
        system.DisplayTrending();
    }
}