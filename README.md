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

## Upload size limits

If you plan to use the podcast serving mechanism and run the app on an IIS-based service (including Windows-based Azure App Services), you'll need to figure out how to raise the request size limit in old-fashioned web.config. [This answer on StackOverflow](https://stackoverflow.com/a/47112438/99897) should help. This is necessary to upload large files via the admin.

## What's in `appsettings.json`?

* `PopBlogConnectionString`: The connection string to your database. For local dev, use something like `server=localhost;Database=popblog;Trusted_Connection=True;`.
* `ReCaptchaSiteKey` and `ReCaptchaSecretKey`: You'll need these from [Google reCAPTCHA](https://www.google.com/recaptcha) to help avoid comment spam.
* `BlogTitle`: This appears after a hyphen on all of the page `<title>` tags.
* `BlogDescription`: This appears in the `<h1>` of the blog home page, and in front of the hyphen on the home page `<title>`.
* `TimeZone`: And integer represnting the time zone you want all of the times to be set for. For example, `-5` is US Eastern Time. It will adjust during Daylight Saving.

If you're using this to serve a podcast, you're going to need this stuff, too. Don't yell at me, Apple wants this stuff:

* `StorageConnectionString`: The connection string to your Azure Storage Account where the blobs go. Use `UseDevelopmentStorage=true` for the local storage emulator.
* `StorageContainerName`: The container in your blob storage where you're gonna drop your podcast masterpieces. Make sure the container is publicly accessible, or your users will be `403`'d.
* `StorageAccountBaseUrl`: The base URL for where your blob container is. For local, it's probably `http://127.0.0.1:10000/devstoreaccount1`, for a production location, it's something like `https://[storageaccountname].blob.core.windows.net`. **Do not** include the trailing slash.
* `Language`: The two-letter ISO 639 code for the language of your podcast. Defaults to `en`.
* `FeedImageUrl`: Location of your feed image.
* `ItunesCategory`: From [Apple's list](https://help.apple.com/itc/podcasts_connect/#/itc9267a2f12) of categories.
* `ItunesExplicit`: Boolean to let Apple know if you use naughty words.