using System;
using System.Collections.Generic;
using System.IO;

// Main program class
class Program
{
    // List of prompts for journal entries
    static List<string> prompts = new List<string>
    {
        "Who was the most interesting person I interacted with today?",
        "What was the best part of my day?",
        "How did I see the hand of the Lord in my life today?",
        "What was the strongest emotion I felt today?",
        "If I had one thing I could do over today, what would it be?"
    };

    // Main method - entry point of the program
    static void Main(string[] args)
    {
        Journal journal = new Journal(); // Create a new journal instance
        bool running = true; // Flag to keep the program running

        // Main loop to display menu and handle user input
        while (running)
        {
            Console.WriteLine("Journal Menu:");
            Console.WriteLine("1. Write a new entry");
            Console.WriteLine("2. Display the journal");
            Console.WriteLine("3. Save the journal to a file");
            Console.WriteLine("4. Load the journal from a file");
            Console.WriteLine("5. Quit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            // Handle user choice
            switch (choice)
            {
                case "1":
                    WriteNewEntry(journal); // Write a new journal entry
                    break;
                case "2":
                    journal.DisplayEntries(); // Display all journal entries
                    break;
                case "3":
                    SaveJournal(journal); // Save journal to a file
                    break;
                case "4":
                    LoadJournal(journal); // Load journal from a file
                    break;
                case "5":
                    running = false; // Exit the program
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again."); // Handle invalid input
                    break;
            }
        }
    }

    // Method to write a new journal entry
    static void WriteNewEntry(Journal journal)
    {
        Random random = new Random();
        string prompt = prompts[random.Next(prompts.Count)]; // Select a random prompt
        Console.WriteLine(prompt);
        Console.Write("Your response: ");
        string response = Console.ReadLine();
        journal.AddEntry(new Entry(prompt, response)); // Add the new entry to the journal
    }

    // Method to save the journal to a file
    static void SaveJournal(Journal journal)
    {
        Console.Write("Enter the filename to save the journal: ");
        string filename = Console.ReadLine();
        journal.SaveToFile(filename); // Save journal entries to the specified file
        Console.WriteLine("Journal saved.");
    }

    // Method to load the journal from a file
    static void LoadJournal(Journal journal)
    {
        Console.Write("Enter the filename to load the journal: ");
        string filename = Console.ReadLine();
        
        try
        {
            journal.LoadFromFile(filename); // Load journal entries from the specified file
            Console.WriteLine("Journal loaded.");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: The file '{filename}' was not found."); // Handle file not found error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading the journal: {ex.Message}"); // Handle other errors
        }
    }
}

// Class representing a journal entry
public class Entry
{
    public string Prompt { get; set; } // The prompt for the entry
    public string Response { get; set; } // The user's response to the prompt
    public string Date { get; set; } // The date the entry was created

    // Constructor to initialize a new entry
    public Entry(string prompt, string response)
    {
        Prompt = prompt;
        Response = response;
        Date = DateTime.Now.ToString("yyyy-MM-dd"); // Set the date to the current date
    }

    // Override ToString method to format the entry for display
    public override string ToString()
    {
        return $"{Date} - {Prompt}\n{Response}\n";
    }
}

// Class representing a journal
public class Journal
{
    private List<Entry> entries = new List<Entry>(); // List to store journal entries

    // Method to add a new entry to the journal
    public void AddEntry(Entry entry)
    {
        entries.Add(entry);
    }

    // Method to display all journal entries
    public void DisplayEntries()
    {
        foreach (var entry in entries)
        {
            Console.WriteLine(entry);
        }
    }

    // Method to save journal entries to a file
    public void SaveToFile(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            foreach (var entry in entries)
            {
                writer.WriteLine($"{entry.Date}|{entry.Prompt}|{entry.Response}");
            }
        }
    }

    // Method to load journal entries from a file
    public void LoadFromFile(string filename)
    {
        entries.Clear(); // Clear existing entries
        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split('|');
                if (parts.Length == 3)
                {
                    entries.Add(new Entry(parts[1], parts[2]) { Date = parts[0] }); // Add loaded entry to the journal
                }
            }
        }
    }
}