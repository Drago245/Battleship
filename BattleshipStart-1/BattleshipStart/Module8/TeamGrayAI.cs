using System;
using System.Collections.Generic;

namespace Module8
{
    internal class TeamGrayAI : IPlayer
    {
        private static readonly List<Position> Guesses = new List<Position>();
        private int _index;
        private static readonly Random Random = new Random();
        private int _gridSize;
        private Position lastGuess;
        private bool lastGuessHit = false;
        private bool hasGuessedYet = false;

        public TeamGrayAI(string name)
        {
            Name = name;
        }

        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {
            _gridSize = gridSize;
            _index = playerIndex;

            GenerateGuesses();

            // Random player just puts the ships in the grid in Random columns
            // Note it cannot deal with the case where there's not enough columns
            // for 1 per ship
            var availableColumns = new List<int>();
            for (int i = 0; i < gridSize; i++)
            {
                availableColumns.Add(i);
            }

            foreach (var ship in ships._ships)  // placing ships
            {
                // Choose an X from the set of remaining columns
                var x = availableColumns[Random.Next(availableColumns.Count)];
                availableColumns.Remove(x); //Make sure we can't pick it again

                // Choose a Y based o nthe ship length and grid size so it always fits
                var y = Random.Next(gridSize - ship.Length);
                ship.Place(new Position(x, y), Direction.Vertical);
            }
        }

        private void GenerateGuesses()
        {
            // We want all instances of TeamGrayAI to share the same pool of guesses
            // So they don't repeat each other.

            // We need to populate the guesses list, but not for every instance - so we only do it if the set is missing some guesses
            if (Guesses.Count < _gridSize*_gridSize)
            {
                Guesses.Clear();
                for (int x = 0; x < _gridSize; x++)
                {
                    for (int y = 0; y < _gridSize; y++)
                    {
                        Guesses.Add(new Position(x,y));
                    }
                }
            }
        }

        public string Name { get; }
        public int Index => _index;

        public Position GetAttackPosition()
        {
            // TeamGrayAI just guesses random squares. Its smart in that it never repeats a move from any other random 
            // player since they share the same set of guesses
            // But it doesn't take into account any other players guesses
            hasGuessedYet = true;


            if (lastGuessHit) // if the previous guess hit
            {

                Position subsequentGuess = Guesses.Find(g => g.X == lastGuess.X + 1 && g.Y == lastGuess.Y);

                if (subsequentGuess != null) // Makes sure that there was a valid guess immedietly to the right
                {
                    Guesses.Remove(subsequentGuess);
                    lastGuess = subsequentGuess;
                    return subsequentGuess;
                    
                }
                else  //  if no guesses remain immedieatly to the right (if the end of the board or already guessed) return to random
                {
                    var guess = Guesses[Random.Next(Guesses.Count)];
                    Guesses.Remove(guess); // Don't use this one again
                    lastGuess = guess;
                    return guess;
                }
            }

            else
            {
                var guess = Guesses[Random.Next(Guesses.Count)];
                Guesses.Remove(guess); // Don't use this one again
                lastGuess = guess;
                return guess;
            }
        }

        public void SetAttackResults(List<AttackResult> results)
        {

            if (!hasGuessedYet)
            {
                return;
            }

            AttackResult? currResult = null;

            foreach (var result in results)
            {
                if (result.PlayerIndex == _index &&  // find the matching location
                    result.Position.X == lastGuess.X && 
                    result.Position.Y == lastGuess.Y)
                {
                    currResult = result; // stop searching once the correct result is found
                    break;
                }
            }
            //MUST check for result before running, as this may be called before guesses happen
            if (currResult == null)
            {
                return;
            }
            
            switch (currResult.Value.ResultType)
            {
                case AttackResultType.Hit:
                    Console.WriteLine("HEY THIS IS A HIT HEY HEY HEY");
                    lastGuessHit = true;
                    break;

                case AttackResultType.Miss:
                    lastGuessHit = false;
                    break;

                case AttackResultType.Sank:
                    lastGuessHit = false;
                    break;
            }


        }
    }
}
