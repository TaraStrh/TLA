using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DFA
{
    class FiniteStateMachine
    {
        private Dictionary<string, State> _states;
        private HashSet<char> _alphabet;
        private State _startingState;

        public FiniteStateMachine(HashSet<char> alphabet)
        {
            _states = new Dictionary<string, State>();
            _alphabet = alphabet;
        }

        public FiniteStateMachine(string regex)
        {
            _alphabet = ExtractAlphabet(regex);
            _states = new Dictionary<string, State>();
            var nfaResult = ConvertToNFA(regex);
            _startingState = nfaResult.Item1;
            nfaResult.Item2.IsFinal = true;
        }

        private Tuple<State, State> ConvertToNFA(string regex)
        {
            if (regex.Length == 1)
                return CreateSimpleNFA(regex[0]);

            var tokens = TokenizeRegex(regex);
            if (tokens.Count > 1)
                return CreateOrNFA(tokens);

            return CreateComplexNFA(regex);
        }

        private Tuple<State, State> CreateSimpleNFA(char character)
        {
            State startState = GenerateState();
            State endState = GenerateState();
            startState.AddTransition(character, endState);
            return new Tuple<State, State>(startState, endState);
        }

        private List<string> TokenizeRegex(string regex)
        {
            List<string> tokens = new List<string>();
            StringBuilder tokenBuilder = new StringBuilder();
            int scopeLevel = 0;

            for (int i = 0; i < regex.Length; i++)
            {
                if (IsOpeningParenthesis(regex, i))
                    scopeLevel++;
                else if (IsClosingParenthesis(regex, i))
                    scopeLevel--;
                else if (IsOrOperator(regex, i, scopeLevel))
                {
                    tokens.Add(tokenBuilder.ToString());
                    tokenBuilder.Clear();
                    continue;
                }
                tokenBuilder.Append(regex[i]);
            }
            if (tokenBuilder.Length > 0)
                tokens.Add(tokenBuilder.ToString());

            return tokens;
        }

        private bool IsOpeningParenthesis(string regex, int index) => regex[index] == '(' && (index == 0 || regex[index - 1] != '\\');
        private bool IsClosingParenthesis(string regex, int index) => regex[index] == ')' && (index == 0 || regex[index - 1] != '\\');
        private bool IsOrOperator(string regex, int index, int scopeLevel) => regex[index] == '|' && scopeLevel == 0 && (index == 0 || regex[index - 1] != '\\');

        private Tuple<State, State> CreateOrNFA(List<string> tokens)
        {
            State rootState = GenerateState();
            State tailState = GenerateState();

            foreach (var token in tokens)
            {
                var nfaResult = ConvertToNFA(token);
                rootState.AddTransition('$', nfaResult.Item1);
                nfaResult.Item2.AddTransition('$', tailState);
            }

            return new Tuple<State, State>(rootState, tailState);
        }

        private Tuple<State, State> CreateComplexNFA(string regex)
        {
            State currentStartState = null;
            State currentEndState = null;

            for (int i = 0; i < regex.Length; i++)
            {
                if (IsOpeningParenthesis(regex, i))
                {
                    var innerNfa = ProcessInnerRegex(ref i, regex);
                    currentEndState = UpdateCurrentStates(innerNfa, ref currentStartState, currentEndState);
                }
                else
                {
                    var singleCharNfa = ProcessSingleCharacter(ref i, regex);
                    currentEndState = UpdateCurrentStates(singleCharNfa, ref currentStartState, currentEndState);
                }
            }

            return new Tuple<State, State>(currentStartState, currentEndState);
        }

        private Tuple<State, State> ProcessInnerRegex(ref int i, string regex)
        {
            int j = i + 1;
            int scopeLevel = 1;
            while (j < regex.Length && scopeLevel > 0)
            {
                if (IsOpeningParenthesis(regex, j)) scopeLevel++;
                if (IsClosingParenthesis(regex, j)) scopeLevel--;
                j++;
            }
            var innerRegex = regex.Substring(i + 1, j - i - 2);
            var innerNfa = ConvertToNFA(innerRegex);
            i = j - 1;
            if (i < regex.Length - 1 && regex[i + 1] == '*')
            {
                innerNfa = MakeStarTransition(innerNfa.Item1, innerNfa.Item2);
                i++;
            }

            return innerNfa;
        }

        private Tuple<State, State> ProcessSingleCharacter(ref int i, string regex)
        {
            char inputChar = regex[i];
            if (inputChar == '\\')
            {
                i++;
                inputChar = regex[i];
            }
            var singleCharNfa = CreateSimpleNFA(inputChar);
            if (i < regex.Length - 1 && regex[i + 1] == '*')
            {
                singleCharNfa = MakeStarTransition(singleCharNfa.Item1, singleCharNfa.Item2);
                i++;
            }

            return singleCharNfa;
        }

        private State UpdateCurrentStates(Tuple<State, State> nfa, ref State currentStartState, State currentEndState)
        {
            if (currentEndState != null)
                currentEndState.AddTransition('$', nfa.Item1);
            else
                currentStartState = nfa.Item1;
            return nfa.Item2;
        }

        private Tuple<State, State> MakeStarTransition(State startState, State endState)
        {
            State newStartState = GenerateState();
            State newEndState = GenerateState();

            newStartState.AddTransition('$', startState);
            endState.AddTransition('$', startState);
            endState.AddTransition('$', newEndState);
            newStartState.AddTransition('$', newEndState);

            return Tuple.Create(newStartState, newEndState);
        }

        private static HashSet<char> ExtractAlphabet(string regex)
        {
            HashSet<char> alphabet = new HashSet<char>();

            for (int i = 0; i < regex.Length; i++)
            {
                if (IsSpecialCharacter(regex[i]))
                    continue;

                if (IsEscapeCharacter(regex, i))
                {
                    if (i + 1 < regex.Length)
                    {
                        alphabet.Add(regex[i + 1]);
                        i++;
                    }
                }
                else
                {
                    alphabet.Add(regex[i]);
                }
            }

            return alphabet;
        }

        private static bool IsSpecialCharacter(char c)
        {
            return c == '(' || c == ')' || c == '*' || c == '|';
        }

        private static bool IsEscapeCharacter(string regex, int index)
        {
            return regex[index] == '\\';
        }

        private State GenerateState(bool isFinal = false)
        {
            var state = new State($"{_states.Count + 1}", isFinal);
            if (_states.Count == 0)
                _startingState = state;
            _states.Add(state.Name, state);
            return state;
        }

        public void AddState(string name, bool isFinal)
        {
            var state = new State(name, isFinal);
            if (_states.Count == 0)
                _startingState = state;
            _states.Add(name, state);
        }

        public void RemoveState(string name)
        {
            _states.Remove(name);
        }

        public void AddTransition(string from, char input, string to)
        {
            if (!_alphabet.Contains(input) && input != '$')
                throw new Exception("This input is not allowed.");
            var fromState = _states[from];
            var toState = _states[to];
            fromState.AddTransition(input, toState);
        }

        public bool MatchString(string input)
        {
            return Match(input, 0, _startingState);
        }

        private bool Match(string text, int index, State currentState)
        {
            if (index == text.Length)
                return CheckFinalStateOrEpsilonTransitions(currentState);

            char nextChar = text[index];
            return ProcessTransitions(text, index, currentState, nextChar);
        }

        private bool CheckFinalStateOrEpsilonTransitions(State state)
        {
            if (state.IsFinal)
                return true;

            if (state.Transitions.ContainsKey('$'))
            {
                foreach (var nextState in state.Transitions['$'])
                {
                    if (Match(string.Empty, 0, nextState))
                        return true;
                }
            }
            return false;
        }

        private bool ProcessTransitions(string text, int index, State currentState, char nextChar)
        {
            foreach (var transition in currentState.Transitions)
            {
                char input = transition.Key;
                foreach (var nextState in transition.Value)
                {
                    if ((input == nextChar && Match(text, index + 1, nextState)) ||
                        (input == '$' && Match(text, index, nextState)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DepthFirstSearch(State currentState, HashSet<State> visitedStates)
        {
            visitedStates.Add(currentState);

            foreach (var neighborList in currentState.Transitions.Values)
            {
                foreach (var neighbor in neighborList)
                {
                    if (!visitedStates.Contains(neighbor))
                    {
                        DepthFirstSearch(neighbor, visitedStates);
                    }
                }
            }
        }
    }

    class State
    {
        public string Name { get; }
        public bool IsFinal { get; set; }
        public Dictionary<char, HashSet<State>> Transitions { get; }

        public State(string name, bool isFinal = false)
        {
            Name = name;
            IsFinal = isFinal;
            Transitions = new Dictionary<char, HashSet<State>>();
        }

        public void AddTransition(char input, State nextState)
        {
            if (!Transitions.ContainsKey(input))
            {
                Transitions[input] = new HashSet<State>();
            }
            Transitions[input].Add(nextState);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            Console.ReadLine();
            var automata = new FiniteStateMachine(Console.ReadLine());
            string text = Console.ReadLine();
            Console.WriteLine(automata.MatchString(text) ? "Accepted" : "Rejected");
        }
    }
}
