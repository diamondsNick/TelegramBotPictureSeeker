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


        if (searchText != "")//protects bot from glitching
        {
            
            //Message helpMessage = await botClient.SendTextMessageAsync(
            //chatId: chatId,
            //text: searchText,
            //parseMode: ParseMode.Html,        //debug only
            //disableNotification: true,
            //cancellationToken: cancellationToken);



            IWebDriver driver = new ChromeDriver();

            //driver.Manage().Window.Position = new Point(-5000, -5000);//window moves so you can't see it

            driver.Navigate().GoToUrl("https://www.google.ru/imghp?hl=en&ogbl"); //enters Google Pictures

            bool IsSearchBarFound = false ; //boolean variable to check if search bar was found

            try //finding search bar
            {            
                IWebElement searchBar;
                //searchBar = driver.FindElement(By.XPath("//textarea[@type='search']"));
                searchBar = driver.FindElement(By.CssSelector("#APjFqb"));
                searchBar.SendKeys(searchText);
                searchBar.SendKeys(Keys.Enter);

                IsSearchBarFound = true ;
            }
            catch //if search bar wasn't found
            {
                Console.WriteLine("Error: can't access search bar");//printing message in console

                //Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                //chatId: chatId,
                //text: "Error accured, please try again later",//messaging user
                //parseMode: ParseMode.Html,
                //disableNotification: true,
                //cancellationToken: cancellationToken);
            }

            bool isPictureWasFound = false;

            if (IsSearchBarFound)
            {
                try //finding picture on the site to click
                {

                    //IWebElement pictureElement = driver.FindElement(By.XPath("//*[@id=\"islrg\"]/div[1]/div[1]/a[1]/div[1]"));// alternative
                    IWebElement pictureElement = driver.FindElement(By.CssSelector("#islrg > div.islrc > div:nth-child(2) > a.wXeWr.islib.nfEiy > div.bRMDJf.islir > img"));
                    new Actions(driver)
                            .Click(pictureElement)
                        .Perform();

                    isPictureWasFound = true;

                }
                catch
                {
                    Console.WriteLine("Error: can't find picture on the site");

                    //Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                    //chatId: chatId,
                    //text: "Error accured, please try again later",
                    //parseMode: ParseMode.Html,
                    //disableNotification: true,
                    //cancellationToken: cancellationToken);
                }
            }

            bool isThreePointsFound = false;
            if (isPictureWasFound)
            {
                try //finding three points element to press, so we can get acces to share button
                {

                    IWebElement threePointsElement = driver.FindElement(By.CssSelector("#Sva75c > div.DyeYj > div > div.dFMRD > div.pxAole > div.tvh9oe.BIB1wf > c-wiz > div > div > div > div:nth-child(2) > div > div > div.Ox7icf > div:nth-child(2) > div > a > div > svg"));
                    //#Sva75c > div.DyeYj > div > div.dFMRD > div.pxAole > div.tvh9oe.BIB1wf > c-wiz > div > div > div > div:nth-child(2) > div > div > div.Ox7icf > div:nth-child(2) > div > a > div > svg
                    //#Sva75c > div.DyeYj > div > div.dFMRD > div.pxAole > div.tvh9oe.BIB1wf > c-wiz > div > div > div > div:nth-child(2) > div > div > div.Ox7icf > div:nth-child(2) > div > a > div
                    new Actions(driver)//clicks on menu that opens options for an image
                          .Click(threePointsElement)
                        .Perform();
                    isThreePointsFound = true;
                }
                catch
                {
                    Console.WriteLine("Error: can't find three points element on the site");

                    //Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                    //chatId: chatId,
                    //text: "Error accured, please try again later",
                    //parseMode: ParseMode.Html,
                    //disableNotification: true,
                    //cancellationToken: cancellationToken);
                }
            }
            bool isShareButtonFound = false;

            if (isThreePointsFound == true) 
            {
                try 
                { 

                    IWebElement shareButton = driver.FindElement(By.CssSelector("#Sva75c > div.DyeYj > div > div.dFMRD > div.pxAole > div.tvh9oe.BIB1wf > c-wiz > div > div > div > div:nth-child(2) > div > div > div.Ox7icf > div:nth-child(2) > div > div > div:nth-child(1) > a"));
                    new Actions(driver)
                            .Click(shareButton)
                        .Perform();
                    isShareButtonFound = true;
                }
                catch
                {
                    Console.WriteLine("Error: can't find share button");

                    //Message MessageOfUnaccesableSearchBar = await botClient.SendTextMessageAsync(
                    //chatId: chatId,
                    //text: "Error accured, please try again later",
                    //parseMode: ParseMode.Html,
                    //disableNotification: true,
                    //cancellationToken: cancellationToken);
                }
            }


            if (isShareButtonFound == true)
            {
                try
                {//finding link to photo


                    //bool isPictureLinkFound = false;
                    //Console.WriteLine(i); debug only'

                    string linkToPhoto = driver.FindElement(By.ClassName("c1AlVc")).GetAttribute("href");
                    Console.WriteLine(linkToPhoto);//debug only

                    Message messageSendsPicture = await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: linkToPhoto,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                    driver.Close();
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
    else if (isHelpInfoSeen == false) //blocks this information if /help send or else it will show both messages in a same time
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