# Zbyrach API

Zbyrach API is a backend for the Zbyrach App. It manages users, user settings, and articles. Also, Zbyrach API responsible for searching articles and sending emails with the last found articles according to the schedule.

## Swagger

[Zbyrach API](http://zbyrach-api.herokuapp.com/index.html) is an HTTP API that holds such entities:

* Accounts
* Articles
* MailingsSettings
* Tags
* Statistic

API uses token bases third-party authentication provided by Google.

![](https://github.com/alexmartyniuk/zbyrach-api/blob/master/docs/img/api-swagger.png?raw=true)

## Architecture

Internally Zbyrach API uses three dependant components:

* database - for storing own data
* medium.com - for finding articles by keywords
* PDF service - for converting HTML pages into PDF documents

To communicate to the Web UI in the real time is used SignalR.

![](https://github.com/alexmartyniuk/zbyrach-api/blob/master/docs/img/architecture-diagram.png?raw=true)

### Database

The database is managed by PostgreSQL engine and hosted in the Heroku cloud.

![](https://github.com/alexmartyniuk/zbyrach-api/blob/master/docs/img/database-schema.png?raw=true)

## medium.com

The [Medium.com](http://medium.com) site is used to find articles by keywords and get the content of the article. The technic is used to grab content from the site is similar to the web crawler.

## PDF service

[PDF service](http://zbyrach-pdf-service.herokuapp.com/index.html) is an internal service for Zbyrach that provides HTTP web API for converting and storing the PDF files for articles.

![](https://github.com/alexmartyniuk/zbyrach-api/blob/master/docs/img/pdf-service-swagger.png?raw=true)