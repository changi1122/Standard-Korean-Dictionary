# Standard-Korean-Dictionary

This repo contains Universal Windows Platform (UWP) App that can connect to the Standard Korean dictionary and search and output the results. These codes were created with the Universal Windows Platform templates available in Visual Studio, and are designed to run on Windows 10(Build 16299 or later) desktop, that support the Universal Windows Platform. I plan to port to a headless browser like PhantomJS.

## Structure
The analysis algorithm takes HTML from the Standard Korean dictionary search web page, selects the necessary information, and stores it in the struct for the word.

## Universal Windows Platform development
Universal Windows Platform App project uses C# and requires Visual Studio 2017 or higher and the Windows Software Development Kit (SDK) version 16299 for Windows 10.

HTML AnalysisLogic project uses C# and requires Visual Studio and Windows Presentation Foundation (WPF) Development Kit.
