# POP Blog

This is a lightweight blog app that uses ASP.NET Core, SQL Server and Azure storage for podcast files.

## About this project

This is based roughly on my personal blog, which was first posted in the late oughts as an ASP.NET Webforms app, eventually converted to MVC and then ASP.NET Core after that. It's a little crusty, but it's super simple. I open sourced it because I needed a basic aggregator for a few projects, and wanted to use a Razor class library and just override the layout with the appropriate header and CSS to skin it as necessary.

## Getting started

Run the database project by passing your database connection string as the only parameter, and dbup will magically make the schema current.

The MVC project can be used as the class library with which to feed your web app. Override views with your own markup in the web project, along with your assets and such.
