using System;
namespace RaceTo21
{
    public class Card
    {
        public string id;
        public string fullName;
        public Card(string shortName, string longName)
        {
            id = shortName;
            fullName = longName;
        }
    }
}
