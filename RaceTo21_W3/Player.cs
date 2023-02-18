using System;
using System.Collections.Generic;

namespace RaceTo21
{
	public class Player
	{
		public string name;
		public List<Card> cards = new List<Card>();
		public PlayerStatus status = PlayerStatus.active;
		public int score;
        private object players;

        public Player(string n)
		{
			name = n;
		}

		/* Introduces player by name
		 * Called by CardTable object
		 */
		public void Introduce(int playerNum)
		{
			Console.WriteLine("Hello, my name is " + name + " and I am player #" + playerNum);
		}
		public void ShufflePlayer()
		{
			Console.WriteLine("Shuffling Players...");

			Random pyr = new Random();

			// one-line method that uses Linq:
			// cards = cards.OrderBy(a => rng.Next()).ToList();

			// multi-line method that uses Array notation on a list!
			// (this should be easier to understand)
			for (int i = 0; i < players.Count; i++)
			{
				Player pp = players[i];
				int swapindex = pyr.Next(players.Count);
				players[i] = players[swapindex];
				players[swapindex] = pp;
			}
		}
	}

}