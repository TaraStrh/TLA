using System;
using System.Collections.Generic;
using System.Linq;

class NFA
{
    private readonly Dictionary<string, Dictionary<char, List<string>>> _transitions;
    private readonly HashSet<string> _acceptStates;
    private readonly string _startState;

    public NFA(
        IEnumerable<string> states,
        IEnumerable<char> alphabet,
        IEnumerable<(string, char, string)> transitions,
        string startState,
        IEnumerable<string> acceptStates)
    {
        _startState = startState;
        _acceptStates = new HashSet<string>(acceptStates);
        _transitions = states.ToDictionary(state => state, state => new Dictionary<char, List<string>>());

        foreach (var (from, symbol, to) in transitions)
        {
            if (!_transitions[from].ContainsKey(symbol))
            {
                _transitions[from][symbol] = new List<string>();
            }
            _transitions[from][symbol].Add(to);
        }
    }

    public bool Accepts(string input)
    {
        return Accepts(input, _startState, new Stack<string>());
    }

    private bool Accepts(string input, string currentState, Stack<string> visitedStates)
    {
        if (input == string.Empty)
        {
            if (_acceptStates.Contains(currentState))
                return true;

            // Check for epsilon transitions on empty input
            if (_transitions[currentState].ContainsKey('$') && !visitedStates.Contains(currentState))
            {
                visitedStates.Push(currentState);
                foreach (var nextState in _transitions[currentState]['$'])
                {
                    if (Accepts(input, nextState, visitedStates))
                        return true;
                }
                visitedStates.Pop();
            }

            return false;
        }

        char symbol = input[0];
        string remainingInput = input.Substring(1);

        if (_transitions[currentState].ContainsKey(symbol))
        {
            foreach (var nextState in _transitions[currentState][symbol])
            {
                if (Accepts(remainingInput, nextState, visitedStates))
                    return true;
            }
        }

        // Epsilon transitions
        if (_transitions[currentState].ContainsKey('$') && !visitedStates.Contains(currentState))
        {
            visitedStates.Push(currentState);
            foreach (var nextState in _transitions[currentState]['$'])
            {
                if (Accepts(input, nextState, visitedStates))
                    return true;
            }
            visitedStates.Pop();
        }

        return false;
    }
}

class Program
{
    static void Main()
    {
        int numStates = int.Parse(Console.ReadLine());
        var states = Console.ReadLine().Split(' ');

        int alphabetSize = int.Parse(Console.ReadLine()); // not used, but read to adhere to input format
        var alphabet = Console.ReadLine().Replace("$", string.Empty).ToCharArray(); // remove $ as it's for epsilon transitions

        int numAcceptStates = int.Parse(Console.ReadLine());
        var acceptStates = Console.ReadLine().Split(' ');

        int numTransitions = int.Parse(Console.ReadLine());
        var transitions = new List<(string, char, string)>();
        for (int i = 0; i < numTransitions; i++)
        {
            string[] transitionParts = Console.ReadLine().Split(',');
            char symbol = transitionParts[1] == "$" ? '$' : transitionParts[1][0]; // special case for epsilon transitions
            transitions.Add((transitionParts[0], symbol, transitionParts[2]));
        }

        string inputString = Console.ReadLine();

        var nfa = new NFA(states, alphabet, transitions, states.First(), acceptStates);
        bool isAccepted = nfa.Accepts(inputString);

        Console.WriteLine(isAccepted ? "Accepted" : "Rejected");
    }
}