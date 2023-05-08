# TelegramBotPictureSeeker

REMINDER: THIS CODE IS NOT AN EXAMPLE HOW TO WRITE YOUR CSHARP CODE AND BOT, because it only works with SLOW pcs and NOT with FAST ones.(idk why it is working like that)

Hello! This is a bot that finds pictures on the Internet and returns picture back to the user. This project is working with Chrome and uses ChromeDriver.
Parsing part was made with Selenium of version 4.9.0.
Currently this code uses ChromeDriver for version 112.
As for the usability of the bot on fast pcs, in debugger code works fine, but in a real work it's opening a new tab.
For ide I used Microsoft Visual Studio Professional 2022 (version 17.5.4).

Code information:
To make this code work you need to check version of your Chrome browser on "chrome://version/" and then download and paste correct wersion of ChromeDriver in a project folder and update Selenium if needed.
To move browser window from the screen you need to remove "//" in "//driver.Manage().Window.Position = new Point(-5000, -5000);"
