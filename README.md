# Homework Tracker Server

![GitHub commit activity](https://img.shields.io/github/commit-activity/m/CoPokBl/HomeworkTrackerServer?label=Commit%20Frequency&style=for-the-badge)  
![GitHub last commit](https://img.shields.io/github/last-commit/CoPokBl/HomeworkTrackerServer?style=for-the-badge)  
![GitHub all releases](https://img.shields.io/github/downloads/CoPokBl/HomeworkTrackerServer/total?style=for-the-badge)  

## Development
> **At this point this project is very old.**  
> This was me learning how to make web APIs, don't judge the terrible code.   
> You can check out my much more advances projects on [my profile](https://github.com/CoPokBl).

I am probably finished adding features to this so for now it will only be updated to update dependencies and fix any bugs I come across.
Still, feel free to contribute if you really want to.

## So what is this
This is an API for keeping track of your homework, I will be making clients that support this server so that you can save your homework and access it on multiple devices.

## Background
Yeah, there are probably alternatives, but I am making this to learn more about APIs and how they work, this is NOT a commercial project. I may stop working on this at any time if I get bored and I don't have any goals beyond making it fully functional. I will add features as I see fit or as I want them, you can suggest features, but I won't necessarily add them.

## Hosting
I will be keeping the latest working release of this project hosted at *http://homeworktrack.serble.net:9898/* or *https://homeworktrack.serble.net:9897/* for HTTPS
 
## Contact me
I prefer to use Discord for communication, you can DM me at @copokbl if you have any questions surrounding this project.

## Clients

> ### [Homework Tracker Unity Client](https://github.com/CoPokBl/HomeworkTrackerUnityClient)
> This client is a client I made in the Unity game engine. This client is quite slow and laggy and I made it because at that time I didn't know any front-end frameworks. It has all the basic features that you would expect, it can manage tasks, manage accounts, log in, and register. There are also some really crappy themes.

If you'd like to have a client you made added here, DM me on [Discord](https://discord.com) (@copokbl)

## FAQ

Q: Does it give you a notification when a task is due?  
*A: This is a server, clients might but this doesn't. Although this server doesn't really implement fancy stuff that allows this to happen easily.*

Q: Why is this helpful?  
*A: If you have multiple computers and want to have a list of to-do tasks sync across them then this is kinda useful.*

Q: What data does it collect  
*A: It saves the username you provide, the SHA256 hash of the password you provide (Meaning it doesn't know your password), your account creation date, and all of your tasks. It also will have your user agent in the log files if logging is set to debug.*

Q: I found a bug, what do I do?  
*A: Open an issue in the issues tab in GitHub.*

Q: How do I suggest stuff?  
*A: Open an issue in the issues tab in GitHub.*

Q: How do I use the API?  
*A: Check out the [wiki](https://github.com/CoPokBl/HomeworkTrackerServer/wiki)*
