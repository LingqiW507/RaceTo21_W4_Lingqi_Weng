using System;
using System.Collections.Generic;

namespace RaceTo21
{
    public class Game
    {
        int numberOfPlayers; // number of players in current game
        List<Player> players = new List<Player>(); // list of objects containing player data
        CardTable cardTable; // object in charge of displaying game information
        Deck deck = new Deck(); // deck of cards
        int currentPlayer = 0; // current player on list
        public Task nextTask; // keeps track of game state
        private bool cheating = false; // lets you cheat for testing purposes if true

        public Game(CardTable c)
        {
            cardTable = c;
            deck.Shuffle();
            deck.ShowAllCards();
            nextTask = Task.GetNumberOfPlayers;
        }

        /* Adds a player to the current game
         * Called by DoNextTask() method
         */
        public void AddPlayer(string n)
        {
            players.Add(new Player(n));
        }

        /* Figures out what task to do next in game
         * as represented by field nextTask
         * Calls methods required to complete task
         * then sets nextTask.
         */
        public void DoNextTask()
        {
            Console.WriteLine("================================"); // this line should be elsewhere right?
            if (nextTask == Task.GetNumberOfPlayers)
            {
                numberOfPlayers = cardTable.GetNumberOfPlayers();
                nextTask = Task.GetNames;
            }
            else if (nextTask == Task.GetNames)
            {
                for (var count = 1; count <= numberOfPlayers; count++)
                {
                    var name = cardTable.GetPlayerName(count);
                    AddPlayer(name); // NOTE: player list will start from 0 index even though we use 1 for our count here to make the player numbering more human-friendly
                }
                nextTask = Task.IntroducePlayers;
            }
            else if (nextTask == Task.IntroducePlayers)
            {
                cardTable.ShowPlayers(players);
                nextTask = Task.PlayerTurn;
            }
            else if (nextTask == Task.PlayerTurn)
            {
                cardTable.ShowHands(players);
                Player player = players[currentPlayer];
                if (player.status == PlayerStatus.active)
                {
/*A player can choose to draw up to 3 cards each turn, but they get all cards at once; 
 * they don’t get to decide after each card (more risk, but can get to 21 faster!) */
                    bool getOneCard = cardTable.OfferACard(player);
                    bool getThreeCards = false;
                    //When a player wants to take a card, it also asks if they need three cards
                    if (getOneCard)
                    {
                        getThreeCards = cardTable.OfferThreeCards(player);
                    }
                    //if a player only want to get one card
                    if (getOneCard && !getThreeCards)
                    { 
                        Card card = deck.DealTopCard();
                        player.cards.Add(card);
                        player.score = ScoreHand(player);
                        if (player.score > 21)
                        {
                            player.status = PlayerStatus.bust;
                        }
                        else if (player.score == 21)
                        {
                            player.status = PlayerStatus.win;
                        }
                    }
                    //if a player want to get three cards
                    else if (getOneCard && getThreeCards)
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            Card card = deck.DealTopCard();
                            player.cards.Add(card);
                        }
                        player.score = ScoreHand(player);
                        if (player.score > 21)
                        {
                            player.status = PlayerStatus.bust;
                        }
                        else if (player.score == 21)
                        {
                            player.status = PlayerStatus.win;

                        }
                        //If the player got three cards and still not bust or win, will stay 
                        if (player.status == PlayerStatus.active)
                        {
                            player.status = PlayerStatus.stay;
                        }
                    }
                    else
                    {
                        player.status = PlayerStatus.stay;
                    }
                }
                cardTable.ShowHand(player);
                nextTask = Task.CheckForEnd;
            }
            else if (nextTask == Task.CheckForEnd)
            {
                if (!CheckActivePlayers())
                {
                    Player winner = DoFinalScoring();
                    if (winner != null)
                    {
                        cardTable.AnnounceWinner(winner);

                    }
                    nextTask = Task.GameOver;
                }
                else
                {
                    currentPlayer++;
                    if (currentPlayer > players.Count - 1)
                    {
                        currentPlayer = 0; // back to the first player...
                    }
                    nextTask = Task.PlayerTurn;
                }
            }
            else // we shouldn't get here...
            {
                Console.WriteLine("I'm sorry, I don't know what to do now!");
                nextTask = Task.GameOver;
            }
        }

        public int ScoreHand(Player player)
        {
            int score = 0;
            if (cheating == true && player.status == PlayerStatus.active)
            {
                string response = null;
                while (int.TryParse(response, out score) == false)
                {
                    Console.Write("OK, what should player " + player.name + "'s score be?");
                    response = Console.ReadLine();
                }
                return score;
            }
            else
            {
                foreach (Card card in player.cards)
                {
                    string faceValue = card.id.Remove(card.id.Length - 1);
                    switch (faceValue)
                    {
                        case "K":
                        case "Q":
                        case "J":
                            score = score + 10;
                            break;
                        case "A":
                            score = score + 1;
                            break;
                        default:
                            score = score + int.Parse(faceValue);
                            break;
                    }
                }
            }
            return score;
        }

        public bool CheckActivePlayers()
        {
            //if find the first winner,end the game
            foreach (var player in players)
            {
                if (player.status == PlayerStatus.win)
                {
                    return false;
                }
            }
            int bustPlayer = 0;

            foreach (var player in players)
            {
                if (player.status == PlayerStatus.bust)
                {
                    bustPlayer++;
                }
            }
            if (bustPlayer == players.Count - 1)
            {
                return false;
            }

            foreach (var player in players)
            {
                if (player.status == PlayerStatus.active)
                {
                    return true; // at least one player is still going!
                }
            }
            return false; // everyone has stayed!
        }
        public Player DoFinalScoring()
        {
            int highScore = 0;
            foreach (var player in players)
            {
                cardTable.ShowHand(player);
                if (player.status == PlayerStatus.win) // someone hit 21
                {
                    return player;
                }
                if (player.status == PlayerStatus.stay) // still could win...
                {
                    if (player.score > highScore)
                    {
                        highScore = player.score;
                    }
                }
                // if busted don't bother checking!
            }
            if (highScore > 0) // someone scored, anyway!
            {
                // find the FIRST player in list who meets win condition
                return players.Find(player => player.score == highScore);
            }
            return null; // everyone must have busted because nobody won!
        }
    /* At end of round, each player is asked if they want to keep playing.If a player says no, 
     * they are removed from the player list.If only 1 player remains, that player is the winner
     * (equivalent to everyone else “folding” in a card game). */
        public void toBeContinued()
        {
            int remainPlayers = players.Count;
            for (int p = 0; p < players.Count; p++)
            { 
                Console.Write(players[p].name + ", One more game (Y/N)");
                string response = Console.ReadLine();
                if (response.ToUpper().StartsWith("Y"))
                {
                    players[p].cards = new List<Card>();
                    players[p].status = PlayerStatus.active;
                }
                else if (response.ToUpper().StartsWith("N"))
                {
                    //If a player says no, they are removed from the player list
                    players.RemoveAt(p);
                    p--;
                }
                else
                {
                    Console.WriteLine("Please answer Y(es) or N(o)!");
                }
            }
            //If only 1 player remains, that player is the winner
            if (remainPlayers == 1)
            {
                Console.WriteLine(players[0].name + " wins!");
                Console.Write("Press <Enter> to exit... ");
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                nextTask = Task.GameOver;
            }

        }
    }
}
