# Jira Time Logger

## Set up instructions

No set up is required.
After starting the console app you will be asked for your Jira credentials. 

## Saving credentials

If you do not wish to type your username and password every time then you can generate an API token and create a credentials file.

### Create an API token

<https://id.atlassian.com/manage/api-tokens>

### Create a credentials file

Create a new file at c:\secure\jira-api-credentials.json in the following format

```
{
  "username": "[youremail]@riveragency.com",
  "password": "[apikey]"
}
```

## Finding the sprint id

The sprint id can be found in the URL when looking at the sprint board. 
For example,
/secure/RapidBoard.jspa?rapidView=13&selectedIssue=JGL-486&**sprint=99**