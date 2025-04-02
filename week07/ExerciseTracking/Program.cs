using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedExerciseTracking
{
    public enum DistanceUnit { Miles, Kilometers }
    public enum ExerciseIntensity { Low, Moderate, High }

    public abstract class Activity
    {
        private DateTime _date;
        private int _minutes;
        private DistanceUnit _unit;
        private ExerciseIntensity _intensity;
        private static int _activityCount = 0;
        private readonly int _activityId;

        public Activity(DateTime date, int minutes, DistanceUnit unit = DistanceUnit.Miles, 
                       ExerciseIntensity intensity = ExerciseIntensity.Moderate)
        {
            _date = date;
            _minutes = minutes;
            _unit = unit;
            _intensity = intensity;
            _activityId = ++_activityCount;
        }

        public DateTime Date => _date;
        public int Minutes => _minutes;
        public DistanceUnit Unit => _unit;
        public ExerciseIntensity Intensity => _intensity;
        public int ActivityId => _activityId;
        public static int TotalActivities => _activityCount;

        public abstract double GetDistance();
        public abstract double GetSpeed();
        public abstract double GetPace();
        public abstract double GetCaloriesBurned();

        public virtual string GetSummary()
        {
            string unitAbbr = _unit == DistanceUnit.Miles ? "mi" : "km";
            string speedUnit = _unit == DistanceUnit.Miles ? "mph" : "kph";
            string paceUnit = $"min/{unitAbbr}";

            return $"{_date:dd MMM yyyy} {GetType().Name} (ID: {_activityId}, {_minutes} min, Intensity: {_intensity})\n" +
                   $"• Distance: {GetDistance():F1} {unitAbbr}\n" +
                   $"• Speed: {GetSpeed():F1} {speedUnit}\n" +
                   $"• Pace: {GetPace():F1} {paceUnit}\n" +
                   $"• Calories: {GetCaloriesBurned():F0} kcal";
        }

        public string GetShortDescription() => $"{_date:yyyy-MM-dd} {GetType().Name} ({_minutes} min)";

        protected double ConvertToPreferredUnit(double distanceInMiles)
        {
            return _unit == DistanceUnit.Miles ? distanceInMiles : distanceInMiles * 1.60934;
        }
    }

    public class Running : Activity
    {
        private double _distance;
        private double _elevationGain;

        public Running(DateTime date, int minutes, double distance, double elevationGain = 0,
                      DistanceUnit unit = DistanceUnit.Miles, ExerciseIntensity intensity = ExerciseIntensity.Moderate)
            : base(date, minutes, unit, intensity)
        {
            _distance = distance;
            _elevationGain = elevationGain;
        }

        public override double GetDistance() => ConvertToPreferredUnit(_distance);
        public override double GetSpeed() => ConvertToPreferredUnit(_distance) / Minutes * 60;
        public override double GetPace() => Minutes / ConvertToPreferredUnit(_distance);
        public override double GetCaloriesBurned()
        {
            double met = Intensity switch
            {
                ExerciseIntensity.Low => 6.0,
                ExerciseIntensity.Moderate => 8.0,
                ExerciseIntensity.High => 10.0 + (_elevationGain > 500 ? 2.0 : 0)
            };
            return met * 3.5 * (70.0 / 200) * Minutes;
        }

        public override string GetSummary() => 
            base.GetSummary() + $"\n• Elevation Gain: {_elevationGain:F0} ft";
    }

    public class Cycling : Activity
    {
        private double _speed;
        private double _resistance;

        public Cycling(DateTime date, int minutes, double speed, double resistance = 5,
                      DistanceUnit unit = DistanceUnit.Miles, ExerciseIntensity intensity = ExerciseIntensity.Moderate)
            : base(date, minutes, unit, intensity)
        {
            _speed = speed;
            _resistance = Math.Clamp(resistance, 1, 10);
        }

        public override double GetDistance() => ConvertToPreferredUnit((_speed * Minutes) / 60);
        public override double GetSpeed() => ConvertToPreferredUnit(_speed);
        public override double GetPace() => 60 / ConvertToPreferredUnit(_speed);
        public override double GetCaloriesBurned()
        {
            double met = 8.0 + (_resistance / 2) + (int)Intensity;
            return met * 3.5 * (70.0 / 200) * Minutes;
        }

        public override string GetSummary() => 
            base.GetSummary() + $"\n• Resistance Level: {_resistance:F1}/10";
    }

    public class Swimming : Activity
    {
        private int _laps;
        private string _strokeType;
        private const double LapLengthMeters = 50;
        private const double MetersToMiles = 0.000621371;

        public int Laps => _laps;  // Public property to access laps

        public Swimming(DateTime date, int minutes, int laps, string strokeType = "Freestyle",
                       DistanceUnit unit = DistanceUnit.Miles, ExerciseIntensity intensity = ExerciseIntensity.Moderate)
            : base(date, minutes, unit, intensity)
        {
            _laps = laps;
            _strokeType = strokeType;
        }

        private double GetDistanceInMiles() => _laps * LapLengthMeters * MetersToMiles;

        public override double GetDistance() => ConvertToPreferredUnit(GetDistanceInMiles());
        public override double GetSpeed() => ConvertToPreferredUnit(GetDistanceInMiles()) / Minutes * 60;
        public override double GetPace() => Minutes / ConvertToPreferredUnit(GetDistanceInMiles());
        public override double GetCaloriesBurned()
        {
            double strokeFactor = _strokeType switch
            {
                "Freestyle" => 1.0,
                "Breaststroke" => 1.2,
                "Backstroke" => 0.9,
                "Butterfly" => 1.5,
                _ => 1.0
            };
            double met = 7.0 * strokeFactor * (1 + (int)Intensity * 0.3);
            return met * 3.5 * (70.0 / 200) * Minutes;
        }

        public override string GetSummary() => 
            base.GetSummary() + $"\n• Stroke Type: {_strokeType}\n• Laps Completed: {_laps}";
    }

    public class ActivityLogger
    {
        private List<Activity> _activities = new List<Activity>();

        public void AddActivity(Activity activity) => _activities.Add(activity);
        public void RemoveActivity(int activityId) => _activities.RemoveAll(a => a.ActivityId == activityId);
        public IEnumerable<Activity> GetActivities() => _activities.OrderBy(a => a.Date);
        public IEnumerable<Activity> GetActivitiesByType<T>() where T : Activity => _activities.OfType<T>();
        public double GetTotalDistance(DistanceUnit unit)
        {
            return _activities.Sum(a => 
                a.Unit == unit ? a.GetDistance() : 
                unit == DistanceUnit.Miles ? a.GetDistance() / 1.60934 : 
                a.GetDistance() * 1.60934);
        }
        public void DisplayAllActivities()
        {
            Console.WriteLine("\n=== ACTIVITY LOG ===\n");
            foreach (var activity in GetActivities())
            {
                Console.WriteLine(activity.GetSummary());
                Console.WriteLine("----------------------------");
            }
            Console.WriteLine($"\nTotal Activities: {Activity.TotalActivities}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ActivityLogger();

            // Add sample activities
            logger.AddActivity(new Running(
                new DateTime(2023, 6, 15), 
                45, 4.2, 320, 
                DistanceUnit.Miles, 
                ExerciseIntensity.High));

            logger.AddActivity(new Cycling(
                new DateTime(2023, 6, 16), 
                60, 15.5, 7, 
                DistanceUnit.Kilometers, 
                ExerciseIntensity.Moderate));

            logger.AddActivity(new Swimming(
                new DateTime(2023, 6, 17), 
                30, 40, "Butterfly", 
                DistanceUnit.Miles, 
                ExerciseIntensity.High));

            // Display all activities
            logger.DisplayAllActivities();

            // Display statistics
            Console.WriteLine("\n=== STATISTICS ===");
            Console.WriteLine($"Total Running Distance: {logger.GetActivitiesByType<Running>().Sum(r => r.GetDistance()):F1} mi");
            Console.WriteLine($"Total Cycling Distance: {logger.GetActivitiesByType<Cycling>().Sum(c => c.GetDistance()):F1} km");
            Console.WriteLine($"Total Swimming Laps: {logger.GetActivitiesByType<Swimming>().Sum(s => ((Swimming)s).Laps)}");

            // Display activities by intensity
            Console.WriteLine("\n=== HIGH INTENSITY WORKOUTS ===");
            foreach (var activity in logger.GetActivities()
                .Where(a => a.Intensity == ExerciseIntensity.High))
            {
                Console.WriteLine(activity.GetShortDescription());
            }
        }
    }
}