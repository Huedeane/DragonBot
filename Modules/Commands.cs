using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace TutorialBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private string[] effect = new string[] { "False", "???? " };
        private string[] type = new string[] { "False", "???? " };
        private string[] attribute = new string[] { "False", "???? " };
        private string[] level = new string[] { "False", "???? " };
        private string[] scale = new string[] { "False", "???? " };
        private string[] stat = new string[] { "False", "???? " };
        private string[] property = new string[] { "False", "???? " };
        private string[] alignment = new string[] { "False", "???? " };
        private int num = 0;

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("block")]
        [Alias(new string[] { "b" })]
        public async Task Block(string name)
        {

            if (checkForAvailability(name, "blacklist.txt", 0) == true)
            {
                await ReplyAsync(name + " is already blocked");
            }
            else
            {
                TextWriter file = new StreamWriter("blacklist.txt", true);
                await ReplyAsync(name + " has been blocked");
                file.WriteLine(name);
                file.Close();
            }
        }

        [Command("unblock")]
        [Alias(new string[] { "ub" })]
        public async Task Unblock(string name)
        {

            if (checkForAvailability(name, "blacklist.txt", 0) == true)
            {
                checkForAvailability(name, "blacklist.txt", 1);
                await ReplyAsync(name + " has been unblocked");

            }
            else
            {
                await ReplyAsync(name + " has not been blocked");
            }
        }

        [Command("saveQuote")]
        [Alias(new string[] { "sq" })]
        [Summary("quote what wrote to be output later")]
        public async Task SaveQuote([Remainder]string quote)
        {

            if (checkForAvailability(Context.User.Username, "blacklist.txt", 0) == true)
            {
                await ReplyAsync("blocked");
                return;
            }
            string fullQuote = quote + " ~" + Context.User.Username;
            if (quote.Contains("~"))
            {
                await ReplyAsync("Error, your message contains a ~");
            }
            else
            {
                if (checkForAvailability(fullQuote, "quote.txt", 0) == true)
                {
                    await ReplyAsync("Text already exist");
                }
                else
                {
                    TextWriter file = new StreamWriter("quote.txt", true);
                    file.WriteLine(fullQuote);
                    file.Close();
                    Console.WriteLine(fullQuote);
                    await ReplyAsync("Saved");
                }
            }
        }



        [Command("getQuote")]
        [Alias(new string[] { "gq" })]
        [Summary("Get a random quote from someone")]
        public async Task GetQuote()
        {

            if (checkForAvailability(Context.User.Username, "blacklist.txt", 0) == true)
            {
                await ReplyAsync("blocked");
                return;
            }

            string quote = getQuote();
            int l = quote.IndexOf("~");
            string newQuote = quote.Substring(0, l);
            if (newQuote.Contains(".com") || newQuote.Contains(".net") || newQuote.Contains(".org"))
            {
                await ReplyAsync(newQuote);
            }
            else
            {
                await ReplyAsync("```" + newQuote + "```");
            }
        }

        [Command("cardinfo")]
        [Alias(new string[] {"c"})]
        [Summary("Get the info of the card from the Yugioh Wikia")]
        public async Task CardInfo([Remainder] string card)
        {
            if (checkForAvailability(Context.User.Username, "blacklist.txt", 0) == true)
            {
                await ReplyAsync("blocked");
                return;
            }
            if (num != 0)
            {
                await ReplyAsync("Busy processing another card");
                return;
            }

            card = card.ToLower().ToAllFirstLetterInUpper();
            string user = Context.User.Username;
            num = 1;
            string convertedString, cardName, wikilink;
            convertedString = modifyStringEntry(card, 1);
            cardName = card;
            if (cardName.Contains("_"))
            {
                cardName = modifyStringEntry(cardName, 0);
            }

            wikilink = ("http://yugioh.fandom.com/wiki/" + convertedString);
            await ReplyAsync(Context.Message.Author.Mention);
            await ReplyAsync("Fetching the data for " + "***" + cardName + "***" + ", pls wait one moment");
            try
            {
                string orgin = getCardOrigin(wikilink);
                Console.WriteLine("Getting " + cardName + " card image");

                
                string cardPic = getCardImage(wikilink, orgin);
                Console.WriteLine("Getting " + cardName + " card text");
                checkInfo(wikilink);
                string cardText = organizeText(wikilink, cardName);
                await ReplyAsync(cardPic);
                await ReplyAsync(cardText);
                num = 0;
                

            }
            catch
            {
                await ReplyAsync("ERROR!");
                num = 0;
            }
        }

        private string organizeText(string wikilink, string cardName)
        {

            try
            {
                string orgin = getCardOrigin(wikilink);
                string nameText = "Name: ";

                //Check the Property of Card to determine if it's a Spell or Trap
                if (type[1].Contains("Trap") || (type[1].Contains("Spell") && property[0] == "True"))
                {
                    //Since it determine it's not a Spell or Trap, then the card must be a monster 
                    string categoryText = "Category: ";
                    string propertyText = "Property: ";

                    //Return the card text
                    string cardText = "```" + nameText + cardName + "\n"
                    + categoryText + type[1] + ", " + propertyText + property[1] + "\n"
                    + effect[1] + "```";
                    return cardText;


                }

                //If it's not spell or trap, then it automatically assume it's a monster
                else
                {

                    //Since it determine it's not a Spell or Trap, then the card must be a monster 
                    string categoryText = "Category: Monster Card";
                    string levelText = "Level: ";
                    string typeText = "Type: ";
                    string scaleText = "Scale: ";
                    string attributeText = "Attribute: ";
                    string attText = "ATK: ";
                    string defText = "DEF: ";

                    //Split the Att and Def into 2 different string
                    int l = stat[1].IndexOf("/");
                    string newStatAtt = stat[1].Substring(0, l);
                    string newStatDef = stat[1].Substring(l + 2);

                    //
                    if (type[1].Contains("Xyz"))
                    {
                        levelText = "Rank: ";
                    }


                    //If the monster is a Pendulum
                    if (type[1].Contains("Pendulum"))
                    {

                        //Output the message
                        string cardText = "```" + nameText + cardName + "\n"
                        + categoryText + ", " + levelText + level[1] + ", " + scaleText + scale[1] + "\n"
                        + attributeText + attribute[1] + "\n"
                        + typeText + type[1] + "\n" + "\n"
                        + effect[1] + "\n\n" + attText + newStatAtt + " " + defText + newStatDef + "```";
                        return cardText;

                    }
                    //If the monster is not a Pendulum
                    else
                    {

                        //Output the message
                        string cardText = "```" + nameText + cardName + "\n"
                        + categoryText + ", " + levelText + level[1] + "\n"
                        + attributeText + attribute[1] + "\n"
                        + typeText + type[1] + "\n" + "\n"
                        + effect[1] + "\n\n" + attText + newStatAtt + " " + defText + newStatDef + "```";
                        return cardText;

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error Getting Text");
                return null;
            }


        }

        private string getCardImage(string link, string orgin)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string YugiohPicture = null;
                    //X-Path
                    if (orgin == "Duel Monsters cards")
                    {
                        YugiohPicture = "//td/a/img";
                    }
                    else
                    {
                        YugiohPicture = "//div[3]/a/img";
                    }

                    //Verify and Get string from the site using the X-Path    
                    string PictureLink = NodeVerify(link, YugiohPicture, true);

                    //Download an image from the wikia
                    client.DownloadFileAsync(new Uri(PictureLink), "blank.png");
                    //Send to the Channel the picture that was just downloaded from the Wikia
                    string pictureDirectory = "blank.png";
                    
                    return PictureLink;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public static string NodeVerify(string wikilink, string xPath, Boolean image)
        {

            var web = new HtmlWeb();
            var doc = web.Load(wikilink);
            var textnode = doc.DocumentNode.SelectSingleNode(xPath);

            if (image == true)
            {

                string imagePath = textnode.Attributes["src"].Value;
                return imagePath;
            }
            else
            {
                return textnode.InnerText;
            }
        }

        private string getCardOrigin(string wikilink)
        {
            int counter = 1;
            try
            {
                string Orgin = "Duel Monsters cards";
                while (counter != 5)
                {
                    string YugiohOrgin = "//nav/div/div[1]/ul/li[" + counter + "]/a";
                    Orgin = NodeVerify(wikilink, YugiohOrgin, false);

                    
                    if (Orgin.Contains("Duel Monsters cards"))
                    {
                        counter = 6;
                        return Orgin;
                    }
                    else if (Orgin.Contains("Manga cards"))
                    {
                        counter = 6;
                        return Orgin;
                    }
                    else if (Orgin.Contains("Anime cards"))
                    {
                        counter = 6;
                        return Orgin;
                    }
                    counter++;
                }
                return Orgin;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        public void checkInfo(string wikilink)
        {
            int counter = 0;
            effect = new string[] { "False", "???? " };
            type = new string[] { "False", "???? " };
            attribute = new string[] { "False", "???? " };
            level = new string[] { "False", "???? " };
            scale = new string[] { "False", "???? " };
            stat = new string[] { "False", "???? " };
            property = new string[] { "False", "???? " };
            alignment = new string[] { "False", "???? " };

            string orgin = getCardOrigin(wikilink);


            try
            {
                if (orgin == "Duel Monsters cards")
                {
                    effect = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//td/table//tr[3]/td", false), 2) };
                }
                else if (orgin == "Manga cards")
                {
                    effect = new string[] { "True", NodeVerify(wikilink, "//td/div/div/div/p", false) };
                }
                else
                {
                    effect = new string[] { "True", NodeVerify(wikilink, "//td/div/div", false) };
                }
            }
            catch
            {
                effect = new string[] { "False", "????" };
            }



            while (counter != 20)
            {
                try
                {
                    string xPath = "//tr[" + counter + "]/th";

                    string analyze = NodeVerify(wikilink, xPath, false);
                    string analyze2 = modifyStringEntry(NodeVerify(wikilink, xPath, false), 3);


                    if (analyze == "Type" || analyze == "Types" || analyze == "Card type" || analyze2 == "Type" || analyze2 == "Types" || analyze2 == "Card type")
                    {
                        if (orgin == "Duel Monsters cards")
                        {
                            type = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                        }
                        else
                        {
                            if (counter == 1)
                            {
                                type = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[1]/td/p", false), 3) };
                            }
                            else
                            {
                                string typeText = "";
                                try
                                {
                                    int countTest = counter;
                                    for (int count = 1; count < 4; count++)
                                    {
                                        string[] type = new string[4];
                                        type[count] = NodeVerify(wikilink, "//tr[" + countTest + "]/td/div/ul/li[" + count + "]/a", false);
                                        //Console.WriteLine(countTest + " " + count);
                                        typeText += type[count] + " / ";
                                    }
                                }
                                catch
                                {
                                    int lenght = typeText.Length;
                                    typeText = typeText.Remove(lenght - 2, 1);
                                }
                                type = new string[] { "True", typeText };
                            }
                        }
                    }
                    else if (analyze.Contains("Attribute"))
                    {
                        if (orgin == "Duel Monsters cards")
                        {
                            attribute = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                        }
                        else
                        {
                            if (counter == 1)
                            {
                                attribute = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[1]/td/p", false), 3) };
                            }
                            else
                            {
                                attribute = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 4) };
                            }
                        }
                    }
                    else if (analyze.Contains("Level") || analyze.Contains("Rank"))
                    {
                        if (orgin == "Duel Monsters cards")
                        {
                            level = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                        }
                        else
                        {
                            level = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false).Substring(0, 3), 2) };
                        }
                    }
                    else if (analyze.Contains("Scale"))
                    {
                        if (orgin == "Duel Monsters cards")
                        {
                            scale = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                        }
                        else
                        {
                            scale = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 4) };
                            int lenght = scale[1].Length;
                            scale[1] = scale[1].Remove(lenght - 1, 1);
                        }
                    }
                    else if (analyze.Contains("ATK / DEF"))
                    {
                        stat = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                    }
                    else if (analyze.Contains("Property"))
                    {
                        property = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                    }
                    else if (analyze.Contains("Alignment"))
                    {
                        alignment = new string[] { "True", modifyStringEntry(NodeVerify(wikilink, "//tr[" + counter + "]/td", false), 2) };
                    }

                }
                catch
                {
                }
                counter++;
            }
        }
        public static string modifyStringEntry(string input, int choice)
        {
            //Replace _ with Spaces
            if (choice == 0)
            {
                StringBuilder sb = new StringBuilder(input);

                sb.Replace("_", " ");

                return sb.ToString();
            }
            //Replace Spaces with _
            if (choice == 1)
            {
                StringBuilder sb = new StringBuilder(input);

                sb.Replace(" ", "_");

                return sb.ToString();
            }
            //Delete first substring
            else if (choice == 2)
            {
                String newWord = input.Substring(1);
                return newWord;
            }
            //Delete Last substring
            else if (choice == 3)
            {
                int lenght = input.Length;
                String newWord = input.Remove(lenght - 1, 1);
                return newWord;
            }
            //Delete first and last substring
            else if (choice == 4)
            {
                String newWord = input.Substring(1);
                int lenght = newWord.Length;
                newWord = newWord.Remove(lenght - 1, 1);
                return newWord;
            }


            return null;
        }
        public Boolean checkForAvailability(string quote, string file, int option)
        {
            TextReader reader = new StreamReader(file);
            List<String> lines = new List<String>();
            String buffer = "";
            while ((buffer = reader.ReadLine()) != null)
            {
                lines.Add(buffer);
            }
            reader.Close();

            for (int index = 0; index < lines.Count(); index++)
            {

                if (lines[index].Contains(quote))
                {
                    if (option == 0)
                    {
                        Console.WriteLine(lines[index]);
                        return true;
                    }
                    else
                    {
                        deleteTextinTextFile(index);
                        return false;
                    }

                }
            }
            return false;
        }
        public void deleteTextinTextFile(int delete)
        {

            var file = new List<string>(System.IO.File.ReadAllLines("blacklist.txt"));
            file.RemoveAt(delete);
            File.WriteAllLines("blacklist.txt", file.ToArray());

        }
        public string getQuote()
        {
            string[] allLines = File.ReadAllLines("quote.txt");
            Random rnd1 = new Random();
            return allLines[rnd1.Next(allLines.Length)];
        }



    }
    public static class StringExtension
    {
        public static string ToAllFirstLetterInUpper(this string str)
        {
            var array = str.Split(' ');

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == "" || array[i] == " " || listOfArticles_Prepositions().Contains(array[i])) continue;
                array[i] = array[i].ToFirstLetterUpper();
            }
            return string.Join(" ", array);
        }

        private static string ToFirstLetterUpper(this string str)
        {
            return str?.First().ToString().ToUpper() + str?.Substring(1).ToLower();
        }

        private static string[] listOfArticles_Prepositions()
        {
            return new[]
            {
                "in","on","to","of","and","or","for","a","an","is"
            };
        }
    }
}
