# POP Blog

This is a lightweight blog app that uses ASP.NET Core, SQL Server and Azure storage for podcast files.

## Roadmap

The intention for v1 is to do simple blog posts, RSS, optional attachments, storage of podcast files in Azure storage. It will be done when it has been placed in a several production applications.

## About this project

This is based roughly on my personal blog, which was first posted in the late oughts as an ASP.NET Webforms app, eventually converted to MVC and then ASP.NET Core after that. It's a little crusty, but it's super simple. It's not a SPA because the public side is all text, and the admin side is too simple to bother. I open sourced it because I needed a basic aggregator for a few projects, and wanted to use a Razor class library and just override the layout with the appropriate header and CSS to skin it as necessary.

## Getting started

Run the database project by passing your database connection string as the only parameter, and dbup will magically make the schema current.

The MVC project can be used as the class library with which to feed your web app. Override views with your own markup in the web project, along with your assets and such. Even better, go the package route.

## Go the package route

The latest CI build NuGet package feed for the MVC project is here:  
https://www.myget.org/F/popblog/api/v3/index.json 

[![Build Status](https://dev.azure.com/popw/POP%20Blog/_apis/build/status/POP%20Blog-ASP.NET%20Core-CI?branchName=main)](https://dev.azure.com/popw/POP%20Blog/_build/latest?definitionId=6&branchName=main)

The latest CI Build npm package feed for the meager script needs is here:  
https://www.myget.org/F/popblog/npm/  
You'll need to do copying of the various files, which the project views expect to be in `/lib/{packagename}`.
