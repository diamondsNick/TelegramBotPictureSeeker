using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V110.Debugger;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("6068290456:AAHa2MRUFjiTcL2lle1QXtCKlFS918sqTpA");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    bool isHelpInfoSeen = false;


    if (messageText == "/help")
    {
        Message helpMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "To find picture choose /find, then fill in thing that you want to find",
        parseMode: ParseMode.Html,
        disableNotification: true,
        cancellationToken: cancellationToken);
        isHelpInfoSeen = true;
    }

    if (messageText.StartsWith("/find")) //main part of a program
    {
        char[] TrimmingFinderArray = { '/', 'f', 'i', 'n', 'd' };
        string searchText = messageText.TrimStart(TrimmingFinderArray); //making our request


        if (searchText != "")//protect bot from glitching
        {
            
            //Message helpMessage = await botClient.SendTextMessageAsync(
            //chatId: chatId,
            //text: searchText,
            //parseMode: ParseMode.Html,        debug only
            //disableNotification: true,
            //cancellationToken: cancellationToken);



            IWebDriver driver = new ChromeDriver();

            driver.Manage().Window.Position = new Point(-5000, -5000);//window moves so you can't see it

            driver.Navigate().GoToUrl("https://www.google.ru/imghp?hl=en&ogbl"); //enters Google Pictures

            bool IsSearchBarFound = false ; //boolean variable to check if search bar was found
            bool IsSearchBarNotFound = false ;

            try
            {
                int i = 0; //counter
                while ((IsSearchBarNotFound == false && IsSearchBarFound == false) != false || i < 10) //if both of them are false, then it stops
                {
                    
                    IWebElement searchBar;
                    try 
                    {
                        
                        searchBar = driver.FindElement(By.XPath("//textarea[@type='search']"));
                        searchBar.SendKeys(searchText);
                        searchBar.SendKeys(Keys.Enter);

                        IsSearchBarFound = true;
                        i = 10;
                    }
                    catch 
                    { 
                        Console.WriteLine("Error: cant find search bar");
                        ++i;
                        if(i >= 10)
                        {
                            IsSearchBarNotFound = true;
                        }

                    }
                }
            }
            catch //if search bar wasn't found
            {
                Console.WriteLine("Error: can't access search bar");
                driver.Close();

                Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error accured, please try again later",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            }

            bool isPictureWasFound = false;
            bool isPictureWasNotFound = false ;

            try //finding picture on the site to click
            {
                int i = 0;
                while ((isPictureWasNotFound == false && isPictureWasFound == false) != false || i<10)
                {
                    try
                    {
                        IWebElement pictureElement = driver.FindElement(By.XPath("//*[@id=\"islrg\"]/div[1]/div[1]/a[1]/div[1]"));// finds picture

                        new Actions(driver)
                              .Click(pictureElement)
                            .Perform();

                        isPictureWasFound = true;
                        i = 10;

                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error: cant find picture");
                        ++i;
                        if(i >= 10)
                        {
                            isPictureWasNotFound = true;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error: can't find picture on the site");
                driver.Close();

                Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error accured, please try again later",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            }

            try //finding three points element to press, so we can get acces to share button
            {
                int i = 0;
                bool isThreePointsFound = true;
                bool isThreePointsNotFound = false ;
                while ((isThreePointsNotFound == false && isThreePointsFound == false) != false || i<10)
                {
                    try
                    {
                        IWebElement threePointsElement = driver.FindElement(By.XPath("//*[@id=\"Sva75c\"]/div[2]/div/div[2]/div[2]/div[2]/c-wiz/div/div/div/div[2]/div/div[2]/div[2]/div/a/div"));


                        new Actions(driver)//clicks on menu that opens options for an image
                              .Click(threePointsElement)
                            .Perform();

                        isThreePointsFound = true;
                        i = 10;
                    }
                    catch
                    {
                        Console.WriteLine("Error: cant find three points");
                        ++i;
                        if(i >= 10)
                        {
                            isThreePointsNotFound = true;
                        }
                    }
                }
            }
            catch 
            {
                Console.WriteLine("Error: can't find three points element on the site");
                driver.Close();

                Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error accured, please try again later",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            }

            try
            {
                bool isShareButtonFound = false;
                bool isShareButtonNotFound = false;
                int i = 0;
                while ((isShareButtonNotFound == false && isShareButtonFound == false) != false || i < 10)
                {
                    try //finding share button to press
                    {
                        IWebElement shareButton = driver.FindElement(By.XPath("//*[@id=\"Sva75c\"]/div[2]/div/div[2]/div[2]/div[2]/c-wiz/div/div/div/div[2]/div/div[2]/div[2]/div/div/div"));


                        new Actions(driver)
                              .Click(shareButton)
                            .Perform();
                        isShareButtonFound = true;
                        i = 10;

                    }
                    catch
                    {
                        Console.WriteLine("Error: cant find share button");
                        ++i;
                        if (i >= 10)
                        {
                            isShareButtonNotFound = true;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error: can't find share button");
                driver.Close();

                Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error accured, please try again later",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            }

            try//finding link to photo
            {
                string linkToPhoto = driver.FindElement(By.XPath("//*[@id=\"yDmH0d\"]/div[6]/div/div[2]/span/div/div/div[4]/a")).GetAttribute("href");

                Console.WriteLine(linkToPhoto);//debug only

                bool isPictureLinkFound = false;
                bool isPictureLinkNotFound = false;
                int i = 0;
                while((isPictureLinkNotFound != true && isPictureLinkFound == false) != false || i<10)
                {
                    //Console.WriteLine(i); debug only
                    try
                    {
                        Message messageSendsPicture = await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: linkToPhoto,
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);

                        isPictureLinkFound = true;

                        i = 10;

                        driver.Close();
                    }
                    catch
                    {
                        Console.WriteLine("Error: cant send link to photo");

                        i++;
                        if(isPictureLinkFound == false) 
                        {
                            try
                            {
                                Message messageSendsPicture = await botClient.SendPhotoAsync(
                                chatId: chatId,
                                photo: linkToPhoto,
                                parseMode: ParseMode.MarkdownV2,
                                cancellationToken: cancellationToken);
                                isPictureLinkFound = true;
                            }
                            catch 
                            { 
                                
                            }

                        }

                        if (i >= 10) 
                        {
                            isPictureLinkNotFound = true;
                            driver.Close();

                            Message MessageOfUnaccesableLinkToPicture = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Error accured, please try again",
                            parseMode: ParseMode.Html,
                            disableNotification: true,
                            cancellationToken: cancellationToken);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error: can't find link to photo");
                driver.Close();

                Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Error accured, please try again",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: cancellationToken);
            }
        }
        else
        {
            Message helpMessage = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: "Error, please enter a real request",
             parseMode: ParseMode.Html,
             disableNotification: true,
             cancellationToken: cancellationToken);
        }
    }
    else if (isHelpInfoSeen == false)
    {
        Message helpMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "No command found, please write /help",
        parseMode: ParseMode.Html,
        disableNotification: true,
        cancellationToken: cancellationToken);
    }

}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}