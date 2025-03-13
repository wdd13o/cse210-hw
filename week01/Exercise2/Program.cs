using System;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter your grade percentage: ");
        string input = Console.ReadLine();
        int percentage = int.Parse(input);

        string letter = "";
        string sign = "";

        if (percentage >= 90)
        {
            letter = "A";
            if (percentage >= 97)
            {
                sign = "+";
            }
            else if (percentage < 93)
            {
                sign = "-";
            }
        }
        else if (percentage >= 80)
        {
            letter = "B";
            if (percentage % 10 >= 7)
            {
                sign = "+";
            }
            else if (percentage % 10 < 3)
            {
                sign = "-";
            }
        }
        else if (percentage >= 70)
        {
            letter = "C";
            if (percentage % 10 >= 7)
            {
                sign = "+";
            }
            else if (percentage % 10 < 3)
            {
                sign = "-";
            }
        }
        else if (percentage >= 60)
        {
            letter = "D";
            if (percentage % 10 >= 7)
            {
                sign = "+";
            }
            else if (percentage % 10 < 3)
            {
                sign = "-";
            }
        }
        else
        {
            letter = "F";
        }

        // Handle exceptional cases
        if (letter == "A" && sign == "+")
        {
            sign = "";
        }
        if (letter == "F")
        {
            sign = "";
        }

        Console.WriteLine($"Your grade is: {letter}{sign}");

        if (percentage >= 70)
        {
            Console.WriteLine("Congratulations! You passed the course.");
        }
        else
        {
            Console.WriteLine("You did not pass the course. Better luck next time!");
        }
    }
}