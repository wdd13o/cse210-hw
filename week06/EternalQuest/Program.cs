using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

namespace EternalQuestPro
{
    // Base Goal class with additional properties
    public abstract class Goal
    {
        protected string _name;
        protected string _description;
        protected int _points;
        protected bool _isComplete;
        protected DateTime _creationDate;
        protected DateTime? _completionDate;
        protected DifficultyLevel _difficulty;
        protected List<string> _tags;

        public Goal(string name, string description, int points, DifficultyLevel difficulty, List<string> tags)
        {
            _name = name;
            _description = description;
            _points = points;
            _isComplete = false;
            _creationDate = DateTime.Now;
            _difficulty = difficulty;
            _tags = tags ?? new List<string>();
        }

        public abstract void RecordProgress();
        public abstract int GetPoints();
        public abstract string GetProgress();
        public abstract string GetGoalType();

        public virtual string GetDisplayString()
        {
            string completionStatus = _isComplete ? "[✓]" : "[ ]";
            string difficultyStars = new string('★', (int)_difficulty);
            string tags = _tags.Count > 0 ? $"Tags: {string.Join(", ", _tags)}" : "";
            return $"{completionStatus} {_name} ({_description}) {difficultyStars}\n   {tags}\n   {GetProgress()}";
        }

        public virtual string GetSaveString()
        {
            return $"{GetGoalType()}|{_name}|{_description}|{_points}|{_isComplete}|{(int)_difficulty}|{string.Join(",", _tags)}|{_creationDate:o}|{_completionDate?.ToString("o")}";
        }

        public bool IsComplete => _isComplete;
        public string Name => _name;
        public DateTime CreationDate => _creationDate;
        public DifficultyLevel Difficulty => _difficulty;
        public List<string> Tags => _tags;

        public virtual void Complete()
        {
            _isComplete = true;
            _completionDate = DateTime.Now;
        }

        public TimeSpan TimeToComplete => 
            _completionDate.HasValue ? _completionDate.Value - _creationDate : TimeSpan.Zero;
    }

    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Epic = 4,
        Legendary = 5
    }

    // Simple Goal (one-time completion)
    public class SimpleGoal : Goal
    {
        public SimpleGoal(string name, string description, int points, DifficultyLevel difficulty, List<string> tags) 
            : base(name, description, points, difficulty, tags) { }

        public override void RecordProgress()
        {
            if (!_isComplete)
            {
                Complete();
            }
        }

        public override int GetPoints()
        {
            return _isComplete ? _points * (int)_difficulty : 0;
        }

        public override string GetProgress()
        {
            return _isComplete ? $"Completed on {_completionDate:d} (Took {TimeToComplete.Days} days)" : "Not completed";
        }

        public override string GetGoalType() => "SimpleGoal";
    }

    // Eternal Goal (never complete, points each time)
    public class EternalGoal : Goal
    {
        private int _timesCompleted;
        private DateTime _lastCompleted;

        public EternalGoal(string name, string description, int points, DifficultyLevel difficulty, List<string> tags) 
            : base(name, description, points, difficulty, tags)
        {
            _timesCompleted = 0;
        }

        public override void RecordProgress()
        {
            _timesCompleted++;
            _lastCompleted = DateTime.Now;
        }

        public override int GetPoints()
        {
            // Streak bonus - more points for consistent completion
            int streakBonus = HasStreak() ? 50 : 0;
            return (_points * (int)_difficulty) + streakBonus;
        }

        private bool HasStreak()
        {
            // Check if completed at least 3 days in a row
            if (_timesCompleted < 3) return false;
            
            // This would need actual streak tracking in a real implementation
            return true;
        }

        public override string GetProgress()
        {
            string lastCompleted = _timesCompleted > 0 ? $"Last: {_lastCompleted:d}" : "Never completed";
            return $"Completed {_timesCompleted} times. {lastCompleted}";
        }

        public override string GetGoalType() => "EternalGoal";

        public override string GetSaveString()
        {
            return base.GetSaveString() + $"|{_timesCompleted}|{_lastCompleted:o}";
        }
    }

    // Checklist Goal (complete a certain number of times)
    public class ChecklistGoal : Goal
    {
        private int _timesCompleted;
        private int _targetCount;
        private int _bonusPoints;
        private List<DateTime> _completionDates;

        public ChecklistGoal(string name, string description, int points, int targetCount, int bonusPoints, 
                           DifficultyLevel difficulty, List<string> tags) 
            : base(name, description, points, difficulty, tags)
        {
            _targetCount = targetCount;
            _bonusPoints = bonusPoints;
            _timesCompleted = 0;
            _completionDates = new List<DateTime>();
        }

        public override void RecordProgress()
        {
            _timesCompleted++;
            _completionDates.Add(DateTime.Now);
            
            if (_timesCompleted >= _targetCount)
            {
                Complete();
            }
        }

        public override int GetPoints()
        {
            int basePoints = _timesCompleted * (_points * (int)_difficulty);
            if (_isComplete)
            {
                basePoints += _bonusPoints * (int)_difficulty;
            }
            return basePoints;
        }

        public override string GetProgress()
        {
            string progress = $"Completed {_timesCompleted}/{_targetCount} times";
            if (_completionDates.Any())
            {
                DateTime last = _completionDates.Last();
                progress += $". Last: {last:d}";
                
                if (_timesCompleted > 1)
                {
                    double avgDays = _completionDates.Zip(_completionDates.Skip(1), 
                                      (a, b) => (b - a).TotalDays).Average();
                    progress += $". Avg: {avgDays:F1} days between";
                }
            }
            return progress;
        }

        public override string GetGoalType() => "ChecklistGoal";

        public override string GetDisplayString()
        {
            string baseDisplay = base.GetDisplayString();
            
            // Show progress bar for checklist goals
            double percent = (double)_timesCompleted / _targetCount;
            int bars = (int)(percent * 20);
            string progressBar = $"[{new string('=', bars)}{new string(' ', 20 - bars)}] {percent:P0}";
            
            return $"{baseDisplay}\n   Progress: {progressBar}";
        }

        public override string GetSaveString()
        {
            string dates = string.Join(",", _completionDates.Select(d => d.ToString("o")));
            return base.GetSaveString() + $"|{_timesCompleted}|{_targetCount}|{_bonusPoints}|{dates}";
        }
    }

    // Negative Goal (lose points for bad habits)
    public class NegativeGoal : Goal
    {
        private int _timesRecorded;
        
        public NegativeGoal(string name, string description, int points, DifficultyLevel difficulty, List<string> tags) 
            : base(name, description, points, difficulty, tags)
        {
            _timesRecorded = 0;
        }

        public override void RecordProgress()
        {
            _timesRecorded++;
        }

        public override int GetPoints()
        {
            return -(_points * (int)_difficulty * _timesRecorded);
        }

        public override string GetProgress()
        {
            return $"Avoided {_timesRecorded} times. Each occurrence loses {_points * (int)_difficulty} points";
        }

        public override string GetGoalType() => "NegativeGoal";

        public override string GetDisplayString()
        {
            return $"[⚠] {_name} ({_description}) {new string('★', (int)_difficulty)}\n   " +
                   $"Tags: {string.Join(", ", _tags)}\n   {GetProgress()}";
        }

        public override string GetSaveString()
        {
            return base.GetSaveString() + $"|{_timesRecorded}";
        }
    }

    // New: Progress Goal (track progress toward a large goal)
    public class ProgressGoal : Goal
    {
        private int _currentProgress;
        private int _targetProgress;
        private string _progressUnit;

        public ProgressGoal(string name, string description, int points, int targetProgress, 
                           string progressUnit, DifficultyLevel difficulty, List<string> tags)
            : base(name, description, points, difficulty, tags)
        {
            _currentProgress = 0;
            _targetProgress = targetProgress;
            _progressUnit = progressUnit;
        }

        public override void RecordProgress()
        {
            Console.Write($"Enter progress in {_progressUnit} (current: {_currentProgress}, target: {_targetProgress}): ");
            if (int.TryParse(Console.ReadLine(), out int progress))
            {
                _currentProgress += progress;
                if (_currentProgress >= _targetProgress)
                {
                    Complete();
                }
            }
        }

        public override int GetPoints()
        {
            // Points proportional to progress made
            double progressRatio = (double)_currentProgress / _targetProgress;
            return (int)(_points * progressRatio * (int)_difficulty);
        }

        public override string GetProgress()
        {
            return $"Progress: {_currentProgress}/{_targetProgress} {_progressUnit} ({(_currentProgress/(double)_targetProgress):P0})";
        }

        public override string GetGoalType() => "ProgressGoal";

        public override string GetDisplayString()
        {
            string baseDisplay = base.GetDisplayString();
            
            // Visual progress indicator
            double percent = (double)_currentProgress / _targetProgress;
            int bars = (int)(percent * 30);
            string progressBar = $"[{new string('=', bars)}{new string(' ', 30 - bars)}]";
            
            return $"{baseDisplay}\n   {progressBar} {percent:P0}";
        }

        public override string GetSaveString()
        {
            return base.GetSaveString() + $"|{_currentProgress}|{_targetProgress}|{_progressUnit}";
        }
    }

    // Goal Manager with enhanced features
    public class GoalManager
    {
        private List<Goal> _goals;
        private int _score;
        private int _level;
        private int _experience;
        private List<string> _achievements;
        private Dictionary<string, int> _pointHistory;
        private Dictionary<string, int> _categoryPoints;
        private Dictionary<DateTime, int> _dailyProgress;
        private List<Friend> _friends;
        private List<Reward> _rewards;
        private List<Reward> _availableRewards;
        private int _streakDays;
        private DateTime _lastActivityDate;

        public GoalManager()
        {
            _goals = new List<Goal>();
            _score = 0;
            _level = 1;
            _experience = 0;
            _achievements = new List<string>();
            _pointHistory = new Dictionary<string, int>();
            _categoryPoints = new Dictionary<string, int>();
            _dailyProgress = new Dictionary<DateTime, int>();
            _friends = new List<Friend>();
            _rewards = new List<Reward>();
            _availableRewards = GetDefaultRewards();
            _streakDays = 0;
            _lastActivityDate = DateTime.MinValue;
        }

        public void AddGoal(Goal goal)
        {
            _goals.Add(goal);
            
            // Update categories
            foreach (var tag in goal.Tags)
            {
                if (!_categoryPoints.ContainsKey(tag))
                {
                    _categoryPoints[tag] = 0;
                }
            }
            
            CheckForNewbieAchievement();
        }

        public void RecordGoalProgress(int index)
        {
            if (index >= 0 && index < _goals.Count)
            {
                var goal = _goals[index];
                goal.RecordProgress();
                
                UpdateStreak();
                
                int pointsEarned = goal.GetPoints();
                _score += pointsEarned;
                _experience += Math.Abs(pointsEarned);
                
                // Record daily progress
                DateTime today = DateTime.Today;
                if (!_dailyProgress.ContainsKey(today))
                {
                    _dailyProgress[today] = 0;
                }
                _dailyProgress[today] += pointsEarned;
                
                // Update category points
                foreach (var tag in goal.Tags)
                {
                    _categoryPoints[tag] += pointsEarned;
                }
                
                // Add to history
                _pointHistory[goal.Name] = pointsEarned;

                CheckLevelUp();
                CheckAchievements();
                CheckForRewards();

                Console.WriteLine($"\nYou earned {pointsEarned} points!");
                
                if (goal.IsComplete)
                {
                    Console.WriteLine($"Congratulations! You completed the goal: {goal.Name}");
                    CheckCompletionAchievements();
                }
            }
        }

        private void UpdateStreak()
        {
            DateTime today = DateTime.Today;
            if (_lastActivityDate.Date == today.AddDays(-1).Date)
            {
                _streakDays++;
            }
            else if (_lastActivityDate.Date != today.Date)
            {
                // Reset streak if not consecutive
                _streakDays = 1;
            }
            _lastActivityDate = today;
            
            if (_streakDays % 7 == 0)
            {
                Console.WriteLine($"\n🔥 {_streakDays}-day streak! Keep it up!");
            }
        }

        public void DisplayGoals()
        {
            Console.WriteLine("\nYour Goals:");
            
            if (!_goals.Any())
            {
                Console.WriteLine("No goals yet. Create some to get started!");
                return;
            }
            
            // Group by completion status
            var incompleteGoals = _goals.Where(g => !g.IsComplete).ToList();
            var completeGoals = _goals.Where(g => g.IsComplete).ToList();
            
            Console.WriteLine("\n=== Active Goals ===");
            for (int i = 0; i < incompleteGoals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {incompleteGoals[i].GetDisplayString()}");
            }
            
            Console.WriteLine("\n=== Completed Goals ===");
            for (int i = 0; i < completeGoals.Count; i++)
            {
                Console.WriteLine($"{i + incompleteGoals.Count + 1}. {completeGoals[i].GetDisplayString()}");
            }
        }

        public void DisplayScore()
        {
            Console.WriteLine($"\nCurrent Score: {_score} points");
            Console.WriteLine($"Level: {_level} (EXP: {_experience}/{GetExpForNextLevel()})");
            Console.WriteLine($"Current Streak: {_streakDays} days");
            
            if (_achievements.Count > 0)
            {
                Console.WriteLine("\n🏆 Achievements Earned:");
                foreach (var achievement in _achievements)
                {
                    Console.WriteLine($"- {achievement}");
                }
            }
            
            if (_rewards.Count > 0)
            {
                Console.WriteLine("\n🎁 Rewards Claimed:");
                foreach (var reward in _rewards)
                {
                    Console.WriteLine($"- {reward.Name} (Cost: {reward.Cost})");
                }
            }
            
            DisplayCategoryProgress();
        }

        private void DisplayCategoryProgress()
        {
            if (_categoryPoints.Any())
            {
                Console.WriteLine("\n📊 Category Progress:");
                foreach (var category in _categoryPoints.OrderByDescending(c => c.Value))
                {
                    Console.WriteLine($"- {category.Key}: {category.Value} points");
                }
            }
        }

        public void DisplayStatistics()
        {
            Console.WriteLine("\n📈 Your Statistics:");
            
            // Basic stats
            int completedCount = _goals.Count(g => g.IsComplete);
            double completionRate = _goals.Count > 0 ? (double)completedCount / _goals.Count : 0;
            Console.WriteLine($"Goals: {_goals.Count} (Completed: {completedCount}, Rate: {completionRate:P0})");
            
            // Points by goal type
            Console.WriteLine("\nPoints by Goal Type:");
            foreach (var group in _goals.GroupBy(g => g.GetGoalType()))
            {
                Console.WriteLine($"- {group.Key}: {group.Sum(g => g.GetPoints())} points");
            }
            
            // Recent activity
            if (_dailyProgress.Any())
            {
                Console.WriteLine("\n📅 Recent Activity:");
                var lastWeek = _dailyProgress.Where(d => d.Key > DateTime.Today.AddDays(-7))
                                             .OrderBy(d => d.Key);
                foreach (var day in lastWeek)
                {
                    Console.WriteLine($"- {day.Key:d}: {day.Value} points");
                }
            }
        }

        public void SaveGoals(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(_score);
                writer.WriteLine(_level);
                writer.WriteLine(_experience);
                writer.WriteLine(_streakDays);
                writer.WriteLine(_lastActivityDate.ToString("o"));
                
                // Save achievements
                writer.WriteLine(string.Join("|", _achievements));
                
                // Save rewards
                writer.WriteLine(string.Join("|", _rewards.Select(r => $"{r.Name},{r.Cost}")));
                
                // Save goals
                foreach (var goal in _goals)
                {
                    writer.WriteLine(goal.GetSaveString());
                }
            }
        }

        public void LoadGoals(string filename)
        {
            if (File.Exists(filename))
            {
                _goals.Clear();
                string[] lines = File.ReadAllLines(filename);
                
                _score = int.Parse(lines[0]);
                _level = int.Parse(lines[1]);
                _experience = int.Parse(lines[2]);
                _streakDays = int.Parse(lines[3]);
                _lastActivityDate = DateTime.Parse(lines[4], null, DateTimeStyles.RoundtripKind);
                
                // Load achievements
                _achievements = lines[5].Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                // Load rewards
                _rewards = lines[6].Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(r => r.Split(','))
                                  .Select(p => new Reward(p[0], int.Parse(p[1])))
                                  .ToList();
                
                // Load goals
                for (int i = 7; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split('|');
                    string goalType = parts[0];
                    string name = parts[1];
                    string description = parts[2];
                    int points = int.Parse(parts[3]);
                    bool isComplete = bool.Parse(parts[4]);
                    DifficultyLevel difficulty = (DifficultyLevel)int.Parse(parts[5]);
                    List<string> tags = parts[6].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    DateTime creationDate = DateTime.Parse(parts[7], null, DateTimeStyles.RoundtripKind);
                    DateTime? completionDate = !string.IsNullOrEmpty(parts[8]) ? 
                        DateTime.Parse(parts[8], null, DateTimeStyles.RoundtripKind) : (DateTime?)null;

                    Goal goal = null;
                    switch (goalType)
                    {
                        case "SimpleGoal":
                            goal = new SimpleGoal(name, description, points, difficulty, tags);
                            break;
                        case "EternalGoal":
                            goal = new EternalGoal(name, description, points, difficulty, tags);
                            int timesCompleted = int.Parse(parts[9]);
                            for (int j = 0; j < timesCompleted; j++) ((EternalGoal)goal).RecordProgress();
                            break;
                        case "ChecklistGoal":
                            int timesDone = int.Parse(parts[9]);
                            int targetCount = int.Parse(parts[10]);
                            int bonusPoints = int.Parse(parts[11]);
                            goal = new ChecklistGoal(name, description, points, targetCount, bonusPoints, difficulty, tags);
                            if (!string.IsNullOrEmpty(parts[12]))
                            {
                                var dates = parts[12].Split(',')
                                                   .Select(d => DateTime.Parse(d, null, DateTimeStyles.RoundtripKind));
                                foreach (var date in dates) ((ChecklistGoal)goal).RecordProgress();
                            }
                            break;
                        case "NegativeGoal":
                            goal = new NegativeGoal(name, description, points, difficulty, tags);
                            int timesRecorded = int.Parse(parts[9]);
                            for (int j = 0; j < timesRecorded; j++) ((NegativeGoal)goal).RecordProgress();
                            break;
                        case "ProgressGoal":
                            int currentProgress = int.Parse(parts[9]);
                            int targetProgress = int.Parse(parts[10]);
                            string progressUnit = parts[11];
                            goal = new ProgressGoal(name, description, points, targetProgress, progressUnit, difficulty, tags);
                            for (int j = 0; j < currentProgress; j++) ((ProgressGoal)goal).RecordProgress();
                            break;
                    }

                    if (goal != null)
                    {
                        if (isComplete) goal.Complete();
                        _goals.Add(goal);
                    }
                }
            }
        }

        private void CheckLevelUp()
        {
            int expNeeded = GetExpForNextLevel();
            if (_experience >= expNeeded)
            {
                _level++;
                _experience -= expNeeded;
                Console.WriteLine($"\n🎉 LEVEL UP! You are now level {_level}");
                
                // Add level-based rewards
                if (_level % 5 == 0)
                {
                    string rewardName = $"Level {_level} Champion";
                    int rewardCost = _level * 100;
                    _availableRewards.Add(new Reward(rewardName, rewardCost));
                    Console.WriteLine($"New reward unlocked: {rewardName} (Cost: {rewardCost} points)");
                }
            }
        }

        private int GetExpForNextLevel()
        {
            return (int)(1000 * Math.Pow(1.2, _level - 1));
        }

        private void CheckAchievements()
        {
            // Score-based achievements
            CheckAchievement("Point Collector", "Earn 1,000 points", _score >= 1000);
            CheckAchievement("Point Master", "Earn 5,000 points", _score >= 5000);
            CheckAchievement("Point Legend", "Earn 10,000 points", _score >= 10000);
            
            // Goal-based achievements
            int completedGoals = _goals.Count(g => g.IsComplete);
            CheckAchievement("Goal Starter", "Complete 1 goal", completedGoals >= 1);
            CheckAchievement("Goal Achiever", "Complete 5 goals", completedGoals >= 5);
            CheckAchievement("Goal Master", "Complete 10 goals", completedGoals >= 10);
            
            // Streak achievements
            CheckAchievement("Consistent", "3-day streak", _streakDays >= 3);
            CheckAchievement("Dedicated", "7-day streak", _streakDays >= 7);
            CheckAchievement("Unstoppable", "30-day streak", _streakDays >= 30);
            
            // Category achievements
            foreach (var category in _categoryPoints.Keys)
            {
                CheckAchievement($"{category} Enthusiast", $"Earn 500 points in {category}", 
                               _categoryPoints[category] >= 500);
                CheckAchievement($"{category} Expert", $"Earn 2,000 points in {category}", 
                               _categoryPoints[category] >= 2000);
            }
            
            // Difficulty achievements
            CheckAchievement("Challenge Seeker", "Complete a Hard goal", 
                           _goals.Any(g => g.IsComplete && g.Difficulty >= DifficultyLevel.Hard));
            CheckAchievement("Epic Adventurer", "Complete an Epic goal", 
                           _goals.Any(g => g.IsComplete && g.Difficulty >= DifficultyLevel.Epic));
        }

        private void CheckForNewbieAchievement()
        {
            if (_goals.Count == 1 && !_achievements.Contains("Newbie"))
            {
                _achievements.Add("Newbie");
                Console.WriteLine("\n🏆 Achievement Unlocked: Newbie (Created first goal)");
            }
        }

        private void CheckCompletionAchievements()
        {
            var completedToday = _goals.Count(g => g.IsComplete && g.TimeToComplete.TotalDays < 1);
            if (completedToday >= 3 && !_achievements.Contains("Productive Day"))
            {
                _achievements.Add("Productive Day");
                Console.WriteLine("\n🏆 Achievement Unlocked: Productive Day (Completed 3 goals in one day)");
            }
        }

        private void CheckAchievement(string name, string description, bool condition)
        {
            string fullName = $"{name} ({description})";
            if (condition && !_achievements.Contains(fullName))
            {
                _achievements.Add(fullName);
                Console.WriteLine($"\n🏆 Achievement Unlocked: {fullName}");
            }
        }

        private void CheckForRewards()
        {
            foreach (var reward in _availableRewards.Where(r => !_rewards.Contains(r) && _score >= r.Cost).ToList())
            {
                Console.Write($"\n🎁 You can claim a reward: {reward.Name} (Cost: {reward.Cost}). Claim now? (y/n): ");
                if (Console.ReadLine().Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    ClaimReward(reward);
                }
            }
        }

        private void ClaimReward(Reward reward)
        {
            _score -= reward.Cost;
            _rewards.Add(reward);
            Console.WriteLine($"\n🎉 Reward claimed: {reward.Name}! New score: {_score}");
            
            // Special effects for certain rewards
            if (reward.Name.Contains("Boost"))
            {
                _experience += 200;
                Console.WriteLine("+200 EXP Boost applied!");
                CheckLevelUp();
            }
        }

        private List<Reward> GetDefaultRewards()
        {
            return new List<Reward>
            {
                new Reward("Custom Title", 500),
                new Reward("Profile Badge", 1000),
                new Reward("EXP Boost", 1500),
                new Reward("Special Theme", 2000)
            };
        }

        public void ShowRewardShop()
        {
            Console.WriteLine("\n🛒 Reward Shop (Current Points: " + _score + ")");
            Console.WriteLine("Available Rewards:");
            
            int index = 1;
            foreach (var reward in _availableRewards.Where(r => !_rewards.Contains(r)))
            {
                Console.WriteLine($"{index++}. {reward.Name} - {reward.Cost} points");
            }
            
            Console.Write("\nEnter reward number to purchase (or 0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= _availableRewards.Count)
            {
                var selectedReward = _availableRewards[choice - 1];
                if (_score >= selectedReward.Cost)
                {
                    ClaimReward(selectedReward);
                }
                else
                {
                    Console.WriteLine("Not enough points for this reward!");
                }
            }
        }

        public void AddFriend(Friend friend)
        {
            _friends.Add(friend);
            Console.WriteLine($"Added friend: {friend.Name}");
            
            if (_friends.Count >= 3 && !_achievements.Contains("Social Butterfly"))
            {
                _achievements.Add("Social Butterfly");
                Console.WriteLine("\n🏆 Achievement Unlocked: Social Butterfly (Added 3 friends)");
            }
        }

        public void DisplayFriendsProgress()
        {
            if (!_friends.Any())
            {
                Console.WriteLine("\nYou haven't added any friends yet.");
                return;
            }
            
            Console.WriteLine("\n👥 Friends Progress:");
            foreach (var friend in _friends.OrderByDescending(f => f.Score))
            {
                Console.WriteLine($"- {friend.Name}: {friend.Score} points (Level {friend.Level})");
            }
        }
    }

    public class Reward
    {
        public string Name { get; }
        public int Cost { get; }
        
        public Reward(string name, int cost)
        {
            Name = name;
            Cost = cost;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Reward reward && reward.Name == Name;
        }
        
        public override int GetHashCode() => Name.GetHashCode();
    }

    public class Friend
    {
        public string Name { get; }
        public int Score { get; }
        public int Level { get; }
        
        public Friend(string name, int score, int level)
        {
            Name = name;
            Score = score;
            Level = level;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Eternal Quest Pro";
            Console.ForegroundColor = ConsoleColor.White;
            
            GoalManager goalManager = new GoalManager();
            string filename = "goals_save.txt";

            // Load existing goals if available
            if (File.Exists(filename))
            {
                goalManager.LoadGoals(filename);
                Console.WriteLine("Previous progress loaded successfully!");
            }

            while (true)
            {
                Console.WriteLine("\n=== Eternal Quest Pro ===");
                Console.WriteLine("1. Create New Goal");
                Console.WriteLine("2. Record Goal Progress");
                Console.WriteLine("3. View Goals");
                Console.WriteLine("4. View Score & Achievements");
                Console.WriteLine("5. View Statistics");
                Console.WriteLine("6. Reward Shop");
                Console.WriteLine("7. Friends");
                Console.WriteLine("8. Save Progress");
                Console.WriteLine("9. Exit");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateNewGoal(goalManager);
                        break;
                    case "2":
                        RecordProgress(goalManager);
                        break;
                    case "3":
                        goalManager.DisplayGoals();
                        break;
                    case "4":
                        goalManager.DisplayScore();
                        break;
                    case "5":
                        goalManager.DisplayStatistics();
                        break;
                    case "6":
                        goalManager.ShowRewardShop();
                        break;
                    case "7":
                        FriendsMenu(goalManager);
                        break;
                    case "8":
                        goalManager.SaveGoals(filename);
                        Console.WriteLine("Progress saved successfully!");
                        break;
                    case "9":
                        Console.Write("Save before exiting? (y/n): ");
                        if (Console.ReadLine().Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            goalManager.SaveGoals(filename);
                        }
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static void CreateNewGoal(GoalManager goalManager)
        {
            Console.WriteLine("\nSelect Goal Type:");
            Console.WriteLine("1. Simple Goal (one-time)");
            Console.WriteLine("2. Eternal Goal (repeating)");
            Console.WriteLine("3. Checklist Goal (multiple times)");
            Console.WriteLine("4. Negative Goal (avoid)");
            Console.WriteLine("5. Progress Goal (track progress)");
            Console.Write("Enter choice: ");
            string typeChoice = Console.ReadLine();

            Console.Write("Enter goal name: ");
            string name = Console.ReadLine();

            Console.Write("Enter goal description: ");
            string description = Console.ReadLine();

            Console.Write("Enter points: ");
            int points = int.Parse(Console.ReadLine());

            Console.WriteLine("\nSelect difficulty:");
            Console.WriteLine("1. Easy ★");
            Console.WriteLine("2. Medium ★★");
            Console.WriteLine("3. Hard ★★★");
            Console.WriteLine("4. Epic ★★★★");
            Console.WriteLine("5. Legendary ★★★★★");
            Console.Write("Enter choice: ");
            DifficultyLevel difficulty = (DifficultyLevel)int.Parse(Console.ReadLine());

            Console.Write("Enter tags (comma separated): ");
            List<string> tags = Console.ReadLine().Split(',').Select(t => t.Trim()).ToList();

            Goal goal = null;
            switch (typeChoice)
            {
                case "1":
                    goal = new SimpleGoal(name, description, points, difficulty, tags);
                    break;
                case "2":
                    goal = new EternalGoal(name, description, points, difficulty, tags);
                    break;
                case "3":
                    Console.Write("Enter target count: ");
                    int targetCount = int.Parse(Console.ReadLine());
                    Console.Write("Enter bonus points: ");
                    int bonusPoints = int.Parse(Console.ReadLine());
                    goal = new ChecklistGoal(name, description, points, targetCount, bonusPoints, difficulty, tags);
                    break;
                case "4":
                    goal = new NegativeGoal(name, description, points, difficulty, tags);
                    break;
                case "5":
                    Console.Write("Enter target progress (e.g., 100 for 100 miles): ");
                    int targetProgress = int.Parse(Console.ReadLine());
                    Console.Write("Enter progress unit (e.g., miles, pages): ");
                    string progressUnit = Console.ReadLine();
                    goal = new ProgressGoal(name, description, points, targetProgress, progressUnit, difficulty, tags);
                    break;
                default:
                    Console.WriteLine("Invalid goal type.");
                    return;
            }

            goalManager.AddGoal(goal);
            Console.WriteLine("\nGoal added successfully!");
        }

        static void RecordProgress(GoalManager goalManager)
        {
            goalManager.DisplayGoals();
            Console.Write("\nEnter the number of the goal to record: ");
            if (int.TryParse(Console.ReadLine(), out int goalNumber) && goalNumber > 0)
            {
                goalManager.RecordGoalProgress(goalNumber - 1);
            }
            else
            {
                Console.WriteLine("Invalid goal number.");
            }
        }

        static void FriendsMenu(GoalManager goalManager)
        {
            while (true)
            {
                Console.WriteLine("\n👥 Friends Menu");
                Console.WriteLine("1. View Friends Progress");
                Console.WriteLine("2. Add Friend");
                Console.WriteLine("3. Back to Main Menu");
                Console.Write("Select an option: ");
                
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        goalManager.DisplayFriendsProgress();
                        break;
                    case "2":
                        Console.Write("Enter friend's name: ");
                        string name = Console.ReadLine();
                        Console.Write("Enter friend's score: ");
                        int score = int.Parse(Console.ReadLine());
                        Console.Write("Enter friend's level: ");
                        int level = int.Parse(Console.ReadLine());
                        goalManager.AddFriend(new Friend(name, score, level));
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
    }
}