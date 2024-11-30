using System;
using System.Collections.Generic;
using System.Linq;

class TLA
{
    static List<string> States;
    static string[] Alphabet;
    static List<string> FinalStates;
    static Dictionary<(string, string), string> Transitions;

    static void Main()
    {
        int n = int.Parse(Console.ReadLine()); // Number of states
        States = new List<string> { Console.ReadLine().Split()[0] }; // Initial state
        int z = int.Parse(Console.ReadLine()); // Size of alphabet
        Alphabet = Console.ReadLine().Split(); // Alphabet symbols
        int m = int.Parse(Console.ReadLine()); // Number of final states
        FinalStates = Console.ReadLine().Split().ToList(); // Final states
        int k = int.Parse(Console.ReadLine()); // Number of transitions

        Transitions = new Dictionary<(string, string), string>();
        for (int i = 0; i < k; i++)
        {
            string[] transition = Console.ReadLine().Split(',');
            string from = transition[0];
            string symbol = transition[1];
            string to = transition[2];
            Transitions[(from, symbol)] = to;

            if (!States.Contains(to))
            {
                States.Add(to); // Add the state if it doesn't exist
            }
        }

        States = States.Except(FinalStates).ToList(); // Remove final states from the states list
        Call();
    }

    static List<List<string>> FindSets(List<List<string>> equivalenceClasses)
    {
        List<List<string>> result = new List<List<string>>();

        foreach (var eqClass in equivalenceClasses)
        {
            List<string> temp = new List<string>();

            for (int i = 0; i < eqClass.Count; i++)
            {
                if (IsNewClass(result, eqClass[i]))
                {
                    temp.Add(eqClass[i]);
                }

                for (int j = i + 1; j < eqClass.Count; j++)
                {
                    try
                    {
                        for (int l = 0; l < Alphabet.Length; l++)
                        {
                            if (!IsInSameGroup(equivalenceClasses, Transitions[(eqClass[i], Alphabet[l])], Transitions[(eqClass[j], Alphabet[l])]))
                            {
                                throw new Exception("Not In The Same Group");
                            }
                        }

                        if (IsNewClass(result, eqClass[j]))
                        {
                            temp.Add(eqClass[j]);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (temp.Count > 0)
                {
                    result.Add(new List<string>(temp));
                    temp.Clear();
                }
            }
        }

        return result;
    }

    static bool IsInSameGroup(List<List<string>> equivalenceClasses, string A, string B)
    {
        foreach (var eqClass in equivalenceClasses)
        {
            if (eqClass.Contains(A) && eqClass.Contains(B))
            {
                return true;
            }
        }
        return false;
    }

    static bool IsNewClass(List<List<string>> equivalenceClasses, string A)
    {
        foreach (var eqClass in equivalenceClasses)
        {
            if (eqClass.Contains(A))
            {
                return false;
            }
        }
        return true;
    }

    static void Call()
    {
        List<List<string>> initialClasses = new List<List<string>> { States, FinalStates };
        List<List<string>> resultClasses = initialClasses;

        // Run the FindSets function multiple times to refine the equivalence classes
        for (int i = 0; i < 3; i++)
        {
            resultClasses = FindSets(resultClasses);
        }

        // Output the number of equivalence classes
        Console.WriteLine(resultClasses.Count);
    }
}
