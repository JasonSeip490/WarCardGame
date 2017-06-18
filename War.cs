using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    War card game by Jason Seip

    Designed to be integrated with the Unity game engine.

    Game Rules:
    1. Game is for two players.
    2. Uses full deck of 52 cards.
    3. Each round begins with a "Battle".
    4. Battle involves comparing player top cards.
    5. Player with highest card adds both player cards to bottom of their stack.
    6. If player cards match, go to "War".
    7. In War, the next player card is skipped, and the one after that gets compared.
    8. Player with highest card gets all of them. If cards match again repeat War.

    Code Logic:
    1. Create source deck.
    2. Shuffle source deck.
    3. Distribute alternating cards from source deck to player card stacks (each player gets half the deck).
    4. During play, cards from the player stacks are removed and added to "battle" stacks.
    5. The winner of the Battle (or War, if necessary) gets both battle stacks added to their player stacks.
    6. After round, and before War, check to see if players have enough cards to continue. Declare winner if otherwise.
*/
public class War : MonoBehaviour
{
    #region variables

    //UI elements assigned in Unity editor
    public Text TextWinner;
    public Button ButtonNextBattle;
    public Text TextRoundsWonPlayer1;
    public Text TextCardsRemainingPlayer1;
    public Text TextRoundsWonPlayer2;
    public Text TextCardsRemainingPlayer2;
    public Text TextBattleCardsPlayer1;
    public Text TextBattleCardsPlayer2;
    public Text TextRoundWinner;

    //Enums to simplify game and UI logic
    enum FaceValue { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
    enum Suit { Clubs, Diamonds, Hearts, Spades }
    enum Mode { Battle = 1, War = 2 }
    enum Players { None, Player1, Player2 }

    //Build player stacks from this deck
    Deck sourceDeck = new Deck();

    //Player/Battle stacks. Could roll these into arrays, but then need to remember that index 0 is Player1, etc
    List<Card> stackPlayer1 = new List<Card>();
    List<Card> stackPlayer2 = new List<Card>();
    List<Card> battleStackPlayer1 = new List<Card>();
    List<Card> battleStackPlayer2 = new List<Card>();

    //For UI display
    int roundsWonPlayer1 = 0;
    int roundsWonPlayer2 = 0;

    #endregion

    #region Classes

    class Card
    {
        public Suit suit;
        public FaceValue faceValue;

        public Card(Suit s, FaceValue f)
        {
            suit = s;
            faceValue = f;
        }
    }

    class Deck
    {
        private const int CardsInDeck = 52;
        public List<Card> sourceCards = new List<Card>();
        private System.Random randomCardPicker = new System.Random();

        //Automatically populate the full deck on creation
        public Deck()
        {
            for (Suit s = Suit.Clubs; s <= Suit.Spades; s++)
            {
                for (FaceValue f = FaceValue.Two; f <= FaceValue.Ace; f++)
                {
                    this.sourceCards.Add(new Card(s, f));
                }
            }
        }

        //Shuffle the deck
        //A "numerical" shuffle that does not simulate how real humans shuffle, but should be effective
        public void Shuffle()
        {
            for (int i = 0; i < CardsInDeck; i++)
            {
                int cardNumber = randomCardPicker.Next(CardsInDeck);
                Card tmpCard = sourceCards[i];
                sourceCards[i] = sourceCards[cardNumber];
                sourceCards[cardNumber] = tmpCard;
            }
        }

    }

    #endregion

    #region Game Start

    //Unity always calls this function on startup
    void Start()
    {
        //Setup UI elements
        TextWinner.gameObject.SetActive(false);

        //Each round is triggered by press of "Next Battle" button
        ButtonNextBattle.onClick.AddListener(() => {
            Text tmpText = ButtonNextBattle.GetComponentInChildren<Text>();
            tmpText.text = "Next Battle";
            Battle();
        });

        //Shuffle the deck
        sourceDeck.Shuffle();

        //Deal the cards to the players
        for (int i = 0; i < sourceDeck.sourceCards.Count - 1; i += 2)
        {
            stackPlayer1.Add(sourceDeck.sourceCards[i]);
            stackPlayer2.Add(sourceDeck.sourceCards[i + 1]);
        }

        #region Dealing Logs

        //These logs were used to verify that player card stacks were created properly

        //View all the player cards
        //for (int i = 0; i < 26; i++)
        //{
        //    Debug.Log("Player1 card " + i + ": " + stackPlayer1[i].faceValue + " of " + stackPlayer1[i].suit);
        //}
        //for (int i = 0; i < 26; i++)
        //{
        //    Debug.Log("Player2 card " + i + ": " + stackPlayer2[i].faceValue + " of " + stackPlayer2[i].suit);
        //}

        //Verify that none of Player1's cards are the same as Player2's cards
        //for (int i = 0; i < 26; i++)
        //{
        //    for (int j = 0; j < 26; j++)
        //    {
        //        if (stackPlayer1[i].faceValue == stackPlayer2[j].faceValue && stackPlayer1[i].suit == stackPlayer2[j].suit)
        //        {
        //            Debug.Log("Players have the same card!");
        //        }
        //    }
        //}

        #endregion

        //Initialize the UI
        UpdateUI(Players.None);

        //Game begins when the "Start Game" button is pressed
    }

    #endregion

    #region Game Logic

    void Battle()
    {
        //Reset the battle stacks
        battleStackPlayer1.Clear();
        battleStackPlayer2.Clear();

        //Add top player cards to battle stacks
        battleStackPlayer1.Add(stackPlayer1[0]);
        battleStackPlayer2.Add(stackPlayer2[0]);

        //Remove top player cards from player stacks
        stackPlayer1.RemoveRange(0, 1);
        stackPlayer2.RemoveRange(0, 1);

        CompareCards();
    }

    void GoToWar()
    {
        //Add two cards from each player to the battle stacks
        battleStackPlayer1.Add(stackPlayer1[0]);
        battleStackPlayer1.Add(stackPlayer1[1]);
        battleStackPlayer2.Add(stackPlayer2[0]);
        battleStackPlayer2.Add(stackPlayer2[1]);

        //Remove two player cards from player stacks
        stackPlayer1.RemoveRange(0, 2);
        stackPlayer2.RemoveRange(0, 2);

        CompareCards();
    }

    void CompareCards()
    {
        //Always compare highest index of the battle stacks
        int index = battleStackPlayer1.Count - 1;

        int outcome = battleStackPlayer1[index].faceValue.CompareTo(battleStackPlayer2[index].faceValue);
        if (outcome == 0)
        {
            //Cards are equal, so go to War (if each player has at least two cards left)
            if (GameNotOver(Mode.War))
            {
                GoToWar();
            }
        }
        else if (outcome > 0)
        {
            //Player1's card is higher than Player2's card so Player1 wins this Battle

            //Move all battle stack cards to end of Player1's stack
            stackPlayer1.AddRange(battleStackPlayer1);
            stackPlayer1.AddRange(battleStackPlayer2);

            //Update UI
            roundsWonPlayer1++;
            UpdateUI(Players.Player1);

            //Each player must have at least one card left for next Battle
            GameNotOver(Mode.Battle);
        }
        else
        {
            //Player2's card is higher than Player1's card so Player2 wins this Battle

            //Move all battle stack cards to end of Player2's stack
            stackPlayer2.AddRange(battleStackPlayer1);
            stackPlayer2.AddRange(battleStackPlayer2);

            //Update UI
            roundsWonPlayer2++;
            UpdateUI(Players.Player2);

            //Each player must have at least one card left for next Battle
            GameNotOver(Mode.Battle);
        }
    }

    bool GameNotOver(Mode mode)
    {
        //Battle requires one card, War requires two cards (see enum declaration)
        int cardsNecessary = (int)mode;

        if (stackPlayer1.Count < cardsNecessary || stackPlayer2.Count < cardsNecessary)
        {
            //Show winner text, hide button for next battle
            TextWinner.gameObject.SetActive(true);
            ButtonNextBattle.gameObject.SetActive(false);

            if (stackPlayer1.Count < cardsNecessary && stackPlayer2.Count < cardsNecessary)
            {
                //Tie
                TextWinner.text = "Both players are out of cards!";
            }
            else if (stackPlayer1.Count < cardsNecessary)
            {
                //Player1 loses
                TextWinner.text = "Player 2 has won the game!";
            }
            else if (stackPlayer2.Count < cardsNecessary)
            {
                //Player2 loses
                TextWinner.text = "Player 1 has won the game!";
            }

            return false;
        }

        return true;
    }

    #endregion

    #region UI

    void UpdateUI(Players roundWinner)
    {
        DisplayPlayerStats();
        DisplayBattleStats(roundWinner);
    }

    void DisplayPlayerStats()
    {
        //Update player stats for rounds won and cards remaining

        TextRoundsWonPlayer1.text = "Rounds Won: " + roundsWonPlayer1.ToString();
        TextCardsRemainingPlayer1.text = "Cards Remaining: " + stackPlayer1.Count;
        TextRoundsWonPlayer2.text = "Rounds Won: " + roundsWonPlayer2.ToString();
        TextCardsRemainingPlayer2.text = "Cards Remaining: " + stackPlayer2.Count;
    }

    void DisplayBattleStats(Players roundWinner)
    {
        //Show all the cards used in the Battle/War just completed
        //(Multiple repeats of War could cause the card display to be trimmed, but this is very rare)

        string textPlayer1 = "Player 1: ";
        string textPlayer2 = "Player 2: ";

        for (int i = 0; i < battleStackPlayer1.Count; i++)
        {
            string facedown = "";

            if (i > 0)
            {
                textPlayer1 += ", ";
                textPlayer2 += ", ";

                //Odd numbered cards are face down and not compared, so note this in UI
                if (i % 2 != 0)
                {
                    facedown = " (face down)";
                }
            }

            textPlayer1 += battleStackPlayer1[i].faceValue.ToString() + " of " + battleStackPlayer1[i].suit.ToString() + facedown;
            textPlayer2 += battleStackPlayer2[i].faceValue.ToString() + " of " + battleStackPlayer2[i].suit.ToString() + facedown;
        }

        TextBattleCardsPlayer1.text = textPlayer1;
        TextBattleCardsPlayer2.text = textPlayer2;

        if (roundWinner == Players.Player1)
        {
            TextRoundWinner.text = "Round Winner: Player 1";
        }
        else if (roundWinner == Players.Player2)
        {
            TextRoundWinner.text = "Round Winner: Player 2";
        }
        else
        {
            TextRoundWinner.text = "Round Winner:";
        }
    }

    #endregion

}
