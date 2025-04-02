using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MindfulnessApp
{
    // Base Activity Class
    public abstract class Activity
    {
        private string _name;
        private string _description;
        private int _duration;
        public static ActivityLogger _logger = new ActivityLogger();
        
        protected Activity(string name, string description)
        {
            _name = name;
            _description = description;
        }
        
        public void Start()
        {
            DisplayStartingMessage();
            SetDuration();
            PrepareToBegin();
            RunActivity();
            DisplayEndingMessage();
            _logger.LogActivity(_name);
        }
        
        protected abstract void RunActivity();
        
        private void DisplayStartingMessage()
        {
            Console.Clear();
            AnimateTitle($"Welcome to the {_name} Activity", 100);
            Console.WriteLine(_description);
        }
        
        private void SetDuration()
        {
            Console.Write("\nHow long, in seconds, would you like for your session? ");
            while (!int.TryParse(Console.ReadLine(), out _duration) || _duration <= 0)
            {
                Console.Write("Please enter a positive number: ");
            }
        }
        
        private void PrepareToBegin()
        {
            Console.Write("\nGet ready to begin");
            AnimateEllipsis(3);
            Console.WriteLine();
        }
        
        protected void DisplayEndingMessage()
        {
            Console.Write("\nGood job!");
            AnimateEllipsis(2);
            Console.WriteLine($"\nYou have completed the {_name} activity for {_duration} seconds.");
            ShowSpinner(3);
        }
        
        protected void ShowSpinner(int seconds, string message = "")
        {
            Console.Write(message);
            string[] spinner = { "|", "/", "-", "\\" };
            for (int i = 0; i < seconds; i++)
            {
                foreach (var s in spinner)
                {
                    Console.Write(s);
                    Thread.Sleep(250);
                    Console.Write("\b \b");
                }
            }
            Console.WriteLine();
        }
        
        protected void ShowCountdown(int seconds, string prefix = "")
        {
            Console.Write(prefix);
            for (int i = seconds; i > 0; i--)
            {
                Console.Write(i);
                Thread.Sleep(1000);
                if (i > 1)
                {
                    Console.Write("\b \b");
                }
            }
            Console.WriteLine();
        }
        
        protected void AnimateEllipsis(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(".");
                    Thread.Sleep(333);
                }
                Console.Write("\b\b\b   \b\b\b");
            }
            for (int j = 0; j < 3; j++)
            {
                Console.Write(".");
                Thread.Sleep(333);
            }
        }
        
        protected void AnimateTitle(string text, int delay)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        
        protected void BreatheAnimation(int seconds, bool breatheIn)
        {
            string text = breatheIn ? "Breathe in..." : "Breathe out...";
            int maxSize = 20;
            
            Console.Write("\n" + text);
            for (int i = 1; i <= seconds; i++)
            {
                int size = (int)(maxSize * Math.Sin((Math.PI/2) * i/seconds));
                string bar = new string('=', size);
                string formattedBar = bar.PadRight(maxSize);
                Console.Write($" [{formattedBar}] {i}/{seconds}s");
                Thread.Sleep(1000);
                Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                Console.Write(text);
            }
            Console.WriteLine();
        }
        
        protected int GetDuration()
        {
            return _duration;
        }
    }

    // Breathing Activity
    public class BreathingActivity : Activity
    {
        public BreathingActivity() 
            : base("Breathing", "This activity will help you relax by walking you through breathing in and out slowly.\nClear your mind and focus on your breathing.")
        {
        }
        
        protected override void RunActivity()
        {
            int duration = GetDuration();
            int breathDuration = 4; // seconds per breath phase
            int cycles = 0;
            
            while (duration > 0)
            {
                BreatheAnimation(Math.Min(breathDuration, duration), true);
                duration -= breathDuration;
                
                if (duration <= 0) break;
                
                BreatheAnimation(Math.Min(breathDuration, duration), false);
                duration -= breathDuration;
                cycles++;
            }
            
            Console.WriteLine($"\nCompleted {cycles} full breathing cycles.");
        }
    }

    // Reflection Activity
    public class ReflectionActivity : Activity
    {
        private List<string> _prompts = new List<string>
        {
            "Think of a time when you stood up for someone else.",
            "Think of a time when you did something really difficult.",
            "Think of a time when you helped someone in need.",
            "Think of a time when you did something truly selfless."
        };
        
        private Queue<string> _questions = new Queue<string>();
        private Random _random = new Random();
        
        public ReflectionActivity()
            : base("Reflection", "This activity will help you reflect on times in your life when you have shown strength and resilience.\nThis will help you recognize the power you have and how you can use it in other aspects of your life.")
        {
            ResetQuestions();
        }
        
        private void ResetQuestions()
        {
            List<string> questions = new List<string>
            {
                "Why was this experience meaningful to you?",
                "Have you ever done anything like this before?",
                "How did you get started?",
                "How did you feel when it was complete?",
                "What made this time different than other times when you were not as successful?",
                "What is your favorite thing about this experience?",
                "What could you learn from this experience that applies to other situations?",
                "What did you learn about yourself through this experience?",
                "How can you keep this experience in mind in the future?"
            };
            
            // Shuffle questions
            while (questions.Count > 0)
            {
                int index = _random.Next(questions.Count);
                _questions.Enqueue(questions[index]);
                questions.RemoveAt(index);
            }
        }
        
        protected override void RunActivity()
        {
            string prompt = _prompts[_random.Next(_prompts.Count)];
            Console.WriteLine($"\n{prompt}");
            
            Console.WriteLine("\nWhen you have something in mind, press enter to continue.");
            Console.ReadLine();
            
            Console.Write("Now ponder on each of the following questions as they relate to this experience.\nYou may begin in: ");
            ShowCountdown(5);
            
            int duration = GetDuration();
            DateTime endTime = DateTime.Now.AddSeconds(duration);
            
            while (DateTime.Now < endTime)
            {
                if (_questions.Count == 0)
                {
                    ResetQuestions();
                }
                
                string question = _questions.Dequeue();
                Console.Write($"\n{question} ");
                ShowSpinner(5);
            }
        }
    }

    // Listing Activity
    public class ListingActivity : Activity
    {
        private List<string> _prompts = new List<string>
        {
            "Who are people that you appreciate?",
            "What are personal strengths of yours?",
            "Who are people that you have helped this week?",
            "When have you felt the Holy Ghost this month?",
            "Who are some of your personal heroes?"
        };
        
        public ListingActivity()
            : base("Listing", "This activity will help you reflect on the good things in your life by having you list\nas many things as you can in a certain area.")
        {
        }
        
        protected override void RunActivity()
        {
            Random random = new Random();
            string prompt = _prompts[random.Next(_prompts.Count)];
            Console.WriteLine($"\n{prompt}");
            
            Console.Write("\nYou may begin in: ");
            ShowCountdown(5);
            
            List<string> items = new List<string>();
            DateTime endTime = DateTime.Now.AddSeconds(GetDuration());
            
            Console.WriteLine("\nStart listing items (press enter after each item):");
            while (DateTime.Now < endTime)
            {
                Console.Write("> ");
                string item = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(item))
                {
                    items.Add(item);
                    Console.WriteLine($"\x1b[1A\x1b[2K> {item}"); // Rewrite the line for better visibility
                }
                
                // Show time remaining
                TimeSpan remaining = endTime - DateTime.Now;
                Console.WriteLine($"Time remaining: {remaining.Seconds} seconds");
            }
            
            Console.WriteLine($"\nYou listed {items.Count} items!");
            
            if (items.Count > 0)
            {
                Console.WriteLine("\nHere's what you listed:");
                foreach (var item in items)
                {
                    Console.WriteLine($"- {item}");
                }
            }
        }
    }

    // Gratitude Activity (Additional Activity)
    public class GratitudeActivity : Activity
    {
        public GratitudeActivity()
            : base("Gratitude", "This activity will help you cultivate gratitude by focusing on the positive aspects\nof your life and appreciating what you have.")
        {
        }
        
        protected override void RunActivity()
        {
            int duration = GetDuration();
            DateTime endTime = DateTime.Now.AddSeconds(duration);
            
            Console.WriteLine("\nLet's take a moment to appreciate the good things in your life.");
            Console.Write("Prepare to begin");
            AnimateEllipsis(3);
            
            List<string> items = new List<string>();
            int secondsPerItem = 10;
            
            while (DateTime.Now < endTime)
            {
                Console.Write("\nThink of something you're grateful for: ");
                string item = Console.ReadLine();
                
                if (!string.IsNullOrWhiteSpace(item))
                {
                    items.Add(item);
                    Console.WriteLine($"\nTake {secondsPerItem} seconds to appreciate '{item}'");
                    ShowSpinner(secondsPerItem, "Appreciating... ");
                }
                
                TimeSpan remaining = endTime - DateTime.Now;
                Console.WriteLine($"\nTime remaining: {remaining.Seconds} seconds");
            }
            
            Console.WriteLine($"\nYou expressed gratitude for {items.Count} things!");
            
            if (items.Count > 0)
            {
                Console.WriteLine("\nYour gratitude list:");
                foreach (var item in items)
                {
                    Console.WriteLine($"- I'm grateful for {item}");
                }
            }
        }
    }

    // Activity Logger
    public class ActivityLogger
    {
        private const string LogFile = "mindfulness_log.csv";
        private Dictionary<string, int> _activityStats = new Dictionary<string, int>();
        
        public ActivityLogger()
        {
            LoadLog();
        }
        
        public void LogActivity(string activityName)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(LogFile, $"{timestamp},{activityName}\n");
            
            if (_activityStats.ContainsKey(activityName))
            {
                _activityStats[activityName]++;
            }
            else
            {
                _activityStats[activityName] = 1;
            }
        }
        
        public void DisplayLog()
        {
            Console.Clear();
            AnimateTitle("Activity History", 50);
            
            if (!File.Exists(LogFile) || new FileInfo(LogFile).Length == 0)
            {
                Console.WriteLine("No activities logged yet.");
                return;
            }
            
            Console.WriteLine("\nYour Mindfulness Journey:\n");
            
            string[] lines = File.ReadAllLines(LogFile);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                Console.WriteLine($"{parts[0]} - {parts[1]}");
            }
            
            Console.WriteLine("\nStatistics:");
            foreach (var stat in _activityStats)
            {
                Console.WriteLine($"- {stat.Key}: {stat.Value} times");
            }
            
            Console.WriteLine($"\nTotal sessions: {lines.Length}");
        }
        
        private void LoadLog()
        {
            if (File.Exists(LogFile))
            {
                string[] lines = File.ReadAllLines(LogFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    string activity = parts[1];
                    
                    if (_activityStats.ContainsKey(activity))
                    {
                        _activityStats[activity]++;
                    }
                    else
                    {
                        _activityStats[activity] = 1;
                    }
                }
            }
        }
        
        private void AnimateTitle(string text, int delay)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    // Main Program
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Mindfulness Program";
            Console.ForegroundColor = ConsoleColor.White;
            
            // Exceeding Requirements Features:
            // 1. Added Gratitude activity as a 4th activity type
            // 2. Enhanced animations throughout (spinners, countdowns, breathing visualization)
            // 3. Added comprehensive activity logging with statistics
            // 4. Colorful console output
            // 5. Time remaining indicators during activities
            // 6. Better input validation
            // 7. Shows lists of items entered during listing activities
            // 8. Ensures all reflection questions are used before repeating
            // 9. Visual breathing animation with progress bar
            
            while (true)
            {
                Console.Clear();
                DisplayMenu();
                
                string choice = Console.ReadLine();
                Activity activity = null;
                
                switch (choice)
                {
                    case "1":
                        activity = new BreathingActivity();
                        break;
                    case "2":
                        activity = new ReflectionActivity();
                        break;
                    case "3":
                        activity = new ListingActivity();
                        break;
                    case "4":
                        activity = new GratitudeActivity();
                        break;
                    case "5":
                        Activity._logger.DisplayLog();
                        Console.WriteLine("\nPress enter to continue...");
                        Console.ReadLine();
                        continue;
                    case "6":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press enter to try again.");
                        Console.ReadLine();
                        continue;
                }
                
                activity.Start();
            }
        }
        
        static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"  __  __ _       _  _  _  _    ___   __   __ _  ____  ____  ____ ");
            Console.WriteLine(@" (  )(  ( \ ___ ( \/ )( \/ )  / __) / _\ (  ( \(  __)(  _ \/ ___)");
            Console.WriteLine(@"  )( /    /(___)/ \/ \ )  /   \__ \/    \/    / ) _)  )   /\___ \");
            Console.WriteLine(@" (__)\_)__)     \_)(_/(__/    (___/\_/\_/\_)__)(____)(__\_)(____/");
            Console.ResetColor();
            
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. Breathing Activity");
            Console.WriteLine("2. Reflection Activity");
            Console.WriteLine("3. Listing Activity");
            Console.WriteLine("4. Gratitude Activity (New!)");
            Console.WriteLine("5. View Activity Log");
            Console.WriteLine("6. Exit");
            Console.Write("\nSelect an option: ");
        }
    }
}