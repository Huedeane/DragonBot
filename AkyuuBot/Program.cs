using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;

namespace AkyuuBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start();

        private DiscordClient _client;
        private string[] effect = new string[] { "False", "???? " };
        private string[] type = new string[] { "False", "???? " };
        private string[] attribute = new string[] { "False", "???? " };
        private string[] level = new string[] { "False", "???? " };
        private string[] scale = new string[] { "False", "???? " };
        private string[] stat = new string[] { "False", "???? " };
        private string[] property = new string[] { "False", "???? " };
        private string[] alignment = new string[] { "False", "???? " };
        private int num = 0;

        public void Start()
        {

            _client = new Discord.DiscordClient(x =>
            {
                x.AppName = "Akyuu";
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            var token = "MjE2Nzg4NjM0NDE2NzA5NjMz.Cq-TDg.lrYY8OiW1j7r4oFH9eF8bjewxM8";

            _client.UsingCommands(x =>
            {
                x.PrefixChar = '~';
                x.HelpMode = HelpMode.Public;
                x.AllowMentionPrefix = true;
            });

            CreateCommands();

            _client.ExecuteAndWait(async () =>
            {
                try
                {
                    await _client.Connect(token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Token is Invalid.");
                    Console.WriteLine(ex);
                    Console.ReadKey();
                    return;
                }
            });

        }

        public void CreateCommands()
        {
            var cService = _client.GetService<CommandService>(true);
            cService.CreateCommand("ping")
                .Description("Respect for Harambe")
                .Do(async (e) =>
                {
                    if (checkForAvailability(e.User.Name, "blacklist.txt", 0) == true)
                    {
                        await e.Channel.SendMessage("blocked");
                        return;
                    }
                    await e.Channel.SendMessage("pong");
                });           
            cService.CreateCommand("cardinfo")
                .Alias(new string[] { "c" })
                .Description("Get the info of the card from the Yugioh Wikia")
                .Parameter("card", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    if (checkForAvailability(e.User.Name, "blacklist.txt", 0) == true)
                    {
                        await e.Channel.SendMessage("blocked");
                        return;
                    }
                    if (num != 0)
                    {
                        await e.Channel.SendMessage("Busy processing another card");
                        return;
                    }
                    num = 1;
                    string convertedString, cardName, wikilink;

                    convertedString = modifyStringEntry(e.GetArg("card"), 1);
                    cardName = e.GetArg("card");

                    if (cardName.Contains("_"))
                    {
                        cardName = modifyStringEntry(cardName, 0);
                    }

                    wikilink = ("http://yugioh.wikia.com/wiki/" + convertedString);
                    await e.Channel.SendMessage("@" + e.User.Name);
                    await e.Channel.SendMessage("Fetching the data for " + "***" + cardName + "***" + ", pls wait one moment");
                    try
                    {
                        string orgin = getCardOrigin(wikilink);
                        Console.WriteLine("Getting " + cardName + " card image");

                        string cardPic = getCardImage(wikilink, orgin);
                        Console.WriteLine("Getting " + cardName + " card text");
                        checkInfo(wikilink);
                        string cardText = organizeText(wikilink, cardName);
                        await e.Channel.SendMessage(cardPic);
                        await e.Channel.SendMessage(cardText);
                        num = 0;

                    }
                    catch
                    {
                        await e.Channel.SendMessage("ERROR!");
                        num = 0;
                    }
                });
            cService.CreateCommand("block")
                .Alias(new string[] { "b" })
                .Parameter("name", ParameterType.Unparsed)
                .Do((e) =>
                {

                        if (e.User.Id == 146048024433393664 || e.User.Id == 120726928918315008)
                        {
                            string fullName = e.GetArg("name");

                            if (checkForAvailability(fullName, "blacklist.txt", 0) == true)
                            {
                                e.Channel.SendMessage(fullName + " is already blocked");
                            }
                            else
                            {
                                TextWriter file = new StreamWriter("blacklist.txt", true);
                                e.Channel.SendMessage(e.GetArg("name") + " has been blocked");
                                file.WriteLine(fullName);
                                file.Close();
                            }
                        }
                        else
                        {
                            e.Channel.SendMessage("You are not an admin");
                        }
                   
                });
            cService.CreateCommand("unblock")
                .Alias(new string[] { "ub" })
                .Parameter("name", ParameterType.Unparsed)
                .Do((e) =>
                {
                    if (e.User.Id == 146048024433393664 || e.User.Id == 120726928918315008)
                    {
                        string fullName = e.GetArg("name");

                        if (checkForAvailability(fullName, "blacklist.txt", 0) == true)
                        {
                            checkForAvailability(fullName, "blacklist.txt", 1);
                            e.Channel.SendMessage(fullName + " has been unblocked");

                        }
                        else
                        {
                            e.Channel.SendMessage(fullName + " has not been blocked");
                        }
                    }
                    else
                    {
                        e.Channel.SendMessage("You are not an admin");
                    }

                });
            cService.CreateCommand("saveQuote")
                .Alias(new string[] { "sq" })
                .Description("quote what wrote to be output later")
                .Parameter("quote", ParameterType.Unparsed)
                .Do((e) =>
                {
                    if (checkForAvailability(e.User.Name, "blacklist.txt", 0) == true)
                    {
                        e.Channel.SendMessage("blocked");
                        return;
                    }
                    string fullQuote = e.GetArg("quote") + " ~" + e.User.Name;
                    if (e.GetArg("quote").Contains("~"))
                    {
                        e.Channel.SendMessage("Error, your message contains a ~");
                    }
                    else
                    {
                        if (checkForAvailability(fullQuote, "quote.txt", 0) == true)
                        {
                            e.Channel.SendMessage("Text already exist");
                        }
                        else
                        {
                            TextWriter file = new StreamWriter("quote.txt", true);
                            file.WriteLine(fullQuote);
                            file.Close();
                            Console.WriteLine(fullQuote);
                            e.Channel.SendMessage("Saved");
                        }
                    }



                });
            cService.CreateCommand("getQuote")
                .Alias(new string[] { "gq" })
                .Description("Get a random quote from someone")
                .Parameter("person", ParameterType.Unparsed)
                .Do((e) =>
                {
                    if (checkForAvailability(e.User.Name, "blacklist.txt", 0) == true)
                    {
                        e.Channel.SendMessage("blocked");
                        return;
                    }
                    
                    string quote = getQuote("~" + e.GetArg("person"));
                    int l = quote.IndexOf("~");
                    string newQuote = quote.Substring(0, l);
                    Console.WriteLine(e.User.Name);
                    Console.WriteLine(newQuote);
                    if (newQuote.Contains(".com") || newQuote.Contains(".net"))
                    {
                        e.Channel.SendMessage(newQuote);
                    }
                    else if (newQuote.Contains("/tts")) {
                        e.Channel.SendTTSMessage(newQuote);
                    }
                    else
                    {
                        e.Channel.SendMessage("```" + newQuote + "```");
                    }
                    
                    
                });
            cService.CreateCommand("setname")
                    .Alias("sn")
                    .Description("Give the bot a new name. (Creator Only)")
                    .Parameter("new_name", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (e.GetArg("new_name") == null) return;
                        if (e.User.Id == 146048024433393664)
                        {
                            await _client.CurrentUser.Edit("", e.GetArg("new_name")).ConfigureAwait(false);
                        }
                        
                    });
                    

        }

        private string getCardOrigin(string wikilink)
        {
            int counter = 1;
            try
            {
                string Orgin = "Duel Monsters cards";
                while (counter != 5)
                {
                    string YugiohOrgin = "//nav/div/div/ul/li[" + counter + "]/a";
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
            catch
            {
            }
            return null;
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
                    /*
                    //Download an image from the wikia
                    client.DownloadFileAsync(new Uri(PictureLink), "blank.png");
                    //Send to the Channel the picture that was just downloaded from the Wikia
                    string pictureDirectory = "blank.png";
                    */
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
        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{ e.Severity}] [{e.Source}] {e.Message}");
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
        public string getQuote(string name)
        {
            int amountOfQuote = 0;
            int lineOfQuote = 0;
            String buffer = "";
            List<int> lineInfo = new List<int>();
            var file = new StreamReader("quote.txt");

            while (!file.EndOfStream)
            {
                lineOfQuote++;
                if (file.ReadLine().Contains(name))
                {
                    lineInfo.Add(lineOfQuote);
                    amountOfQuote++;


                }
            }
            Random rnd = new Random();
            int rndNum = rnd.Next(0, lineInfo.Count() + 1);
            if (rndNum >= lineInfo.Count())
            {
                rndNum--;
            }
            int rndQuote = lineInfo[rndNum];
            List<String> lines = new List<String>();
            TextReader file2 = new StreamReader("quote.txt");
            while ((buffer = file2.ReadLine()) != null)
            {
                lines.Add(buffer);
            }
            file2.Close();
            return lines[rndQuote - 1];
        }

    }


}

