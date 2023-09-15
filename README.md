# ExploreLocal Project Setup Guide

## Prerequisites

- **Database**: Download the project database from [this link](https://cutt.ly/OwjPXR0Y).
- **Project Files**: Download the ExploreLocal project files from GitHub using [this link](https://cutt.ly/mwkhMEb2).
- **Visual Studio**: Ensure you have Visual Studio 2019 or a later version installed on your system.

## Database Setup

1. Download the database file from the provided link.
2. Open SQL Server Management Studio (SSMS).
3. Connect to your SQL Server instance.
4. In SSMS, right-click on "Databases" in the Object Explorer, and select "Restore Database."
5. Choose "Device" and browse for the downloaded database file.
6. Click "OK" to restore the database.

## Project Setup

1. Open Visual Studio.
2. Open the ExploreLocal project that you downloaded from GitHub.
3. Make sure to configure your connection string in the Web.config file to match your database setup.
4. Open the "Models" folder and create your model. Ensure you've configured your entities and database connections according to your needs.
5. In the model class Tbl_Destination, add the following line at the top:

   ```csharp
   using System.Web.Mvc;
