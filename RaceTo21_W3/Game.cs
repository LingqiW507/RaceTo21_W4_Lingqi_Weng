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
                    int number = cardTable.GetNumberOfCards(player); //the player want to draw how many cards this loop will work related times. 

                    if (number <= 3)
                    {
                        //int number = cardTable.GetNumberOfCards(player); 
                        for (int i = 0; i < number; i++)
                        {

                            Card card = deck.DealTopCard();
                            player.cards.Add(card);
                            player.score = ScoreHand(player);
                        }
                        if (player.score > 21)
                        {
                            player.status = PlayerStatus.bust;
                        }
                        else if (player.score == 21)
                        {
                            player.status = PlayerStatus.win;
                        }
                        else
                        {
                            player.status = PlayerStatus.stay;
                        }
                    }

                    else if (number == 0)
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
                        nextTask = Task.GameOver;
                    }
                    else
                    {
                        cardTable.AnnounceWinner(winner);
                        nextTask = Task.GameOver;
                    }
                    //cardTable.AnnounceWinner(winner);
                    //nextTask = Task.GameOver;

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


        //private int GetNumberOfCards(Player player)
        //{
        //   return 1;
        //}

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
        public void CheckPlayers()//Ask players if they want to play again
        {

            foreach (var player in players)
            {
                if (!CardTable.ToBeContinue(player))//if there is one player don't play again
                {
                    //the
                    nextTask = Task.GameOver;//the game will be over
                }

                else//if everyone keeps playing
                {
                    //continue the game 
                }
            }
        }
        private void PlayAgain()
        {
            //shuffle cards 
            deck = new Deck();
            deck.Shuffle();
            //shuffle players
            Player.ShufflePlayer();
        }
        //I don't know how to put them together
        /* first check they want to stay and play again
         * then if they are all want to stay the game will reset the table
         * if there is no one want to play the game will be end.
    }
}
    